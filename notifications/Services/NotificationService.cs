using System;
using System.Threading.Tasks;
using Dapr.Actors.Client;
using Dapr.Client;
using Grpc.Core;
using Notifications.Actors;

namespace Notifications.Services
{
    /// <summary>
    /// A service that handles subscriptions.
    /// </summary>
    public class NotificationService : Notification.NotificationBase
    {
        private readonly DaprClient _daprClient;

        /// <summary>
        /// The class constructor.
        /// </summary>
        /// <param name="daprClient">An instance of the dapr client.</param>
        public NotificationService(DaprClient daprClient)
        {
            _daprClient = daprClient;
        }

        /// <summary>
        /// Subscribes for voting notifications.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        async public override Task<SubscriptionResponse> Subscribe(SubscriptionRequest request, ServerCallContext context)
        {
            var subscription = new Subscription(request.Email, request.Name);

            var proxy = ActorProxy.Create<ISubscriptionActor>(SubscriptionActor.ID, SubscriptionActor.Name);
            await proxy.Subscribe(subscription);

            return new SubscriptionResponse
            {
                Message = $"Successfully created subscription for user {request.Name} with email {request.Email}"
            };
        }

        /// <summary>
        /// Unsubscribe from voting notifications.
        /// </summary>
        /// <param name="request">The unsubscription request.</param>
        /// <param name="context">The gRPC server context.</param>
        /// <returns>The unsubscription response.</returns>
        async public override Task<UnsubscriptionResponse> Unsubscribe(UnsubscriptionRequest request, ServerCallContext context)
        {
            var proxy = ActorProxy.Create<ISubscriptionActor>(SubscriptionActor.ID, SubscriptionActor.Name);
            await proxy.Unsubscribe(request.Email);

            return new UnsubscriptionResponse
            {
                Message = $"Successfully removed subscription for email: {request.Email}"
            };
        }
    }
}
