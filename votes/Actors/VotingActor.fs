namespace Votes.Actors

open Dapr.Client
open Dapr.Actors
open Dapr.Actors.Runtime
open FSharpx.Control
open Shared.Config
open Shared.Extensions
open System.Threading.Tasks
open Shared

module VotingActor =

    /// <summary>
    /// The name of the actor.
    /// </summary>
    [<Literal>]
    let Name = "VotingActor"

    /// <summary>
    /// The ID of the actor.
    /// </summary>
    let ID = ActorId("voting")

/// <summary>
/// An actor that is responsible for handling the votes in the state store.
/// </summary>
type IVotingActor =
    inherit IActor

    /// <summary>
    /// Increase the votes for the given `Animal`.
    /// </summary>
    /// <param name="animal">The animal to vote for.</param>
    /// <returns>The updated votes.</returns>
    abstract Vote: animal: Animal -> Task<Votes>

/// <summary>
/// Initializes a new instance of the <see cref="VotingActor"/> class.
/// </summary>
/// <param name="actorService">The <see cref="P:Dapr.Actors.Runtime.Actor.ActorService" /> that will host this actor instance.</param>
/// <param name="actorId">ID for the actor.</param>
/// <param name="daprClient">A dapr client instance.</param>
[<Actor(TypeName = VotingActor.Name)>]
type VotingActor(actorService: ActorService, actorId: ActorId, daprClient: DaprClient) =
    inherit Actor(actorService, actorId)

    interface IVotingActor with

        /// <inheritdoc/>
        member _.Vote(animal: Animal) =
            async {
                let! votes = daprClient.GetStateAsyncOr(StateStore.name, StateStore.votes, Votes.empty)

                let updatedVotes = votes.Vote(animal)

                return! daprClient.SaveStateAsync<Votes>(StateStore.name, StateStore.votes, updatedVotes)
                |> Async.AwaitTask
                |> Async.map(fun _ -> updatedVotes)
            }
            |> Async.StartAsTask
