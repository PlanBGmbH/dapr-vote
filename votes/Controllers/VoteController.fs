namespace Votes.Controllers

open Dapr.Actors
open Dapr.Actors.Client
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open Shared
open Shared.Actors

[<ApiController>]
[<Route("[controller]")>]
type VoteController () =
    inherit ControllerBase()

    let proxy = ActorProxy.Create<IVotingActor>(ActorId("voting"), "VotingActor")

    [<HttpGet>]
    member _.Results() : Task<Votes> =
        proxy.Results

    [<HttpPost>]
    member _.Vote(animal : Animal): Task<Votes> =
        proxy.Vote(animal)
