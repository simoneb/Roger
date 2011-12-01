namespace Rabbus.Errors
{
    internal static class ErrorMessages
    {
        internal static readonly string ReplyMessageNotAReply = string.Format("Reply message should be decorated with the {0} attribute", typeof(RabbusReplyAttribute).Name);
        internal const string ReplyInvokedOutOfRequestContext = "Reply can only be called when handling a request message";
    }
}