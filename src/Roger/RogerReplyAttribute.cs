using System;

namespace Roger
{
    /// <summary>
    /// Identifies a message enabled to be published to RabbitMQ as a reply to another message
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class RogerReplyAttribute : Attribute
    {
        public Type RequestType { get; private set; }

        public RogerReplyAttribute(Type requestType)
        {
            RequestType = requestType;
        }
    }
}