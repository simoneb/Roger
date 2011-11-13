using System;
using Rabbus;
using Rabbus.Resolvers;

namespace Tests.Integration.Bus
{
    public class Demo
    {
        private static IRabbitBus Bus;

        static Demo()
        {
            main();
        }

        class HelloMessage
        { }

        class HelloMessageConsumer : IConsumer<HelloMessage>
        {
            public void Consume(HelloMessage message)
            {
                Console.WriteLine("Received Hello!");
            }
        }

        static void main()
        {
            // your favorite IoC container here
            var consumerResolver = new ManualRegistrationConsumerResolver(
                                    new DefaultSupportedMessageTypesResolver());

            consumerResolver.Register(new HelloMessageConsumer());

            Bus = new DefaultRabbitBus(new DefaultConnectionFactory("localhost"), consumerResolver);
            Bus.Initialize();

            Bus.Publish(new HelloMessage());
        }
    }

}