namespace Notifications
{
    /// <summary>
    /// Represents a subscription.
    /// </summary>
    readonly public struct Subscription
    {
        /// <summary>
        /// Constructs a new subscription.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="name"></param>
        public Subscription(string email, string name)
        {
            this.Email = email;
            this.Name = name;
        }

        /// <summary>
        /// The email address of the subscriber.
        /// </summary>
        public string Email { get; }

        /// <summary>
        /// The subscribers name.
        /// </summary>
        public string Name { get; }
    }
}
