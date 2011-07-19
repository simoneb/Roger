using System;

namespace Rabbus
{
    public class ReplyFailureReason
    {
        public AggregateException Exception { get; private set; }

        public ReplyFailureReason(AggregateException exception)
        {
            Exception = exception;
        }
    }
}