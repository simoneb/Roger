using System;

namespace Rabbus.Errors
{
    public class PublishFailureReason
    {
        public Guid MessageId { get; private set; }
        public ushort ReplyCode { get; private set; }
        public string ReplyText { get; private set; }

        internal PublishFailureReason(Guid messageId, ushort replyCode, string replyText)
        {
            MessageId = messageId;
            ReplyCode = replyCode;
            ReplyText = replyText;
        }
    }
}