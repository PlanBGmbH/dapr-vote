namespace Votes.Actors

open System
open Dapr.Client
open Dapr.Actors
open Dapr.Actors.Runtime
open Dapr.Client.Http
open FSharpx.Control
open Shared.Config
open Shared.Extensions
open System.Threading.Tasks
open Shared
open Proto

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
    /// The name of the notify reminder.
    /// </summary>
    [<Literal>]
    let NotifyReminder = "NotifyReminder"

/// <summary>
/// An actor that is responsible for handling the votes.
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

    let stateManager = base.StateManager

    /// <summary>
    /// Indicates if someone has voted and that a notification update should be sent to subscribed users.
    /// </summary>
    let mutable sendNotificationAfterVote = false

    // <summary>
    /// Sends a notification to subscribed users.
    /// </summary>
    let SendNotification() =
        // Disable notification sending, will be activated again after a new vote
        sendNotificationAfterVote <- false
        async {
            let httpExtension = HTTPExtension()
            httpExtension.Verb <- HTTPVerb.Get

            let! votes = daprClient.GetStateAsyncOr(StateStore.name, Apps.votes, Votes.empty)
            let! subscriptions =
                daprClient.InvokeMethodAsync<List<Subscription>>(Apps.subscriptions, "subscriptions", httpExtension)
                    .AsTask() |> Async.AwaitTask

            let grpcRequest =
                subscriptions
                |> List.fold (fun (acc: Notifications.NotificationRequest) subscription ->
                    acc.Subscriptions.Add(
                        Notifications.Subscription(
                            Name = subscription.Name,
                            Email = subscription.Email))
                    acc
                ) (Notifications.NotificationRequest())

            grpcRequest.Votes <- Notifications.Votes(Cats = votes.Cats, Dogs = votes.Dogs)

            return!
                daprClient.InvokeMethodAsync<Notifications.NotificationRequest>(Apps.notifications, "Notify", grpcRequest)
                |> Async.AwaitTask
        }

    /// <summary>
    /// Tries to get the `sendNotificationAfterVote` actor state on actor activation.
    /// </summary>
    override this.OnActivateAsync() =
        this.RegisterReminders()
        async {
            let! state = stateManager.GetStateAsyncOr<bool>("sendNotificationAfterVote", false)

            sendNotificationAfterVote <- state

            return ()
        }
        |> Async.StartAsTask :> Task

    /// <summary>
    /// Sets the `sendNotificationAfterVote` actor state on actor deactivation.
    /// </summary>
    override this.OnDeactivateAsync() =
        async {
            stateManager.SetStateAsync<bool>("sendNotificationAfterVote", sendNotificationAfterVote)
            |> Async.AwaitTask
            |> ignore
        }
        |> Async.StartAsTask :> Task

    /// <summary>
    /// Registers the reminders.
    /// </summary>
    member this.RegisterReminders() =
        let now = DateTime.Now
        let secondsToNextHour = TimeSpan(0, 0, 3600 - (now.Minute * 60) - now.Second)

        // Starts the reminder every full hour starting on the next full hour
        this.RegisterReminderAsync(VotingActor.NotifyReminder, null, secondsToNextHour, TimeSpan.FromHours(1.0))
        |> ignore

    /// <summary>
    /// Implementation of the `IVotingActor` interface.
    /// </summary>
    interface IVotingActor with

        /// <inheritdoc/>
        member _.Vote(animal: Animal) =
            // A new notification should be sent the next time the timer (reminder) ticks
            sendNotificationAfterVote <- true
            async {
                let! votes = daprClient.GetStateAsyncOr(StateStore.name, Apps.votes, Votes.empty)
                let updatedVotes = votes.Vote(animal)

                return! daprClient.SaveStateAsync<Votes>(StateStore.name, Apps.votes, updatedVotes)
                |> Async.AwaitTask
                |> Async.map(fun _ -> updatedVotes)
            }
            |> Async.StartAsTask

    /// <summary>
    /// Implementation of the `IRemindable` interface.
    /// </summary>
    interface IRemindable with

        /// <inheritdoc/>
        member _.ReceiveReminderAsync(reminderName: string, _: byte[], _: TimeSpan, _: TimeSpan) =
            async {
                match reminderName with
                | VotingActor.NotifyReminder ->
                    return! SendNotification()
                | _ ->
                    return ()
            }
            |> Async.StartAsTask :> Task
