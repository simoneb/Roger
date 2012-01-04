namespace Rabbus
{
    public class BasicReturn
    {
        public RabbusGuid MessageId { get; private set; }
        public ushort ReplyCode { get; private set; }
        public string ReplyText { get; private set; }

        internal BasicReturn(RabbusGuid messageId, ushort replyCode, string replyText)
        {
            MessageId = messageId;
            ReplyCode = replyCode;
            ReplyText = replyText;
        }
    }
}