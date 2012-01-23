using System;

namespace Roger.Internal
{
    internal interface IBasicReturnHandler
    {
        void Handle(BasicReturn basicReturn);
        void Subscribe(RogerGuid messageId, Action<BasicReturn> callback);
    }
}