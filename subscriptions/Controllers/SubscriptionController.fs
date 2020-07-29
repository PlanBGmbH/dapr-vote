namespace Subscriptions.Controllers

open Dapr.Client
open Dapr.Actors.Client
open FSharpx.Collections
open Microsoft.AspNetCore.Mvc
open Shared.Config
open Shared.Extensions
open Shared
open Subscriptions.Actors

[<ApiController>]
type VoteController([<FromServices>] daprClient: DaprClient) =
    inherit ControllerBase()

    [<HttpGet("subscriptions")>]
    member _.Subscriptions() =
        async {
            let! subscriptions =
                daprClient.GetStateAsyncOr<Subscriptions>(StateStore.name, Apps.subscriptions, Map.empty)

            return OkObjectResult(subscriptions |> Map.values)
        }

    [<HttpPost("subscriptions")>]
    member _.Subscribe([<FromBody>] subscription: Subscription) =
        async {
            let proxy = ActorProxy.Create<ISubscriptionActor>(SubscriptionActor.ID, SubscriptionActor.Name)

            proxy.Subscribe(subscription) |> Async.AwaitTask |> ignore

            return OkResult()
        }

    [<HttpDelete("subscriptions/{email}")>]
    member _.Unsubscribe(email: string) =
        async {
            let proxy = ActorProxy.Create<ISubscriptionActor>(SubscriptionActor.ID, SubscriptionActor.Name)

            proxy.Unsubscribe(email) |> Async.AwaitTask |> ignore

            return OkResult()
        }
