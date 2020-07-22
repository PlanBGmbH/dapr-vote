namespace Votes.Controllers

open Dapr.Client
open Dapr.Actors.Client
open Dapr.Client.Http
open Microsoft.AspNetCore.Mvc
open Shared.Config
open Shared.Extensions
open Shared
open Votes.Actors

[<ApiController>]
type VoteController([<FromServices>] daprClient: DaprClient) =
    inherit ControllerBase()

    [<HttpGet("votes")>]
    member _.Results() =
        async {
            let! votes = daprClient.GetStateAsyncOr<Votes>(StateStore.name, StateStore.votes, Votes.empty)

            return OkObjectResult(votes)
        }

    [<HttpPost("votes")>]
    member _.Vote([<FromBody>] vote: Vote) =
        async {
            let proxy = ActorProxy.Create<IVotingActor>(VotingActor.ID, VotingActor.Name)

            let! votes = proxy.Vote(vote.Animal) |> Async.AwaitTask

            match vote.Subscription with
            | None -> ()
            | Some(subscription) ->
                let httpExtension = HTTPExtension()
                httpExtension.Verb <- HTTPVerb.Post
                httpExtension.ContentType <- "application/json"

                daprClient.InvokeMethodAsync<Subscription>("subscriptions", "subscriptions", subscription, httpExtension)
                |> Async.AwaitTask |> ignore

            return OkObjectResult(votes)
        }
