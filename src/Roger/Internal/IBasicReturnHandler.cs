using System;

namespace Rabbus.Internal
{
    internal interface IBasicReturnHandler
    {
        void Handle(BasicReturn basicReturn);
        void Subscribe(RabbusGuid messageId, Action<BasicReturn> callback);
    }
}