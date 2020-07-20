extern alias Shaded;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapr.Actors;
using Dapr.Actors.Runtime;
using Shaded.Dapr.Client;
using Shared;

namespace Notifications.Actors
{
    /// <summary>
    /// An actor that is responsible for handling the subscriptions in the state store.
    /// </summary>
    public interface ISubscriptionActor : IActor
    {
        /// <summary>
        /// Adds the given subscriptions to the state store.
        /// </summary>
        /// <param name="subscription">The subscription data.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task Subscribe(Subscription subscription);

        /// <summary>
        /// Removes the subscription for the given email from the state store.
        /// </summary>
        /// <param name="email">The email to unsubscribe.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task Unsubscribe(string email);
    }

    /// <summary>
    /// Concrete subscription actor implementation.
    ///
    /// Storing all subscriptions under a single key, may be an overhead for a large set of subscriptions. For
    /// production usage, an other storage format may be better.
    /// </summary>
    [Actor(TypeName = SubscriptionActor.Name)]
    public class SubscriptionActor : Actor, ISubscriptionActor
    {
        /// <summary>
        /// The name of the actor.
        /// </summary>
        public const string Name = "SubscriptionActor";

        /// <summary>
        /// The ID of the actor.
        /// </summary>
        public static ActorId ID = new ActorId("subscription");

        private readonly DaprClient _daprClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionActor"/> class.
        /// </summary>
        /// <param name="actorService">The <see cref="P:Dapr.Actors.Runtime.Actor.ActorService" /> that will host this actor instance.</param>
        /// <param name="actorId">ID for the actor.</param>
        /// <param name="daprClient">A dapr client instance.</param>
        public SubscriptionActor(ActorService actorService, ActorId actorId, DaprClient daprClient)
            : base(actorService, actorId)
        {
            _daprClient = daprClient;
        }

        /// <inheritdoc/>
        async public Task Subscribe(Subscription subscription)
        {
            var subscriptions = await _daprClient.GetStateAsync<Dictionary<string, Subscription>>(
                Config.StateStore.name,
                Config.StateStore.subscriptions);

            Subscription? value = subscriptions.GetValueOrDefault(subscription.Email);
            // Add a new subscription if not exists or updates the subscription if the name has changed
            if (!value.HasValue || value.Value.Name != subscription.Name)
            {
                subscriptions[subscription.Email] = subscription;

                await _daprClient.SaveStateAsync(
                    Config.StateStore.name,
                    Config.StateStore.subscriptions,
                    subscriptions);
            }
        }

        /// <inheritdoc/>
        async public Task Unsubscribe(string email)
        {
            var subscriptions = await _daprClient.GetStateAsync<Dictionary<string, Subscription>>(
                Config.StateStore.name,
                Config.StateStore.subscriptions);

            var updated = subscriptions.Remove(email);
            if (updated)
            {
                await _daprClient.SaveStateAsync(
                    Config.StateStore.name,
                    Config.StateStore.subscriptions,
                    subscriptions);
            }

        }
    }
}
