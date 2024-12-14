namespace VinePlus.Polling

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Comicvine.Database
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting

module Program =
    let createHostBuilder args =
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true) // https://stackoverflow.com/questions/69961449/net6-and-datetime-problem-cannot-write-datetime-with-kind-utc-to-postgresql-ty
        Host
            .CreateDefaultBuilder(args)
            .ConfigureServices(fun hostContext services ->
                services.AddHostedService<PollingWorker>()
                |> ignore
                services.RegisterDataServices(hostContext.Configuration) // IConfiguration for worker service in hostContext.Configuration
                |> ignore
            )

    [<EntryPoint>]
    let main args =
        createHostBuilder(args).Build().Run()
        0 // exit code