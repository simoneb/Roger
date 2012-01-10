using System;

namespace Roger.Internal
{
    public interface IBasicReturnHandler
    {
        void Handle(BasicReturn basicReturn);
        void Subscribe(RogerGuid messageId, Action<BasicReturn> callback);
    }
}