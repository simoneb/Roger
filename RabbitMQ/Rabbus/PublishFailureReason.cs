namespace Rabbus
{
    public class PublishFailureReason
    {
        public ushort ReplyCode { get; private set; }
        public string ReplyText { get; private set; }

        public PublishFailureReason(ushort replyCode, string replyText)
        {
            ReplyCode = replyCode;
            ReplyText = replyText;
        }
    }
}