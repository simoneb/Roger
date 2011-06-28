using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Shoveling.Test.Bus
{
    public abstract class RabbitObserver<T> : IRabbitObserver<T>
    {
        public abstract void OnNext(RabbitMessage<T> value);

        public void OnNext(RabbitMessage value)
        {
            var context = (from method in GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                           where method.Name == "OnNext"
                           let messageType = method.GetParameters().Single().ParameterType.GetGenericArguments().Single()
                           where messageType == value.Type
                           select new {method, messageType})
                .Single();

            var makeGenericType = typeof(RabbitMessage<>).MakeGenericType(context.messageType);
            context.method.Invoke(this, new[]{Activator.CreateInstance(makeGenericType, value)});
        }

        public abstract void OnError(Exception error);
        public abstract void OnCompleted();

        public void AddBindingsTo(ICollection<string> collector)
        {
            //collector.Add("");
            // 1 extract interfaces
            // 2 extract generic types from interfaces
            // 3 add bindings
        }

        public abstract class And<V> : RabbitObserver<T>, IRabbitObserver<V>
        {
            public abstract void OnNext(RabbitMessage<V> value);
        }
    }
}