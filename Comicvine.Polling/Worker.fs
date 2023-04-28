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
open Microsoft.FSharp.Collections

type ThreadOp =
    | New    of Parsers.Thread
    | Update of Parsers.Thread
    | None
    
type PollInfo =
    {
        NewThreads: int
        NewPosts: int
        DeletedPosts: int
    }
type Worker(logger: ILogger<Worker>, factory: ComicvineContextFactory) =
    inherit BackgroundService()
    let timer = new PeriodicTimer(TimeSpan.FromMinutes(1))
    let threadParser = Parsers.ThreadParser()
    let postParser = Parsers.PostParser()

    let newThread = ConcurrentBag<Parsers.Thread>()
    let updateThread = ConcurrentBag<Parsers.Thread>()

         
    let pollThread ct (db: ComicvineContext) = task {
        let update(db: ComicvineContext)(thread: Parsers.Thread) = task {
            let! dbResult = db.Threads.AsNoTracking().FirstOrDefaultAsync((fun t -> t.Id = thread.Id))
            // new thread
            if obj.ReferenceEquals(dbResult, null) then
                printfn $"id {thread.Id}; State: New"
                let! posts = Parsers.Common.ParseMultiple postParser thread.Thread.Link
                // add thread along with all posts found
                let t = { thread with Posts = posts.ToArray() }
                let! _ = db.Threads.AddAsync t
                ()
            elif dbResult.LastPostNo < thread.LastPostNo then
                printfn $"id {thread.Id}; State: Update"
                printfn "%A" thread.Posts.Count
                // let newT =
                //      {
                //         dbResult with
                //             // update thread properties
                //             Thread = { dbResult.Thread with Text = thread.Thread.Text}
                //             Board = thread.Board
                //             IsPinned = thread.IsPinned
                //             IsLocked = thread.IsLocked
                //             LastPostNo = thread.LastPostNo
                //             LastPostPage = thread.LastPostPage
                //             TotalPosts = thread.TotalPosts
                //             TotalView = thread.TotalView
                //     }
                // let _ = db.Threads.Update newT
                ()
            else
                ()
        }
        let updateThreads(thread: Parsers.Thread) = task {
            let! dbResult = db.Threads.FindAsync(thread.Id)
            
            // new thread
            if obj.ReferenceEquals(dbResult, null) then
                // add thread to db
                newThread.Add(thread)
                // let! _ = db.Threads.AddAsync(thread)
                do! Console.Out.WriteLineAsync($"id {thread.Id}; State: New")
                // add all posts to db since they are new
                let! posts = Parsers.Common.ParseMultiple postParser thread.Thread.Link
                return {NewPosts = 0; NewThreads = 1; DeletedPosts = 0}
            // thread to update
            elif dbResult.LastPostNo < thread.LastPostNo then
                let threadToAdd =
                    {
                        dbResult with
                            Thread = { dbResult.Thread with Text = thread.Thread.Text}
                            IsPinned = thread.IsPinned
                            IsLocked = thread.IsLocked
                            LastPostNo = thread.LastPostNo
                            LastPostPage = thread.LastPostPage
                            TotalPosts = thread.TotalPosts
                            TotalView = thread.TotalView
                    }
                // stop tracking returned result so we can add updated thread
                db.Entry(dbResult).State <- EntityState.Detached
                // let _ = db.Threads.Update(threadToAdd)
                updateThread.Add(threadToAdd)
                do! Console.Out.WriteLineAsync($"id {thread.Id}; State: Update" )
                 
                let! parsedPosts = Parsers.Common.ParseMultiple postParser thread.Thread.Link
                // get posts already in db
                let dbPosts: Parsers.Post seq = Seq.empty
                
                let idSelector = (fun (x: Parsers.Post) -> x.PostNo)
                let pid = parsedPosts |> Seq.map idSelector
                let did     = dbPosts |> Seq.map idSelector
                let postsNotInDb =
                    pid.Except(did)
                // mark posts not in parsed post as deleted
                let postsToDelete =
                    dbPosts.ExceptBy(pid, idSelector)
                
                
                let updatePosts (orig: Parsers.Post) =
                    // if the post is edited, update contents
                    if orig.IsEdited then
                        { orig with IsEdited = true; Content = orig.Content }
                    else
                        orig
                
                        
                let postsToAdd  =
                    dbPosts
                    |> Seq.map updatePosts
                    |> Seq.append postsToDelete
                    
                return
                    {
                       NewPosts = postsNotInDb.Count()
                       NewThreads = 0
                       DeletedPosts = postsToDelete.Count()
                    }
            else
                return {NewPosts = 0; NewThreads = 0; DeletedPosts = 0}
        }
        
        
        let latest(thread: Parsers.Thread) = task {
            let! posts = Parsers.Common.ParseMultiple postParser thread.Thread.Link
            printfn $"Gotten posts for thread {thread.Id}"
            return
                thread
        }
        
        
        let mutable page = Random().Next(16600)
        let mutable finished = false
        
        while not finished do
            logger.LogInformation("making request to page {0}", page)
            let! stream = Net.getStreamByPageCt ct page "/forums/"
            // let! batched =
            let batched =
                Net.getRootNode stream
                |> threadParser.ParseSingle
                |> Seq.map (update db)
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
            logger.LogInformation("finished polling vine")
            for t in
                batched
            //     |> Seq.filter Option.isSome
            //     |> Seq.map Option.get
                // |> db.Threads.BulkMergeAsync
                do
                    let! thread = t
                    ()
            // do!
            //     batched
            //     |> Seq.filter Option.isSome
            //     |> Seq.map Option.get
            //     |> Seq.map (fun x -> x.Posts)
            //     |> Seq.concat
            //     |> db.Posts.BulkMergeAsync
            //  
            logger.LogInformation("finished updating db")
            // let! _ = db.Threads.AddRangeAsync(newThread)
            // do db.Threads.UpdateRange(updateThread)
            
            // logger.LogInformation($"{nP} new posts; {nT} new threads; {dP} deleted posts")
            // logger.LogInformation("finished updating db")
            
            finished <- true//nT < 50//x |> Seq.forall id
            page <- page + 1
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