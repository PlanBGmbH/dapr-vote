namespace Shared.Actors

open Dapr.Actors
open Shared
open System.Threading.Tasks

module VotingActor =
    [<Literal>]
    let name = "VotingActor"

type IVotingActor =
    inherit IActor

    abstract Vote: Animal -> Task<Votes>
