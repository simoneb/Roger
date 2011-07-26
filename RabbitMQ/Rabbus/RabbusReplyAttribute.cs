using System;

namespace Rabbus
{
    /// <summary>
    /// Identifies a message enabled to be published to RabbitMQ as a reply to another message
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class RabbusReplyAttribute : Attribute
    {
    }
}