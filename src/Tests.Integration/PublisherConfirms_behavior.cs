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

        [SetUp]
        public void Setup()
        {
            unconfirmed = new SortedSet<ulong>();
        }

        [Test]
        public void Broker_should_confirm_all_messages()
        {
            using (var connection = Helpers.CreateConnection())
            using (var model = connection.CreateModel())
            {
                model.ConfirmSelect();

                model.BasicAcks += ModelOnBasicAcks;
                model.BasicNacks += ModelOnBasicNacks;

                Start(() => Publisher(model));

                WaitForConfirms();
            }

            Assert.AreEqual(0, unconfirmed.Count);
        }

        [Test]
        public void The_first_delivery_tag_of_a_channel_should_be_1()
        {
            using (var connection = Helpers.CreateConnection())
            using (var model = connection.CreateModel())
            {
                model.ConfirmSelect();

                Assert.AreEqual(1ul, model.NextPublishSeqNo);
            }
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

        private void WaitForConfirms()
        {
            // wait for publisher to start publishing
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
            MarkAsConfirmed(args.DeliveryTag, args.Multiple);
        }

        private void ModelOnBasicAcks(IModel model, BasicAckEventArgs args)
        {
            MarkAsConfirmed(args.DeliveryTag, args.Multiple);
        }

        private void MarkAsConfirmed(ulong deliveryTag, bool multiple)
        {
            if (multiple)
            {
                lock (unconfirmed)
                    unconfirmed.RemoveWhere(tag => tag <= deliveryTag);
            }
            else
            {
                lock (unconfirmed)
                    unconfirmed.Remove(deliveryTag);
            }
        }
    }
}