using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Text.RegularExpressions;

namespace Roger.MEF
{
    public class MEFConsumerContainer : IConsumerContainer
    {
        private readonly CompositionContainer container;
        readonly Regex importRegex = new Regex(@"Roger.IConsumer\(.+\)");

        public MEFConsumerContainer(CompositionContainer container)
        {
            this.container = container;
        }

        public IEnumerable<IConsumer> Resolve(Type consumerType)
        {
            return container.GetExports(new ImportDefinition(e => e.ContractName.Equals(AttributedModelServices.GetContractName(consumerType)), null, ImportCardinality.ZeroOrMore, false, true))
                .Select(export => export.Value).Cast<IConsumer>();
        }

        public void Release(IEnumerable<IConsumer> consumers)
        {
        }

        public IEnumerable<Type> GetAllConsumerTypes()
        {
            return container.GetExports(new ImportDefinition(e => importRegex.IsMatch(e.ContractName), null, ImportCardinality.ZeroOrMore, false, true))
                .Select(export => export.Value.GetType());
        }
    }
}
