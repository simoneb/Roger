using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Roger.Messages 
{
    internal interface IReceive<T>
    {
        void Receive(T message);
    }

    internal interface IAggregator 
    {
        void Subscribe(object subscriber);

        void Unsubscribe(object subscriber);

        void Notify(object message);
    }

    internal class Aggregator : IAggregator
    {
        readonly List<SubscriptionProxy> subscriptions = new List<SubscriptionProxy>();

        public void Subscribe(object subscriber) 
        {
            lock(subscriptions) 
            {
                if (subscriptions.Any(x => x.Proxies(subscriber)))
                    return;

                subscriptions.Add(new SubscriptionProxy(subscriber));
            }
        }

        public void Unsubscribe(object subscriber) 
        {
            lock(subscriptions) 
            {
                var found = subscriptions.FirstOrDefault(x => x.Proxies(subscriber));

                if (found != null)
                    subscriptions.Remove(found);
            }
        }

        public void Notify(object message) 
        {
            SubscriptionProxy[] toNotify;
            var messageType = message.GetType();

            lock (subscriptions)
                toNotify = subscriptions.ToArray();

            var toRemove = toNotify
                .Where(s => !s.Receive(messageType, message))
                .ToList();

            if (toRemove.Any())
                lock (subscriptions)
                    toRemove.ForEach(x => subscriptions.Remove(x));
        }

        private class SubscriptionProxy 
        {
            readonly WeakReference subscriber;
            readonly Dictionary<Type, MethodInfo> subscribersByMessageType = new Dictionary<Type, MethodInfo>();

            public SubscriptionProxy(object subscriber) 
            {
                this.subscriber = new WeakReference(subscriber);

                var interfaces = subscriber.GetType()
                                 .GetInterfaces()
                                 .Where(i => i.IsGenericType && typeof(IReceive<>) == i.GetGenericTypeDefinition());

                foreach(var @interface in interfaces) 
                {
                    var messageType = @interface.GetGenericArguments()[0];
                    var method = @interface.GetMethod("Receive");
                    subscribersByMessageType[messageType] = method;
                }
            }

            public bool Proxies(object instance) 
            {
                return subscriber.Target == instance;
            }

            public bool Receive(Type messageType, object message) 
            {
                var target = subscriber.Target;

                if (target == null)
                    return false;

                MethodInfo method;

                if (subscribersByMessageType.TryGetValue(messageType, out method))
                    method.Invoke(target, new[] {message});

                return true;
            }
        }
    }
}