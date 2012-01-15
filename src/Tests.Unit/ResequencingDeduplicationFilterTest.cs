using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using MbUnit.Framework;
using NSubstitute;
using RabbitMQ.Client;
using RabbitMQ.Util;
using System.Linq;
using Roger;
using Roger.Internal;
using Roger.Internal.Impl;

namespace Tests.Unit
{
    [TestFixture]
    public class ResequencingDeduplicationFilterTest
    {
        private ResequencingDeduplicationFilter sut;
        private SharedQueue output;
        private SharedQueue input;
        private IModel model;

        [SetUp]
        public void Setup()
        {
            sut = new ResequencingDeduplicationFilter();
            output = new SharedQueue();
            input = new SharedQueue();
            model = Substitute.For<IModel>();
            StartFiltering().WaitForStart();
        }

        [TearDown]
        public void Teardown()
        {
            input.Close();
            output.Close();
        }

        [Test]
        public void Should_return_ordered_messages_when_already_ordered()
        {
            Delivering(1, 2, 3).WillReceiveJustThat();
        }

        [Test]
        public void Should_filter_simple_hole()
        {
            Delivering(1).WillReceiveJustThat();

            Delivering(3).WillNotReceiveAnything();

            Delivering(2).WillReceiveOnly(2, 3);
        }

        [Test]
        public void Should_filter_bigger_hole()
        {
            Delivering(1).WillReceiveJustThat();

            Delivering(3, 4, 5, 6).WillNotReceiveAnything();

            Delivering(2).WillReceiveOnly(2, 3, 4, 5, 6);
        }

        [Test]
        public void Should_handle_multiple_holes()
        {
            Delivering(1).WillReceiveJustThat();

            Delivering(3).WillNotReceiveAnything();

            Delivering(5, 6, 7).WillNotReceiveAnything();

            Delivering(2).WillReceiveOnly(2, 3);

            Delivering(4).WillReceiveOnly(4, 5, 6, 7);
        }

        [Test]
        public void Should_return_message_with_random_sequence_if_first_time_seen()
        {
            Delivering(100).WillReceiveJustThat();
        }

        [Test]
        public void Should_ignore_messages_with_lower_sequence()
        {
            Delivering(4, 5, 6, 3).WillReceiveOnly(4, 5, 6);
        }

        [Test]
        public void Should_ack_messages_with_lower_sequence_as_they_are_duplicates()
        {
            Delivering(4, 5, 6, 3).WillReceiveOnly(4, 5, 6);
            model.Received().BasicAck(3, false);
        }

        [Test]
        public void Should_handle_sequences_using_publisher_endpoint_as_discriminator()
        {
            Delivering(Message(1, "queue1"), Message(1, "queue2")).WillReceiveJustThat();
        }

        private Delivery Delivering(params uint[] i)
        {
            foreach (var u in i)
                input.Enqueue(Message(u));

            return new Delivery(output, i);
        }

        private Delivery Delivering(params CurrentMessageInformation[] i)
        {
            foreach (var u in i)
                input.Enqueue(u);

            return new Delivery(output, Bodies(i));
        }

        private class Delivery
        {
            private readonly SharedQueue output;
            private readonly uint[] delivered;

            public Delivery(SharedQueue output, uint[] delivered)
            {
                this.output = output;
                this.delivered = delivered;
            }

            public void WillReceiveOnly(params uint[] values)
            {
                foreach (var value in values)
                {
                    object received;
                    Assert.IsTrue(output.Dequeue(500, out received), "Couldn't wait for {0}", value);
                    Assert.AreEqual(value, Value(received));
                }

                WillNotReceiveAnything();
            }

            private static uint? Value(object received)
            {
                return received != null
                           ? (uint) ((CurrentMessageInformation) received).Body
                           : (uint?) null;
            }

            public void WillNotReceiveAnything()
            {
                object _;
                Assert.IsFalse(output.Dequeue(100, out _), "Unexpected receive: {0}", Value(_));
            }

            public void WillReceiveJustThat()
            {
                WillReceiveOnly(delivered);
            }
        }

        private Task StartFiltering()
        {
            return Task.Factory.StartNew(() =>
            {
                foreach (var m in sut.Filter(input.Cast<CurrentMessageInformation>(), model))
                    output.Enqueue(m);
            });
        }

        private static uint[] Bodies(IEnumerable<CurrentMessageInformation> filter)
        {
            return filter.Select(f => f.Body).Cast<uint>().ToArray();
        }

        private static CurrentMessageInformation Message(uint sequenceAndBody, string endpoint = null)
        {
            return new CurrentMessageInformation
            {
                Headers = new Hashtable {{Headers.Sequence, BitConverter.GetBytes(sequenceAndBody)}},
                Body = sequenceAndBody,
                Endpoint = new RogerEndpoint(endpoint ?? "someQueue"),
                DeliveryTag = sequenceAndBody
            };
        }
    }
}