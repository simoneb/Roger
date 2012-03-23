using System;

namespace ManagedShovel
{
    class Program
    {
        static void Main(string[] args)
        {
            var shovel1 = ManagedShovel.From("amqp://localhost")
                                     .Declarations(m => m.ExchangeDeclare("TestExchange", "topic"),
                                                   m => m.QueueBind(m.QueueDeclare(), "TestExchange", "#"))
                                     .To("amqp://localhost/secondary")
                                     .Declarations(m => m.ExchangeDeclare("TestExchange", "topic"))
                                     //.UseQueue("someQueue")
                                     .UseLastCreatedQueue()
                                     .PrefetchCount(50)
                                     .AckMode(AckMode.OnConfirm)
                                     .PublishProperties(p => p.ContentType = "x-protobuf")
                                     //.PublishFields("myExchange", "myRoutingKey")
                                     .ReconnectDelay(TimeSpan.FromSeconds(5))
                                     .MaxHops(0)
                                     .Start();

            var shovel2 = ManagedShovel.From("amqp://localhost/secondary")
                                     .Declarations(m => m.ExchangeDeclare("TestExchange", "topic"),
                                                   m => m.QueueBind(m.QueueDeclare(), "TestExchange", "#"))
                                     .To("amqp://localhost/")
                                     .Declarations(m => m.ExchangeDeclare("TestExchange", "topic"))
                //.UseQueue("someQueue")
                                     .UseLastCreatedQueue()
                                     .PrefetchCount(50)
                                     .AckMode(AckMode.OnConfirm)
                                     .PublishProperties(p => p.ContentType = "x-protobuf")
                //.PublishFields("myExchange", "myRoutingKey")
                                     .ReconnectDelay(TimeSpan.FromSeconds(5))
                                     .MaxHops(0)
                                     .Start();

            Console.ReadLine();
                         
            shovel1.Stop();
            shovel2.Stop();
        }
    }
}