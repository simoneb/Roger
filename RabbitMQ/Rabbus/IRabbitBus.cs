using System;

namespace Rabbus
{
    /// <summary>
    /// Main entrypoint to the library
    /// </summary>
    public interface IRabbitBus : IDisposable
    {
        /// <summary>
        /// Contains the message information related to the message being currently handled
        /// </summary>
        CurrentMessageInformation CurrentMessage { get; }

        /// <summary>
        /// Returns the delay between connection retries when the either connection cannot be estabilished or it shuts down
        /// </summary>
        TimeSpan ConnectionAttemptInterval { get; }

        /// <summary>
        /// The local endpoint on which this instance of the bus is listening for incoming messages
        /// </summary>
        RabbusEndpoint LocalEndpoint { get; }

        /// <summary>
        /// Starts the bus
        /// </summary>
        void Start();

        /// <summary>
        /// Subscribes a consumer manually to the messages it is interested in
        /// This is not usually necessary as consumer subscription is carried out automatically
        /// </summary>
        /// <param name="instanceConsumer">The consumer instance</param>
        /// <returns>A subscription token which, when disposed, removes the subscription</returns>
        IDisposable AddInstanceSubscription(IConsumer instanceConsumer);

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
        /// <param name="basicReturnCallback">A callback invoked when the message cannot be routed to any subscriber</param>
        void PublishMandatory(object message, Action<BasicReturn> basicReturnCallback = null);

        /// <summary>
        /// Sends a request by means of <paramref name="message"/>, expecting a reply,
        /// but fails by calling the optional <paramref name="basicReturnCallback"/> if 
        /// there are no subscribers to which the message can be routed
        /// </summary>
        /// <param name="message">The request message</param>
        /// <param name="basicReturnCallback">A callback invoked when the message cannot be routed to any subscribers</param>
        void Request(object message, Action<BasicReturn> basicReturnCallback = null);

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
        /// Sends a message to a specific endpoint, communicating if there was a failure during publishing
        /// </summary>
        /// <param name="endpoint">The recipient endpoint of the message</param>
        /// <param name="message">The message</param>
        /// <param name="basicReturnCallback">A callback invoked if the message could not be routed to the endpoint</param>
        void Send(RabbusEndpoint endpoint, object message, Action<BasicReturn> basicReturnCallback = null);

        /// <summary>
        /// Fired when the bus is started successfully
        /// </summary>
        event Action Started;

        /// <summary>
        /// Fired when the connection to the server is either unavailable or lost
        /// </summary>
        event Action ConnectionFailure;
    }
}