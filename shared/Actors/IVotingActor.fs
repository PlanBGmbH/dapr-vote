namespace Shared.Actors

open Dapr.Actors
open Shared
open System.Threading.Tasks

// Based on https://github.com/dapr/dotnet-sdk/blob/master/docs/get-started-dapr-actor.md#implement-imyactor-interface
type IVotingActor =
    inherit IActor

    abstract Results: Task<Votes>
    abstract Vote: Animal -> Task<Votes>
