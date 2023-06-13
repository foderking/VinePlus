namespace Comicvine.Polling

open System
open System.Collections.Generic
open System.Linq
open System.Net.Http
open System.Threading
open System.Threading.Tasks
open Comicvine.Core
open Comicvine.Core.Parsers
open Comicvine.Database
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

type Worker(logger: ILogger<Worker>, scopeFactory: IServiceScopeFactory) =
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
    
  let timer =
    new PeriodicTimer(TimeSpan.FromMinutes(5))
  // let newThreadBag =
  //   ConcurrentBag<Couple>()
  // let updateThreadBag =
  //   ConcurrentBag<Couple>()
  // let newPostBag =
  //   ConcurrentBag<Post>()
  // let updatePostBag =
  //   ConcurrentBag<Post>()

  let threadProd(db: ComicvineContext)(thread: Thread) = task {
    let! dbResult =
      db
        .Threads
        .AsNoTracking()
        .FirstOrDefaultAsync(fun t -> t.Id = thread.Id)
        
    if obj.ReferenceEquals(dbResult, null) then
      // newThreadBag.Add({
      //   Data = thread; Pages = seq{ 1..thread.LastPostPage }
      // })
      return
        Result<Couple>.Create(
          newItem    = [|{Data = thread; Pages = seq{ 1..thread.LastPostPage }}|],
          updateItem = [||]
        )
    elif dbResult.LastPostNo < thread.LastPostNo then
      // updateThreadBag.Add({
      //   Data = thread; Pages = seq{ dbResult.LastPostPage .. thread.LastPostPage }
      // })
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
  
  let consNewThread(newPosts: seq<Post>) =
     // every post in the thread would be marked as a new post
    // let! newPosts = PostParser.ParsePage page thread.Thread.Link 
    // add all posts
    // newPosts
    // |> Seq.iter newPostBag.Add
    Result<Post>.Create(
      newItem    = newPosts,
      updateItem = Seq.empty
    )
    // |> Seq.iter (fun x ->
    //   newPostBag.Add(x)
    // )   

  let consUpdatedThread(db: ComicvineContext)(thread: Thread)(parsedPosts: seq<Post>)(page: int) = task {
     // for a visualization of how new, deleted, and updated post related, a venn diagram is available here:
    // https://web.archive.org/web/20230610183122/https://ibb.co/02Vx8Pk
    // get the posts from a particular pge in the thread from db
    let parsedPosts = parsedPosts |> Seq.filter (fun each -> not(page <> 1 && each.PostNo = 0)) // removes the op since it appears every page for blog, polls and co.
    let result = Result<Post>.Create(Seq.empty, Seq.empty)
    let! dbPosts =
      db
        .Posts
        .AsNoTracking()
        .Where(fun each -> each.ThreadId = thread.Id)
        .Where(fun each ->
          (page > 1 && each.PostNo > (page*50-50) && each.PostNo <= (page*50))||
          (page = 1 && each.PostNo >= 0 && each.PostNo <= 50)
        )
        .ToArrayAsync()
    // I'm using a comparer function because `exceptby` isn't working on f# for some reason

    // deleted posts are posts that are present in the db, but not parsed from comicvine
    let deletedPosts = dbPosts.Except(parsedPosts, postComparer)
    // new posts are posts that are parsed in comicvine, but not present in db
    let newPosts = parsedPosts.Except(dbPosts, postComparer)
    // edited posts are posts that are present in both db and parsed from comicvine,...
    // but also have the `IsEdited` property set to true.
    // These posts should be constantly updated
    let editedPosts = parsedPosts.Intersect(dbPosts, postComparer).Where(fun x -> x.IsEdited)
    
    
    // add new posts
    // newPosts
    // |>Seq.iter( fun x ->
    //   newPostBag.Add(x)
    // )
    result.New.AddRange(newPosts)
    // add deleted posts
    // deletedPosts
    // |> Seq.iter (fun x ->
    //   updatePostBag.Add({
    //    x with IsDeleted = true
    //   })
    // )
    deletedPosts
    |> Seq.map (fun each -> { each with IsDeleted = true })
    |> result.Update.AddRange
    // add edited posts
    // editedPosts
    // |> Seq.iter (fun x ->
    //   updatePostBag.Add(x)
    // )
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
    
  let doBatch collection batchSize parsePosts=
    collection
    // make request page by page
    |> Seq.collect (fun abc -> seq {
      for i in abc.Pages ->
        abc.Data, i
    })
    // group to batches of `batchSize`
    |> Seq.mapi    (fun x y -> x,y)
    |> Seq.groupBy (fun (x, _) -> x / batchSize)
    // parsed posts from vine in parallel within each batch
    |> Seq.map(fun (_, batch) ->
      batch
      |> Seq.map(fun (_,(thread,page)) ->
        parsePosts page thread
        |> Async.map (fun x -> x,thread, page)
        |> Task.ofAsync
      )
      |> Task.WhenAll
    )


  let rec newPst page thread = async {
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
      return! newPst page thread
  }
  
  let Poll
    (db: ComicvineContext)(ct: CancellationToken)(forumPage: int) =
    task {
      logger.LogInformation("getting thread  on page {0}. {1}", forumPage, DateTime.Now)
     
      let batchSize = 6
      let threadsData = Result<Couple>.Create(Seq.empty, Seq.empty)
      let postsData   = Result<Post>.Create(Seq.empty, Seq.empty)
      
      let! rawHtml = getHtml ct forumPage "/forums/"
      let nextThreads =
        rawHtml
        |> Net.createRootNode
        |> ThreadParser.ParseSingle
      
      
      logger.LogInformation("parsing threads {0}", DateTime.Now)
      for b in nextThreads do
        let! data = threadProd db b
        threadsData.Extend(data)
        
      let newThreadPost = doBatch threadsData.New batchSize newPst
      let updateThreadPost = doBatch threadsData.Update batchSize newPst
        
      logger.LogInformation("parsing new post {0}", DateTime.Now)
      let newTasks =
        newThreadPost
        |> Seq.map (
          Task.map ( fun batch ->
            batch
            |> Seq.map (fun (a,_,_) -> consNewThread a)
            |> Seq.iter postsData.Extend
          )
        )
      let mutable h= 1
      for task in newTasks do
        printfn "batch %d" h
        h <- h + 1
        do! task
        
      logger.LogInformation("parsing updated post {0}", DateTime.Now)
      let batchedUpdates = List()
      let mutable n = 1
      for batch in updateThreadPost do
        let! xxx = batch
        printfn "batch %d" n
        n <- n + 1
        batchedUpdates.AddRange(xxx)
      // workaround for the weird "state machine not statically compilable" error
      // https://github.com/dotnet/fsharp/issues/12839#issuecomment-1292310944
      let mutable batchEnumerator = batchedUpdates.GetEnumerator()
      while batchEnumerator.MoveNext() do
        let posts, thread, page = batchEnumerator.Current
        let! data = consUpdatedThread db thread posts page
        postsData.Extend(data)
        
      batchEnumerator.Dispose()
        
      logger.LogInformation("finished {0}", DateTime.Now)
      return threadsData, postsData
    }

  let pollVine ct (db: ComicvineContext) =
    task {
      let mutable page = 1
      let mutable finished = false

      while not finished do
        
        // newThreadBag.Clear()
        // updateThreadBag.Clear()
        // newPostBag.Clear()
        // updatePostBag.Clear()
        
        logger.LogInformation("making request to page {0}", page)
        let! threadD, postD = Poll db ct page
        
        printfn "%d %d new, update for thread" threadD.New.Count threadD.Update.Count
        printfn "%d %d new, update for post" postD.New.Count postD.Update.Count
        let ope = postD.New.Intersect(postD.Update, postComparer)
        printfn "%d" (ope.Count())
        
        // for t in threadD.New do
        //   printfn "a - %s" t.Data.Thread.Link
        // for t in threadD.Update do
        //   printfn "b - %s" t.Data.Thread.Link
        
        // let allThreads = Seq.append newThreadBag updateThreadBag
        let allPost = Seq.append postD.New postD.Update
          
        let nXxx =
          threadD.New
          |> Seq.map(fun abc ->
            let th = abc.Data
            let posts =
              allPost
              |> Seq.filter (fun p -> p.ThreadId = th.Id)
              // |> Seq.distinctBy (fun p -> p.Id)
            { th with Posts = posts.ToArray() }
          )
        let uXxx =
          threadD.Update
          |> Seq.map(fun abc ->
            let th = abc.Data
            let posts =
              postD.Update
              |> Seq.filter (fun p -> p.ThreadId = th.Id)
              // |> Seq.distinctBy (fun p -> p.Id)
            { th with Posts = posts.ToList() }
          )
        do! db.Threads.AddRangeAsync(nXxx)
        db.Threads.UpdateRange(uXxx)
        let yy =
          uXxx
          |> Seq.collect (fun ab ->
            postD.New
            |> Seq.filter (fun p -> p.ThreadId = ab.Id)
            // |> Seq.distinctBy (fun p -> p.Id)
          )
        printfn "%d" (Seq.length yy)
        do! db.Posts.AddRangeAsync yy
        
        // save changes and stops tracking entities after changes have been saved in current session...
        // ..to prevent exception when changes are made on the same entities in future sessions
        let! x = db.SaveChangesAsync()
        db.ChangeTracker.Clear() 
        printfn "%d changes" x
        page <- page + 1
        finished <- (threadD.New.Count + threadD.Update.Count) = 0
    }

  override _.ExecuteAsync(ct) =
    task {
      //scoping stuff for injection database context: https://pgroene.wordpress.com/2018/07/12/injecting-a-scoped-service-into-ihostedservice/
      use scope = scopeFactory.CreateScope()

      let dbCtx =
        scope.ServiceProvider.GetRequiredService<ComicvineContext>()

      while not ct.IsCancellationRequested do
        let! r = timer.WaitForNextTickAsync(ct)
        logger.LogInformation("starting new task at: {time}", DateTimeOffset.Now)

        if r then
          do! pollVine ct dbCtx
          logger.LogInformation("completed at {time}", DateTimeOffset.Now)
          let! o = dbCtx.SaveChangesAsync ct
          logger.LogInformation("{0} changes", o)
    }
    :> Task
