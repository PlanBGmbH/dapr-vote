namespace Subscriptions

open Dapr.Actors.AspNetCore
open Dapr.Actors.Runtime
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Shared.Config
open Subscriptions.Actors

module Program =
    let exitCode = 0
    let port = 3001

    let CreateHostBuilder args =
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(fun webBuilder ->
                webBuilder
                    .UseStartup<Startup>()
                    .UseActors(fun actorRuntime ->
                        actorRuntime.RegisterActor<SubscriptionActor>(fun tpe ->
                            ActorService(tpe, fun actorService actorId ->
                                SubscriptionActor(
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
