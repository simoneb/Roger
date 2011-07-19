using System;

namespace Rabbus
{
    public interface IRabbitBus
    {
        /// <summary>
        /// Subscribes a consumer manually to the messages it is interested in
        /// This is not usually necessary as consumer subscription is carried out automatically
        /// </summary>
        /// <param name="consumer">The consumer instance</param>
        /// <returns>A subscription token which, when disposed, removes the subscription</returns>
        IDisposable AddInstanceSubscription(IConsumer consumer);

        /// <summary>
        /// Publishes a message so that subscribers will receive it
        /// </summary>
        /// <param name="message">The message to be published</param>
        void Publish(object message);

        /// <summary>
        /// Publishes a message so that every subscriber will receive it, 
        /// but fails if there are no subscribers to which the message can be routed
        /// </summary>
        /// <param name="message">The message to be published</param>
        /// <param name="publishFailure">A callback invoked when the message cannot be routed to any subscribers</param>
        void PublishMandatory(object message, Action<PublishFailureReason> publishFailure);

        /// <summary>
        /// Sends a request by means of <paramref name="message"/>, expecting a reply
        /// </summary>
        /// <param name="message">The request message</param>
        void Request(object message);

        /// <summary>
        /// Replies to a request sent by means of <see cref="Request"/>
        /// </summary>
        /// <param name="message">The response message</param>
        void Reply(object message);

        /// <summary>
        /// Contains the message information related to the message being handled currently
        /// </summary>
        CurrentMessageInformation CurrentMessage { get; }
    }
}