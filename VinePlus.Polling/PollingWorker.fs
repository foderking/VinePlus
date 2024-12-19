namespace VinePlus.Polling

open System
open System.Collections.Generic
open System.Linq
open System.Net.Http
open System.Threading
open System.Threading.Tasks
open Comicvine.Core
open Comicvine.Core.Parsers
open VinePlus.Database
open FSharp.Control
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.EntityFrameworkCore
open Microsoft.FSharp.Collections

type Couple = { Data: Thread; Pages: seq<int> }

type Result<'T> =
  {
    New: System.Collections.Generic.List<'T>
    Update: System.Collections.Generic.List<'T>
  }
with
  static member Create(newItem: seq<'T>, updateItem: seq<'T>) =
    {
      New    = List(newItem)
      Update = List(updateItem)
    }
  member this.Extend(other: Result<'T>) =
    this.New.AddRange(other.New)
    this.Update.AddRange(other.Update)

type PollingWorker(logger: ILogger<PollingWorker>, scopeFactory: IServiceScopeFactory) =
  inherit BackgroundService()
  // implements comparison for posts
  let postComparer =
    {
      new IEqualityComparer<Post> with
        member _.Equals(x, y) =
          x.Id = y.Id
        member _.GetHashCode(x) =
          match x.Id.Substring(0, 2) with
          | "Cx" ->
            int x.Id[2..]
          | "Tx" ->
            x.Id[2..]
            |> int
            |> (*) -1
          | _ -> failwith "invalid post Id"  
    }
    
  let Assert cond message =
    if not cond then
      failwith message

  let getThreadData(db: ComicvineContext)(thread: Thread) = task {
    let! dbResult =
      db
        .Threads
        .AsNoTracking()
        .FirstOrDefaultAsync(fun t -> t.Id = thread.Id)
        
    if obj.ReferenceEquals(dbResult, null) then
      return
        Result<Couple>.Create(
          newItem    = [|{Data = thread; Pages = seq{ 1..thread.LastPostPage }}|],
          updateItem = [||]
        )
    elif dbResult.LastPostNo < thread.LastPostNo then
      return 
        Result<Couple>.Create(
          newItem    = [||],
          updateItem = [|{Data = thread; Pages = seq{ dbResult.LastPostPage .. thread.LastPostPage }}|]
        )
    else
      return 
        Result<Couple>.Create(
          newItem    = [||],
          updateItem = [||]
        )
    }
  
  /// Removes duplicate polls OP since they appear on every page
  let filterPoll(post: Post)(page: int) =
    (page > 1 && post.PostNo > (page*50-50) && post.PostNo <= (page*50))||
    (page = 1 && post.PostNo >= 0 && post.PostNo <= 50)
    
  let getPostDataFromNewThread(newPosts: seq<Post>)(page: int) =
     // every post in the thread would be marked as a new post
    Result<Post>.Create(
      newItem = newPosts.Where(fun post -> filterPoll post page),
      updateItem = Seq.empty
    )

  let getPostDataFromUpdatedThread(db: ComicvineContext)(thread: Thread)(parsedPosts: seq<Post>)(page: int) = task {
     (**
        for a visualization of how new, deleted, and updated post related, a venn diagram is available here:
        https://web.archive.org/web/20230610183122/https://ibb.co/02Vx8Pk
     *)
    let result = Result<Post>.Create(Seq.empty, Seq.empty)
    // filter the OP since it appears every page for blog, polls and co.
    let parsedPosts =
      parsedPosts
      |> Seq.filter (fun each -> not(page <> 1 && each.PostNo = 0)) 
    // get the posts from a particular pge in the thread from db
    let! dbPosts =
      db
        .Posts
        .AsNoTracking()
        .Where(fun each -> each.ThreadId = thread.Id)// && (filterPoll each page))
        .Where(fun post -> // each.ThreadId = thread.Id && (filterPoll each page))
          (page > 1 && post.PostNo > (page*50-50) && post.PostNo <= (page*50))||
          (page = 1 && post.PostNo >= 0 && post.PostNo <= 50)
        )
        .ToArrayAsync()
    // deleted posts are posts that are present in the db, but not parsed from comicvine
    let deletedPosts = dbPosts.Except(parsedPosts, postComparer)
    // new posts are posts that are parsed in comicvine, but not present in db
    let newPosts = parsedPosts.Except(dbPosts, postComparer)
    // edited posts are posts that are present in both db and parsed from comicvine,...
    // but also have the `IsEdited` property set to true.
    // These posts should be constantly updated
    let editedPosts = parsedPosts.Intersect(dbPosts, postComparer).Where(fun x -> x.IsEdited)
    
    // add new posts
    result.New.AddRange(newPosts)
    // add deleted posts
    deletedPosts
    |> Seq.map (fun each -> { each with IsDeleted = true })
    |> result.Update.AddRange
    // add edited posts
    result.Update.AddRange(editedPosts)
    
    return result
  }
  
  let getHtml (ct: CancellationToken) (page: int) path =
    task {
      use querystring =
        new FormUrlEncodedContent(Dictionary[KeyValuePair("page", page |> string)])
      let! q = querystring.ReadAsStringAsync(ct)
      let client = new HttpClient()
      client.BaseAddress <- Uri("https://comicvine.gamespot.com")
      return! client.GetStringAsync($"{path}?{q}")
    }
    
  let parsePostsInBatches queuedThreads batchSize postsParser=
    queuedThreads
    // make request page by page
    |> Seq.collect (fun abc -> seq {
      for page in abc.Pages ->
        abc.Data, page
    })
    // group to batches of `batchSize` items
    |> Seq.mapi    (fun id data -> id,data)
    |> Seq.groupBy (fun (id, _) -> id / batchSize)
    // parsed posts from vine in parallel within each batch
    |> Seq.map(fun (_, batchedThreadsTuple) ->
      batchedThreadsTuple
      |> Seq.map(fun (_, (thread,page)) ->
        postsParser page thread
        |> Async.map (fun x -> x,thread, page)
        |> Task.ofAsync
      )
      |> Task.WhenAll
    )

  let rec parsePostsInPage page thread = async {
    try 
      let! parsedPosts = PostParser.ParsePage page thread.Thread.Link |> Async.ofTask
      return parsedPosts
     with
    | :? AggregateException as ex
      when ex.Message = "One or more errors occurred. (Response status code does not indicate success: 404 (Not Found).)" ->
        printfn "- deleted thread %s" thread.Thread.Link
        return Seq.empty
    | :? AggregateException as ex
      when ex.Message = "One or more errors occurred. (Response status code does not indicate success: 500 (Internal Server Error).)" ->
        printfn "- error thread %s" thread.Thread.Link
        return Seq.empty
    | x ->
      printfn "%s %A" thread.Thread.Link x
      return! parsePostsInPage page thread
  }
  
  let Poll(db: ComicvineContext)(ct: CancellationToken)(forumPage: int) =
    task {
      logger.LogInformation("getting thread  on page {0}. {1}", forumPage, DateTime.Now)
     
      let batchSize = 6
      let postsData   = Result<Post>.Create(Seq.empty, Seq.empty)
      let threadsData = Result<Couple>.Create(Seq.empty, Seq.empty)
      
      let! rawHtml = getHtml ct forumPage "/forums/"
      let nextThreads =
        rawHtml
        |> Net.createRootNode
        |> ThreadParser.ParseSingle
      
      logger.LogInformation("parsing threads {0}", DateTime.Now)
      
      for b in nextThreads do
        let! data = getThreadData db b
        threadsData.Extend(data)
        
      let newThreadBatch = parsePostsInBatches threadsData.New batchSize parsePostsInPage
      let updateThreadBatch = parsePostsInBatches threadsData.Update batchSize parsePostsInPage
        
      logger.LogInformation("parsing new post {0}", DateTime.Now)
      
      let newTasks =
        newThreadBatch
        |> Seq.map (
          Task.map ( fun batch ->
            batch
            |> Seq.map (fun (a,_,pp) -> getPostDataFromNewThread a pp)
            |> Seq.iter postsData.Extend
          )
        )
        
      // let mutable h = 1
      for task in newTasks do
        do! task
        // printfn "batch %d" h
        // h <- h + 1
        
      logger.LogInformation("parsing updated post {0}", DateTime.Now)
      
      // let mutable n = 1
      let batchedUpdates = List()
      for batch in updateThreadBatch do
        let! xxx = batch
        batchedUpdates.AddRange(xxx)
        // printfn "batch %d" n
        // n <- n + 1
      // workaround for the weird "state machine not statically compilable" error
      // https://github.com/dotnet/fsharp/issues/12839#issuecomment-1292310944
      let mutable batchEnumerator = batchedUpdates.GetEnumerator()
      while batchEnumerator.MoveNext() do
        let posts, thread, page = batchEnumerator.Current
        let! data = getPostDataFromUpdatedThread db thread posts page
        postsData.Extend(data)
      batchEnumerator.Dispose()
        
      logger.LogInformation("finished {0}", DateTime.Now)
      return threadsData, postsData
    }

  let pollVine ct (db: ComicvineContext) = task {
      let mutable page = 1
      let mutable finished = false

      while not finished do
        logger.LogInformation("making request to page {0}", page)
        
        let! threadD, postD = Poll db ct page
        
        logger.LogInformation("{0} new and {1} updated threads. {2}", threadD.New.Count, threadD.Update.Count, DateTime.Now)
        logger.LogInformation("{0} new and {1} updated posts. {2}", postD.New.Count, postD.Update.Count, DateTime.Now)
        
        // for t in threadD.New do
        //   printfn "a - %s" t.Data.Thread.Link
        // for t in threadD.Update do
        //   printfn "b - %s" t.Data.Thread.Link
        
        let newThreadAndPostsToAdd =
          threadD.New
          |> Seq.map(fun abc ->
            {
              abc.Data with
                Posts =
                  postD.New
                  |> Seq.filter (fun p -> p.ThreadId = abc.Data.Id)
                  |> Array.ofSeq
            }
          )
          
        let updatedThreadNewPostToAdd =
          threadD.Update
          |> Seq.map(fun abc ->
            {
              abc.Data
              with
                Posts =
                  (
                    postD.Update
                    |> Seq.filter (fun p -> p.ThreadId = abc.Data.Id)
                  ).ToList()
            }
          )
        
        let updatedThreadUpdatedPostToAdd =
          updatedThreadNewPostToAdd
          |> Seq.collect (fun ab ->
            postD.New
            |> Seq.filter (fun p -> p.ThreadId = ab.Id)
          )
        // save to db
        // order is important for updated threads
        do! db.Threads.AddRangeAsync(newThreadAndPostsToAdd)
        db.Threads.UpdateRange(updatedThreadNewPostToAdd)
        do! db.Posts.AddRangeAsync updatedThreadUpdatedPostToAdd
        
        // save changes and stops tracking entities after changes have been saved in current session...
        // ..to prevent exception when changes are made on the same entities in future sessions
        let! changesMade = db.SaveChangesAsync()
        db.ChangeTracker.Clear() 
        // ensure certain properties always hold
        let intersectCount = postD.New.Intersect(postD.Update, postComparer).Count()
        let calculatedChanges = postD.Update.Count + postD.New.Count + threadD.Update.Count + threadD.New.Count
        Assert (intersectCount = 0) $"new and updated posts not mutually exclusive. {intersectCount} duplicates"
        Assert (changesMade = calculatedChanges) $"number of changes not adding up. Expect {changesMade}, got {calculatedChanges}"
        
        page <- page + 1
        finished <- (threadD.New.Count + threadD.Update.Count) = 0
    }

  override _.ExecuteAsync(ct) =
    task {
      // scoping stuff for injection database context: https://pgroene.wordpress.com/2018/07/12/injecting-a-scoped-service-into-ihostedservice/
      use scope = scopeFactory.CreateScope()
      use timer = new PeriodicTimer(TimeSpan.FromMinutes(5L))

      let dbCtx =
        scope.ServiceProvider.GetRequiredService<ComicvineContext>()

      while not ct.IsCancellationRequested do
        let! r = timer.WaitForNextTickAsync(ct)
        logger.LogInformation("starting new task at: {time}", DateTimeOffset.Now)

        if r then
          do! pollVine ct dbCtx
          logger.LogInformation("completed at {time}", DateTimeOffset.Now)
    }
    :> Task
