namespace Roger
{
    public class BasicReturn
    {
        public RogerGuid MessageId { get; private set; }
        public ushort ReplyCode { get; private set; }
        public string ReplyText { get; private set; }

        internal BasicReturn(RogerGuid messageId, ushort replyCode, string replyText)
        {
            MessageId = messageId;
            ReplyCode = replyCode;
            ReplyText = replyText;
        }
    }
}