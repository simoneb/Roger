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
using Tests.Unit.SupportClasses;

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
        [Row("someQueue", typeof(MyMessage), "someQueue", typeof(MyMessage), true)]
        [Row("someQueue", typeof(MyMessage), "someOtherQueue", typeof(MyMessage), false)]
        [Row("someQueue", typeof(MyMessage), "someQueue", typeof(MyDerivedMessage), true)]
        [Row("someOtherQueue", typeof(MyMessage), "someQueue", typeof(MyDerivedMessage), false)]
        public void Sequence_key_should_handle_equality_correctly(string firstEndpoint, Type firstMessage, string secondEndpoint, Type secondMessage, bool outcome)
        {
            var first = new ResequencingDeduplicationFilter.SequenceKey(new RogerEndpoint(firstEndpoint), firstMessage);
            var second = new ResequencingDeduplicationFilter.SequenceKey(new RogerEndpoint(secondEndpoint), secondMessage);

            Assert.AreEqual(outcome, first.Equals(second));
            Assert.AreEqual(outcome, first.GetHashCode().Equals(second.GetHashCode()));
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
        public void Should_handle_sequences_using_publisher_endpoint_as_discriminator_for_same_type_of_message()
        {
            Delivering(Message(1, "queue1"), Message(1, "queue2")).WillReceiveJustThat();
        }

        [Test]
        public void Should_treat_messages_of_same_hierarchy_as_same_message_when_from_same_endpoint()
        {
            Delivering(Message<MyMessage>(1), Message<MyDerivedMessage>(2)).WillReceiveJustThat();            
        }

        [Test]
        public void Should_treat_messages_of_same_hierarchy_as_same_message_when_from_same_endpoint_deduplicating_them_when_same_sequence()
        {
            Delivering(Message<MyMessage>(1), Message<MyDerivedMessage>(1)).WillReceiveOnly(1);
        }

        [Test]
        public void Should_discriminate_using_message_type_for_messages_belonging_to_different_hierarchies_regardless_of_endpoint()
        {
            Delivering(Message<MyMessage>(1), Message<MyOtherMessage>(1)).WillReceiveJustThat();            
        }

        [Test]
        public void Should_pass_through_messages_without_sequence()
        {
            Delivering(MessageWithoutSequence<MyMessage>(1)).WillReceiveJustThat();
        }

        [Test]
        public void Should_block_messages_without_headers()
        {
            Delivering(MessageWithoutHeaders<MyMessage>(1)).WillNotReceiveAnything();
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

        private static CurrentMessageInformation Message<T>(uint sequenceBodyAndDeliveryTag, string endpoint = null)
        {
            return Message<T>(sequenceBodyAndDeliveryTag,
                              new Hashtable {{Headers.Sequence, BitConverter.GetBytes(sequenceBodyAndDeliveryTag)}},
                              endpoint);
        }

        private static CurrentMessageInformation Message<T>(uint sequenceBodyAndDeliveryTag,
                                                            Hashtable headers,
                                                            string endpoint = null)
        {
            return new CurrentMessageInformation
            {
                Headers = headers,
                Body = sequenceBodyAndDeliveryTag,
                Endpoint = new RogerEndpoint(endpoint ?? "someQueue"),
                DeliveryTag = sequenceBodyAndDeliveryTag,
                MessageType = typeof (T)
            };
        }

        private CurrentMessageInformation MessageWithoutHeaders<T>(uint bodyAndDeliveryTag)
        {
            return Message<T>(bodyAndDeliveryTag, (Hashtable)null);
        }

        private CurrentMessageInformation MessageWithoutSequence<T>(uint bodyAndDeliveryTag)
        {
            return Message<T>(bodyAndDeliveryTag, new Hashtable());
        }

        private static CurrentMessageInformation Message(uint sequenceBodyAndDeliveryTag, string endpoint = null)
        {
            return Message<MyMessage>(sequenceBodyAndDeliveryTag, endpoint);
        }
    }
}