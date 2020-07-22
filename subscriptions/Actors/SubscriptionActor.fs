namespace Subscriptions.Actors

open Dapr.Client
open Dapr.Actors
open Dapr.Actors.Runtime
open FSharpx.Control
open System.Collections.Generic
open System.Threading.Tasks
open Shared.Config
open Shared.Extensions
open Shared

type Subscriptions = Dictionary<string, Subscription>

module SubscriptionActor =

    /// <summary>
    /// The name of the actor.
    /// </summary>
    [<Literal>]
    let Name = "SubscriptionActor"

    /// <summary>
    /// The ID of the actor.
    /// </summary>
    let ID = ActorId("subscription")

/// <summary>
/// An actor that is responsible for handling the subscriptions in the state store.
/// </summary>
type ISubscriptionActor =
    inherit IActor

    /// <summary>
    /// Adds the given subscriptions to the state store.
    /// </summary>
    /// <param name="subscription">The subscription data.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    abstract Subscribe: subscription: Subscription -> Task<Subscriptions>

    /// <summary>
    /// Removes the subscription for the given email from the state store.
    /// </summary>
    /// <param name="email">The email to unsubscribe.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    abstract Unsubscribe: email: string -> Task<Subscriptions>

/// <summary>
/// Concrete subscription actor implementation.
///
/// Storing all subscriptions under a single key, may be an overhead for a large set of subscriptions. For
/// production usage, an other storage format may be better.
/// </summary>
[<Actor(TypeName = SubscriptionActor.Name)>]
type SubscriptionActor(actorService: ActorService, actorId: ActorId, daprClient: DaprClient) =
    inherit Actor(actorService, actorId)

    interface ISubscriptionActor with

        /// <inheritdoc/>
        member _.Subscribe(subscription: Subscription) =
            async {
                let! subscriptions =
                    daprClient.GetStateAsyncOr(StateStore.name, StateStore.subscriptions, Subscriptions())

                let maybeValue = subscriptions.GetValueO(subscription.Email)

                return!
                    match maybeValue with
                    | Some(value) when value.Name = subscription.Name ->
                        async { return subscriptions }
                    | _ ->
                        subscriptions.[subscription.Email] <- subscription

                        daprClient.SaveStateAsync(StateStore.name, StateStore.subscriptions, subscriptions)
                        |> Async.AwaitTask
                        |> Async.map (fun _ -> subscriptions)
            }
            |> Async.StartAsTask

        /// <inheritdoc/>
        member _.Unsubscribe(email: string) =
            async {
                let! subscriptions =
                    daprClient.GetStateAsyncOr(StateStore.name, StateStore.subscriptions, Subscriptions())

                return!
                    match subscriptions.Remove(email) with
                    | true ->
                        daprClient.SaveStateAsync(StateStore.name, StateStore.subscriptions, subscriptions)
                        |> Async.AwaitTask
                        |> Async.map (fun _ -> subscriptions)
                    | false -> async { return subscriptions }
            }
            |> Async.StartAsTask
