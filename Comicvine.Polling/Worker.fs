namespace Comicvine.Polling

open System
open System.Collections.Concurrent
open System.Collections.Generic
open System.Linq
open System.Net.Http
open System.Threading
open System.Threading.Tasks
open Comicvine.Core
open Comicvine.Core.Parsers
open Comicvine.Database
open FSharp.Control
open Microsoft.EntityFrameworkCore.Storage
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.EntityFrameworkCore
open Microsoft.FSharp.Collections

type ThreadOp =
    | New    of Parsers.Thread
    | Update of Parsers.Thread
    | None
    
type PollType<'T> =
    | Update of 'T
    | Delete of 'T
    | New of 'T
    | Old
    
type Aggregate<'T> =
    {
        New: 'T seq
        Update: 'T seq
        Delete: 'T seq
    }
    
type PollInfo =
    {
        NewThreads: int
        NewPosts: int
        DeletedPosts: int
    }
    
type Worker(logger: ILogger<Worker>, scopeFactory: IServiceScopeFactory) =
    inherit BackgroundService()
    let timer = new PeriodicTimer(TimeSpan.FromMinutes(1))
    let newThread = ConcurrentBag<Parsers.Thread>()
    let updateThread = ConcurrentBag<Parsers.Thread>()
    
    let cThreads(db: ComicvineContext)(threads: Thread seq) = 
        threads
        |> Seq.map(fun th -> task {
            let! dbResult =
                db.Threads.AsNoTracking()
                    .FirstOrDefaultAsync(fun t -> t.Id = th.Id)
            
            if obj.ReferenceEquals(dbResult, null) then
                return (New th)
            elif dbResult.LastPostNo < th.LastPostNo then
                return (Update th)
            else
                return Old
        })
    
    let cPosts(db: ComicvineContext)(thread: PollType<Thread>) =
        match thread with
        | New th ->
            // every post in the thread would be marked as a new post
            Common.ParseMultiple PostParser.ParseSingle PostParser.ParseEnd th.Thread.Link
            |> Task.map(Seq.map New)
        | Update th ->
            Common.ParseMultiple PostParser.ParseSingle PostParser.ParseEnd th.Thread.Link
            |> Task.bind(fun posts -> task {
                // gets set of parsed posts to make lookups faster
                let postsSet =
                    (posts
                    |> Seq.map (fun x -> x.Id)).ToHashSet()
                // set of posts that are not in db, initially cloned from the parsed post..
                // but every posts that is already in db is eventually removed
                let newPostSet = HashSet(postsSet)
                // gets posts in db
                let! dbPosts =
                    db.Posts.AsNoTracking()
                        .Where(fun each -> each.ThreadId = th.Id)
                        .ToArrayAsync()
                return
                    dbPosts
                    |> Array.map(fun p ->
                        // if post is in db, and is in parsed, it means the post has to be updated
                        if postsSet.Contains(p.Id) then
                            // remove the post from lookup for new posts 
                            let _ = newPostSet.Remove(p.Id) 
                            // the db post is replaced with the parsed post
                            posts
                            |> Seq.find (fun x -> x.Id = p.Id)
                            |> Update
                        // if post is in db, but isn't parsed, it means the post has been deleted
                        else
                            Delete { p with IsDeleted = true }
                    )
                    |> Seq.append (
                        // adds all the new posts
                        posts
                        |> Seq.filter (fun x -> newPostSet.Contains(x.Id))
                        |> Seq.map New
                    )
            })
        | Delete _ | Old ->
            task { return Seq.empty }
    
    let aggregate(entity: 'T PollType seq) =
        let updates = List()
        let deletes = List()
        let new_ = List()
        for x in entity do
            match x with
            | Old ->
                ()
            | New e ->
                new_.Add(e)
            | Update e ->
                updates.Add(e)
            | Delete e ->
                deletes.Add(e)
        { New = new_; Update = updates; Delete = deletes }
    
    let Poll
        (db: ComicvineContext)(postConsumer: Post Aggregate -> Task)
        (threadConsumer: Thread Aggregate -> Task)(ct: CancellationToken)(page: int) = task{
        
        let print(a: 't) = printfn "%A" a
        // get the thread 
        let! batchedThreads =
            Net.getStreamByPageCt ct page "/forums/"
            |> Task.map (
                Net.getRootNode
                >> ThreadParser.ParseSingle
                >> (cThreads db)
            )
        // send the threads to db
        let mutable info = { NewThreads = 0; NewPosts =  0; DeletedPosts = 0}
        // do!
        //     threadAgg
        //     |> threadConsumer
        let threadAgg = {| New = List(); Update = List(); Delete = List() |}
        
        for thr in batchedThreads do
            let! th = thr
            
            match th with
            | New z ->
                threadAgg.New.Add(z)
            | Update z ->
                threadAgg.Update.Add(z)
            | Delete z ->
                threadAgg.Update.Add(z)
            | Old -> ()
            // gets the posts
            let! batchedPosts = cPosts db th
            
            let postAgg = aggregate batchedPosts
            // print postAgg
            info <-
                {
                    info with
                        NewPosts = info.NewPosts + postAgg.New.Count()
                        DeletedPosts = info.DeletedPosts + postAgg.Delete.Count()
                }
            // sends the posts to db
            do! postConsumer postAgg
        // print threadAgg
        return { info with NewThreads = threadAgg.New.Count }
    }
        
    let pollThread ct (db: ComicvineContext) = task {
        let mutable page = 1
        let mutable finished = false
        
        while not finished do
            logger.LogInformation("making request to page {0}", page)
            let! x = Poll db (fun x-> task{()})  (fun x -> task{()}) ct page 
            // let! stream = Net.getStreamByPageCt ct page "/forums/"
            // // let! batched =
            // let batched =
            //     Net.getRootNode stream
            //     |> threadParser.ParseSingle
            //     |> Seq.map (update db)
                // |> Task.WhenAll
            
            // let folder (curr: int * int * int) (record: PollInfo) =
            //     let (a,b,c) = curr
            //     record.NewThreads+a, record.NewPosts+b, record.DeletedPosts+c
                
            // let nT, nP, dP =
            //     batched
            //     |> Seq.fold folder (0,0,0)
            // for t in batchedThreads do
            //     let! tt = t
            //     let! g = updatePosts tt
            //     dP <- g.DeletedPosts + dP
            //     nP <- g.NewPosts + nP
            //     nT <- g.NewThreads + nT
            // logger.LogInformation("finished polling vine")
            // for t in
            //     batched
            //     |> Seq.filter Option.isSome
            //     |> Seq.map Option.get
                // |> db.Threads.BulkMergeAsync
                // do
                //     try
                //         let! thread = t
                //         ()
                //     with
                //         
                //         | :? HttpRequestException as e ->
                //             printfn "%A %A" e.StatusCode e.TargetSite
                //         | _ ->
                //             ()
            // do!
            //     batched
            //     |> Seq.filter Option.isSome
            //     |> Seq.map Option.get
            //     |> Seq.map (fun x -> x.Posts)
            //     |> Seq.concat
            //     |> db.Posts.BulkMergeAsync
            //  
            // logger.LogInformation("finished updating db")
            // let! _ = db.Threads.AddRangeAsync(newThread)
            // do db.Threads.UpdateRange(updateThread)
            
            // logger.LogInformation($"{nP} new posts; {nT} new threads; {dP} deleted posts")
            // logger.LogInformation("finished updating db")
            printfn "%A" x
            finished <- x = { NewThreads = 0; NewPosts = 0; DeletedPosts = 0 }//nT < 50//x |> Seq.forall id
            page <- page + 1
    }
    
    override _.ExecuteAsync( ct) =
        task {
            //scoping stuff for injection database context: https://pgroene.wordpress.com/2018/07/12/injecting-a-scoped-service-into-ihostedservice/
            use scope = scopeFactory.CreateScope()
            let dbCtx = scope.ServiceProvider.GetRequiredService<ComicvineContext>()
            while not ct.IsCancellationRequested do
                let! r = timer.WaitForNextTickAsync(ct)
                logger.LogInformation("starting new task at: {time}", DateTimeOffset.Now)
                if r then
                    do! pollThread ct dbCtx
                    logger.LogInformation("completed at {time}", DateTimeOffset.Now)
                    let! o = dbCtx.SaveChangesAsync ct
                    logger.LogInformation("{0} changes", o)
        }
        :> Task // need to convert into the parameter-less task