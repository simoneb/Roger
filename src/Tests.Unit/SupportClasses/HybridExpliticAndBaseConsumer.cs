using Rabbus;

namespace Tests.Unit.SupportClasses
{
    public class HybridExpliticAndBaseConsumer<TDerived, TBase> : IConsumer<TDerived>, Consumer<TBase>.SubclassesInSameAssembly where TDerived : class, TBase where TBase : class
    {
        public bool DerivedReceived;
        public bool BaseReceived;

        public void Consume(TDerived message)
        {
            DerivedReceived = true;
        }

        public void Consume(TBase message)
        {
            BaseReceived = true;
        }
    }
}