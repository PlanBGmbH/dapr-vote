namespace Votes.Controllers

open Dapr.Client
open Dapr.Actors.Client
open Microsoft.AspNetCore.Mvc
open Shared.Config
open Shared.Extensions
open Votes
open Votes.Actors

[<ApiController>]
type VoteController([<FromServices>] daprClient: DaprClient) =
    inherit ControllerBase()

    [<HttpGet("votes")>]
    member _.Results() =
        async {
            let! maybeVotes = daprClient.GetStateAsyncF<Votes>(StateStore.name, StateStore.votes)

            let votes =
                match maybeVotes with
                | Some (votes) -> votes
                | None -> Votes.empty

            return OkObjectResult(votes)
        }

    [<HttpPost("votes")>]
    member _.Vote([<FromBody>] vote: Vote) =
        async {
            let proxy = ActorProxy.Create<IVotingActor>(VotingActor.id, VotingActor.name)

            let! votes = proxy.Vote(vote.Animal) |> Async.AwaitTask

            return OkObjectResult(votes)
        }
