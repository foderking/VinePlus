namespace Comicvine.Polling

open System
open System.Collections.Concurrent
open System.Collections.Generic
open System.Linq
open System.Threading
open System.Threading.Tasks
open Comicvine.Core
open Comicvine.Database
open FSharp.Control
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.EntityFrameworkCore

type ThreadOp =
    | New
    | Update
    | None
    
type Worker(logger: ILogger<Worker>, factory: ComicvineContextFactory) =
    inherit BackgroundService()
    let timer = new PeriodicTimer(TimeSpan.FromMinutes(1))
    let threadParser = Parsers.ThreadParser()
    let postParser = Parsers.PostParser()

    let relevantThreads = ConcurrentBag<Parsers.Thread>()
    let relevantPosts = ConcurrentBag<Parsers.Post>()

         
    let pollThread ct (db: ComicvineContext) = task {
        let filterThread (thread: Parsers.Thread) = task {
            printfn "a) got thread: %A - %A" thread.Id DateTime.Now
            let! dbResult = db.Threads.FindAsync(thread.Id)
            let newThreadPredicate = obj.ReferenceEquals(dbResult, null)
            let updatePredicate = not newThreadPredicate && dbResult.LastPostNo < thread.LastPostNo
            
            let pred = newThreadPredicate || updatePredicate
            
            if updatePredicate then db.Threads.Remove(dbResult) |> ignore
            
            printfn "b) is thread update: %A, %A" pred DateTime.Now
            return pred
        }
        //  
        // let consumeThread (thread: Parsers.Thread) = task {
        //     relevantThreads.Add(thread)
        //     relevantThreads.
        // }
        //
        // let getPosts(thread: Task<Parsers.Thread option>) = task {
        //     let! t = thread
        //     if t.IsSome then
        //         let! p = postParser.ParseAll(t.Value.Thread.Link)
        //         return Some p
        //     else
        //         return None
        // }
        //
        // let filterPost (post: Parsers.Post) = task {
        //     let! dbResult = db.FindAsync(thread.Id)
        //     let predicate =  obj.ReferenceEquals(dbResult, null) || dbResult.LastPostNo < thread.LastPostNo
        //     printfn "d"
        //     return
        //         if predic then
        //             Some post
        //         else None
        // }
        //
        // let consumePost (post: Task<seq<Parsers.Post>>) = task {
        //     let consume (post: Task<Parsers.Post option>) = task {
        //         let! p = post
        //         if p.IsSome then
        //             printfn "e %A" (if p.Value.IsComment then p.Value.CommentInfo.Id else p.Value.ThreadId)
        //             relevantPosts.Add(p.Value)
        //         else
        //             printfn "e"
        //     }
        //         
        //     let! p = post
        //     printfn "c"
        //     let! z =
        //         p
        //         |> Seq.map filterPost
        //         |> Seq.map consume
        //         |> Task.WhenAll
        //     z |> ignore
        // }
        //
        //
        
        let mutable page = 1
        let mutable finished = false
        
        while not finished do
            // relevantPosts.Clear()
            // relevantThreads.Clear()
            
            logger.LogInformation("making request to page {0}", page)
            let! stream = Net.getStreamByPageCt ct page "/forums/"
            let threads =
                Net.getRootNode stream
                |> threadParser.ParseSingle
            
            let batchedThreads = 
                threads
                |> TaskSeq.ofSeq
                |> TaskSeq.filterAsync filterThread
                |> TaskSeq.toArray
            // batchedThreads
            // |> Array.Parallel.iter relevantThreads.Add
            let batchedPosts =
                batchedThreads
            
            logger.LogInformation("{0} new threads", batchedThreads.Length)
            do! db.Threads.AddRangeAsync(batchedThreads)
            // let! z =
            //     batchedThreads
            //     |> Seq.filter Option.isSome
            //     |> Seq.map Option.get
            //     |> Seq.map (fun x -> postParser.ParseAll(x.Thread.Link))
            //     |> Seq.map consumePost
            //     |> Task.WhenAll
                // |> Seq.map consumePost
            
            finished <- true//x |> Seq.forall id
            page <- page + 1
            // logger.LogInformation("{0} new posts", relevantPosts.Count)
    }
    
    override _.ExecuteAsync( ct) =
        task {
            while not ct.IsCancellationRequested do
                let! r = timer.WaitForNextTickAsync(ct)
                use dbCtx = factory.CreateDbContext([||])
                logger.LogInformation("starting new task at: {time}", DateTimeOffset.Now)
                if r then
                    do! pollThread ct dbCtx
                    logger.LogInformation("completed at {time}", DateTimeOffset.Now)
                    let! o = dbCtx.SaveChangesAsync ct
                    logger.LogInformation("{0} changes", o)
        }
        :> Task // need to convert into the parameter-less task