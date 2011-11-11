using System.Collections.Generic;
using System.Threading;
using Common;
using MbUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Tests.Integration
{
    public class Publisher_confirms_behavior : With_rabbitmq_broker
    {
        private SortedSet<ulong> unconfirmed;
        private List<ulong > lost;

        [SetUp]
        public void Setup()
        {
            unconfirmed = new SortedSet<ulong>();
            lost = new List<ulong>();
        }

        [Test]
        public void Simple_confirms()
        {
            using (var connection = Helpers.CreateConnection())
            using (var model = connection.CreateModel())
            {
                model.ConfirmSelect();

                model.BasicAcks += ModelOnBasicAcks;
                model.BasicNacks += ModelOnBasicNacks;

                Start(() => Publisher(model));

                WaitUntilAllAckOrNack();
            }

            Assert.AreEqual(0, lost.Count);
        }

        private void Publisher(IModel model)
        {
            for (int i = 0; i < 10000; i++)
            {
                lock (unconfirmed)
                    unconfirmed.Add(model.NextPublishSeqNo);

                model.BasicPublish("", "publisher_confirms", null, i.Bytes());
            }
        }

        private void WaitUntilAllAckOrNack()
        {
            Thread.Sleep(100);

            while (true)
            {
                lock (unconfirmed)
                    if (unconfirmed.Count == 0)
                        break;

                Thread.Sleep(200);
            }
        }

        private void ModelOnBasicNacks(IModel model, BasicNackEventArgs args)
        {
            // beware that nack may be for multiple messages
            lock (unconfirmed)
                unconfirmed.Remove(args.DeliveryTag);

            lost.Add(args.DeliveryTag);
        }

        private void ModelOnBasicAcks(IModel model, BasicAckEventArgs args)
        {
            // beware that ack may be for multiple messages
            lock (unconfirmed)
                unconfirmed.Remove(args.DeliveryTag);
        }
    }
}