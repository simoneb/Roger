using System;

namespace Rabbus
{
    /// <summary>
    /// Identifies a message enabled to be published to RabbitMQ as a reply to another message
    /// </summary>
    /// <remarks>
    /// Reply messages don't need to specify an exchange name as they will be published back 
    /// on the same exchange where their corresponding request was published
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class RabbusReplyAttribute : Attribute
    {
    }
}