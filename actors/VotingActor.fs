namespace Actors

open Dapr.Client
open Dapr.Actors
open Dapr.Actors.Runtime
open Shared
open Shared.Actors
open Shared.Extensions

/// <summary>
/// An actor that is responsible for handling the votes in the state store.
///
/// The voting process with Dapr's state store, isn't an atomic operation. We must first retrieve the votes from
/// the store, increase the voting counter and store it again with the updated value. Due to the concurrent nature
/// of our program, we have a high change that a race condition occurs and that we lost voting requests. Therefore
/// we use an actor to handle the state manipulation. Due to it's nature an actor handles on request at the time.
/// With this we don't need to implement locking or synchronization in our program.
/// </summary>
/// <param name="actorService">The <see cref="P:Dapr.Actors.Runtime.Actor.ActorService" /> that will host this actor instance.</param>
/// <param name="actorId">Id for the actor.</param>
/// <param name="daprClient">A dapr client instance.</param>
[<Actor(TypeName = VotingActor.name)>]
type VotingActor(actorService: ActorService, actorId: ActorId, daprClient: DaprClient) =
    inherit Actor(actorService, actorId)

    interface IVotingActor with

        /// <summary>
        /// Increase the votes for the given `Animal`.
        /// </summary>
        /// <param name="animal">The animal to vote for</param>
        /// <returns>The updated votes</returns>
        member _.Vote(animal) =
            async {
                let! maybeVotes = daprClient.GetStateAsyncF("voting", "votes")

                let updatedVotes =
                    (match maybeVotes with
                     | Some (votes) -> votes
                     | None -> Votes.empty)
                        .Vote(animal)

                daprClient.SaveStateAsync<Votes>("voting", "votes", updatedVotes)
                |> Async.AwaitTask
                |> ignore

                return updatedVotes
            }
            |> Async.StartAsTask
