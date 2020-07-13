namespace Votes.Controllers

open Dapr.Client
open Dapr.Actors
open Dapr.Actors.Client
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open Shared
open Shared.Actors
open Shared.Extensions

[<ApiController>]
[<Route("[controller]")>]
type VoteController([<FromServices>] daprClient: DaprClient) =
    inherit ControllerBase()

    [<HttpGet>]
    member _.Results(): Task<Votes> =
        async {
            let! maybeVotes = daprClient.GetStateAsyncF("voting", "votes")

            return match maybeVotes with
                   | Some (votes) -> votes
                   | None -> Votes.empty
        }
        |> Async.StartAsTask

    [<HttpPost>]
    member _.Vote(animal: Animal): Task<Votes> =
        let proxy = ActorProxy.Create<IVotingActor>(ActorId("voting"), VotingActor.name)
        proxy.Vote(animal)
