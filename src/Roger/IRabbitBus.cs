using System;
using System.Threading.Tasks;

namespace Roger
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
        RogerEndpoint LocalEndpoint { get; }

        event Action Started;
        event Action Stopped;

        /// <summary>
        /// Starts the bus asynchronously
        /// </summary>
        Task<IRabbitBus> StartAsync();

        /// <summary>
        /// Starts the bus by blocking until the connection has been established
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
        /// <param name="persistent"> </param>
        void Publish(object message, bool persistent = true);

        /// <summary>
        /// Publishes a message so that every subscriber will receive it, 
        /// but fails if there are no subscribers to which the message can be routed
        /// </summary>
        /// <param name="message">The message to be published</param>
        /// <param name="basicReturnCallback">A callback invoked when the message cannot be routed to any subscriber</param>
        /// <param name="persistent"> </param>
        void PublishMandatory(object message, Action<BasicReturn> basicReturnCallback = null, bool persistent = true);

        /// <summary>
        /// Sends a request by means of <paramref name="message"/>, expecting a reply,
        /// but fails by calling the optional <paramref name="basicReturnCallback"/> if 
        /// there are no subscribers to which the message can be routed
        /// </summary>
        /// <param name="message">The request message</param>
        /// <param name="basicReturnCallback">A callback invoked when the message cannot be routed to any subscribers</param>
        /// <param name="persistent"> </param>
        void Request(object message, Action<BasicReturn> basicReturnCallback = null, bool persistent = true);

        /// <summary>
        /// Replies to a request sent by means of <see cref="Request(object)"/>
        /// </summary>
        /// <param name="message">The response message</param>
        /// <param name="basicReturnCallback">A callback invoked when the message cannot be routed to any subscribers</param>
        /// <param name="persistent"> </param>
        void Reply(object message, Action<BasicReturn> basicReturnCallback = null, bool persistent = true);

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
        /// <param name="persistent"> </param>
        void Send(RogerEndpoint endpoint, object message, Action<BasicReturn> basicReturnCallback = null, bool persistent = true);

        /// <summary>
        /// Fired when either an attempt at connecting to the server is unsuccessful or an existing connection is shut down unexpectedly
        /// </summary>
        event Action Interrupted;
    }
}