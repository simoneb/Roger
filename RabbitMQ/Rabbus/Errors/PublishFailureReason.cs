using System;
using Rabbus.GuidGeneration;

namespace Rabbus.Errors
{
    public class PublishFailureReason
    {
        public RabbusGuid MessageId { get; private set; }
        public ushort ReplyCode { get; private set; }
        public string ReplyText { get; private set; }

        internal PublishFailureReason(RabbusGuid messageId, ushort replyCode, string replyText)
        {
            MessageId = messageId;
            ReplyCode = replyCode;
            ReplyText = replyText;
        }
    }
}