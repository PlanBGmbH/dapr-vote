namespace Subscriptions.Controllers

open Dapr.Client
open Dapr.Actors.Client
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
            let! votes =
                daprClient.GetStateAsyncOr<Subscriptions>(StateStore.name, StateStore.subscriptions, Subscriptions())

            return OkObjectResult(votes)
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
