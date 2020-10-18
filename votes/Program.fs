namespace Votes

open Dapr.Actors.AspNetCore
open Dapr.Actors.Runtime
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Shared.Config
open Votes.Actors

module Program =
    let exitCode = 0
    let port = 3000

    let CreateHostBuilder args =
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(fun webBuilder ->
                webBuilder
                    .UseStartup<Startup>()
                    .UseActors(fun actorRuntime ->
                        actorRuntime.RegisterActor<VotingActor>(fun tpe ->
                            ActorService(tpe, fun actorService actorId ->
                                VotingActor(
                                    actorService,
                                    actorId,
                                    Dapr.client
                                ) :> Actor
                            )
                        )
                     )
                    .UseUrls(sprintf "http://localhost:%d/" port)
                    |> ignore
            )

    [<EntryPoint>]
    let main args =
        CreateHostBuilder(args).Build().Run()

        exitCode
