using System;
using Rabbus.Errors;
using Rabbus.GuidGeneration;

namespace Rabbus.Returns
{
    internal interface IBasicReturnHandler
    {
        void Handle(BasicReturn basicReturn);
        void Subscribe(RabbusGuid messageId, Action<BasicReturn> callback);
    }
}