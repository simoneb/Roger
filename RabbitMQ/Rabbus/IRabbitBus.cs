using System;
using Rabbus.Errors;

namespace Rabbus
{
    public interface IRabbitBus : IDisposable
    {
        /// <summary>
        /// Contains the message information related to the message being currently handled
        /// </summary>
        CurrentMessageInformation CurrentMessage { get; }

        /// <summary>
        /// Returns the delay, in milliseconds, between connection retries when the connection shuts down
        /// </summary>
        int ConnectionAttemptMillisecondsInterval { get; }

        /// <summary>
        /// Initializes the bus
        /// </summary>
        void Initialize();

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
        /// <param name="publishFailure">A callback invoked when the message cannot be routed to any subscriber</param>
        void PublishMandatory(object message, Action<PublishFailureReason> publishFailure);

        /// <summary>
        /// Sends a request by means of <paramref name="message"/>, expecting a reply,
        /// but fails if there are no subscribers to which the message can be routed
        /// </summary>
        /// <param name="message">The request message</param>
        void Request(object message);

        /// <summary>
        /// Sends a request by means of <paramref name="message"/>, expecting a reply,
        /// but fails if there are no subscribers to which the message can be routed
        /// </summary>
        /// <param name="message">The request message</param>
        /// <param name="requestFailure">A callback invoked when the message cannot be routed to any subscriber</param>
        void Request(object message, Action<PublishFailureReason> requestFailure);

        /// <summary>
        /// Replies to a request sent by means of <see cref="Request(object)"/>
        /// </summary>
        /// <param name="message">The response message</param>
        void Reply(object message);

        /// <summary>
        /// Manually shove a message into the bus and let consumers consume it
        /// </summary>
        /// <param name="message">The message</param>
        void Consume(object message);

        /// <summary>
        /// Sends a message to a specific endpoint
        /// </summary>
        /// <param name="endpoint">The destination endpoint</param>
        /// <param name="message">The message</param>
        void Send(RabbusEndpoint endpoint, object message);

        /// <summary>
        /// Sends a message to a specific endpoint, communicating if there was a failure during publishing
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="message"></param>
        /// <param name="publishFailure"></param>
        void Send(RabbusEndpoint endpoint, object message, Action<PublishFailureReason> publishFailure);
    }
}