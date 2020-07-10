namespace Actors

open Dapr.Actors
open Dapr.Actors.Runtime
open Shared
open Shared.Actors

// Based on https://github.com/dapr/dotnet-sdk/blob/master/docs/get-started-dapr-actor.md#add-actor-implementation

/// <summary>
/// An actor that is responsible for handling the votes in the state store.
///
/// The voting process with Dapr's state store, isn't an atomic operation. We must first retrieve the votes from
/// the store, increase the voting counter and store it again with the updated value. Due to the concurrent nature
/// of our program, we have a high change that a race condition occurs and that we lost voting requests. Therefore
/// we use an actor to handle the state manipulation. Due to it's nature an actor handles on request at the time.
/// With this we don't need to implement locking or synchronization in our program.
/// </summary>
type VotingActor(actorService: ActorService, actorId: ActorId) =
    inherit Actor(actorService, actorId)

    let stateManager = base.StateManager

    /// <summary>
    /// Helper function to handle the non-existence of the votes in the datastore in a functional way, by returning
    /// the `Option` type.
    ///
    /// The state manager throws an exception if the key "votes" doesn't exists in the store. We catch that exception
    /// and return `None` in this case. If we get the result, we return `Some`.
    /// </summary>
    /// <returns>Maybe the votes</returns>
    let getVotes: Async<Option<Votes>> =
        async {
            try
                let! votes =
                    stateManager.GetStateAsync<Votes>("votes")
                    |> Async.AwaitTask
                return Some(votes)
            with :? System.Collections.Generic.KeyNotFoundException -> return None
        }

    (*
        The actor implementation.
    *)

    interface IVotingActor with

        /// <summary>
        /// Gets the voting results
        /// </summary>
        /// <returns>The voting results</returns>
        member _.Results =
            async {
                let! maybeVotes = getVotes

                return match maybeVotes with
                       | Some (votes) -> votes
                       | None -> Votes.empty
            }
            |> Async.StartAsTask

        /// <summary>
        /// Increase the votes for the given `Animal`.
        /// </summary>
        /// <param name="animal">The animal to vote for</param>
        /// <returns>The updated votes</returns>
        member _.Vote(animal) =
            async {
                let! maybeVotes = getVotes

                let updatedVotes =
                    (match maybeVotes with
                     | Some (votes) -> votes
                     | None -> Votes.empty).Vote(animal)

                stateManager.SetStateAsync<Votes>("votes", updatedVotes)
                |> Async.AwaitTask
                |> ignore

                return updatedVotes
            }
            |> Async.StartAsTask
