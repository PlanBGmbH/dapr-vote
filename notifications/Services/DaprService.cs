using System;
using System.Threading.Tasks;
using Dapr.AppCallback.Autogen.Grpc.v1;
using Dapr.Client.Autogen.Grpc.v1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Notifications.Grpc;
using Proto;
using Proto.Notifications;
using Shared;

namespace Notifications.Services
{
    /// <summary>
    /// Implementation of the `AppCallback` service.
    /// </summary>
    public class DaprService : AppCallback.AppCallbackBase
    {
        private readonly NotificationService _notificationService;

        /// <summary>
        /// The class constructor.
        /// </summary>
        /// <param name="notificationService"></param>
        public DaprService(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <inheritdoc/>
        public override Task<InvokeResponse> OnInvoke(InvokeRequest request, ServerCallContext context)
        {
            return request.Method switch
            {
                "Notify" =>
                    ProxyGrpc<NotificationRequest, Response>(request, context, _notificationService.Notify),
                _ => Task.FromResult(new InvokeResponse
                {
                    Data = AnyConverter.ToAny(new Response
                    {
                        Status = Response.Types.Status.Failure,
                        Message = $"Unexpected service method: {request.Method}"
                    }, Config.jsonSerializerOptions)
                })
            };
        }

        /// <inheritdoc/>
        public override Task<ListInputBindingsResponse> ListInputBindings(Empty request, ServerCallContext context)
        {
            return Task.FromResult(new ListInputBindingsResponse());
        }

        /// <inheritdoc/>
        public override Task<ListTopicSubscriptionsResponse> ListTopicSubscriptions(Empty request, ServerCallContext context)
        {
            return Task.FromResult(new ListTopicSubscriptionsResponse());
        }

        /// <summary>
        /// A helper method which proxies the incoming gRPC request to the given gRPC API.
        /// </summary>
        /// <param name="request">The incoming request.</param>
        /// <param name="context">The server call context.</param>
        /// <param name="func">The gRPC service method to invoke with the extracted request.</param>
        /// <typeparam name="TR">The type of the request passed to the gRPC API.</typeparam>
        /// <typeparam name="TP">The type of the response returned from the gRPC API.</typeparam>
        /// <returns></returns>
        private static async Task<InvokeResponse> ProxyGrpc<TR, TP>(
            InvokeRequest request,
            ServerCallContext context,
            Func<TR, ServerCallContext, Task<TP>> func
        ) {
            TP response = await func(AnyConverter.FromAny<TR>(request.Data, Config.jsonSerializerOptions), context);

            return new InvokeResponse {Data = AnyConverter.ToAny(response, Config.jsonSerializerOptions)};
        }
    }
}
