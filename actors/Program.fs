namespace Actors

open Dapr.Actors.AspNetCore
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting

// Based on https://github.com/dapr/dotnet-sdk/blob/master/docs/get-started-dapr-actor.md#register-actor-to-dapr-runtime
module Program =
    let exitCode = 0
    let port = 3000

    let CreateHostBuilder args =
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(fun webBuilder ->
                webBuilder
                    .UseStartup<Startup>()
                    .UseActors(fun actorRuntime ->
                        actorRuntime.RegisterActor<VotingActor>()
                    )
                    .UseUrls(sprintf "http://localhost:%d/" port)
                    |> ignore
            )

    [<EntryPoint>]
    let main args =
        CreateHostBuilder(args).Build().Run()

        exitCode
