using System;

namespace Rabbus
{
    /// <summary>
    /// Identifies a message enabled to be published to RabbitMQ
    /// </summary>
    /// <remarks>
    /// Every message type should be decorated with this attribute specifying a valid 
    /// exchange name where the message will be published
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class RabbusMessageAttribute : Attribute
    {
        public RabbusMessageAttribute(string exchange)
        {
            Exchange = exchange;
        }

        public string Exchange { get; private set; }
    }
}