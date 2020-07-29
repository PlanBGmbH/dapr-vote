extern alias Shaded;
using System.Threading.Tasks;
using Grpc.Core;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using Notifications.Grpc;
using Proto.Notifications;
using Shaded::Dapr.Client;

namespace Notifications.Services
{
    /// <summary>
    /// A service that handles subscriptions.
    /// </summary>
    public class NotificationService : Notification.NotificationBase
    {
        private readonly DaprClient _daprClient;
        private readonly IConfiguration _config;

        /// <summary>
        /// The class constructor.
        /// </summary>
        /// <param name="daprClient">An instance of the dapr client.</param>
        /// <param name="config">The application configuration.</param>
        public NotificationService(DaprClient daprClient, IConfiguration config)
        {
            _daprClient = daprClient;
            _config = config;
        }

        /// <summary>
        /// Subscribes for voting notifications.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        async public override Task<Response> Notify(NotificationRequest request, ServerCallContext context)
        {
            using (var smtpClient = new SmtpClient())
            {
                await smtpClient.ConnectAsync(_config["Smtp:Host"], int.Parse(_config["Smtp:Port"]), SecureSocketOptions.None);

                foreach (var subscription in request.Subscriptions)
                {
                    var mailMessage = new MimeMessage();
                    mailMessage.From.Add(new MailboxAddress("Voting service", "noreply@voting.com"));
                    mailMessage.To.Add(new MailboxAddress(subscription.Name, subscription.Email));
                    mailMessage.Subject = "New voting results";
                    mailMessage.Body = new TextPart("plain")
                    {
                        Text = $"Hello {subscription.Name}\n\n" +
                               "There are new voting results!\n" +
                               $"Cats: {request.Votes.Cats}\n" +
                               $"Dogs: {request.Votes.Dogs}"
                    };

                    await smtpClient.SendAsync(mailMessage);
                }
                await smtpClient.DisconnectAsync(true);
            }

            return new Response
            {
                Status = Response.Types.Status.Successful,
                Message = $"Successfully send notifications to {request.Subscriptions.Count} users"
            };
        }
    }
}
