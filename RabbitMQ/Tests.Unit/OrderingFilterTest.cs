using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MbUnit.Framework;
using RabbitMQ.Util;
using Rabbus;
using Rabbus.Filters;
using System.Linq;

namespace Tests.Unit
{
    [TestFixture]
    public class OrderingFilterTest
    {
        private OrderingFilter sut;

        [SetUp]
        public void Setup()
        {
            sut = new OrderingFilter();
        }

        [Test]
        public void Should_return_ordered_messages_when_already_ordered()
        {
            Assert.AreEqual(new uint[]{1,2,3}, Bodies(sut.Filter(Messages(1, 2, 3))));
        }

        [Test]
        public void Should_not_return_messages_until_ordered()
        {
            var result = new List<CurrentMessageInformation>();
            var sharedQueue = new SharedQueue();
            var handle = new AutoResetEvent(false);

            Task.Factory.StartNew(() =>
            {
                foreach (var m in sut.Filter(sharedQueue.Cast<CurrentMessageInformation>()))
                {
                    result.Add(m);
                    handle.Set();
                }
            });

            sharedQueue.Enqueue(Message(1));

            Assert.IsTrue(handle.WaitOne(100));
            Assert.AreEqual(1, result.Count);

            sharedQueue.Enqueue(Message(3));
            Assert.IsFalse(handle.WaitOne(100));
            Assert.AreEqual(1, result.Count);

            sharedQueue.Enqueue(Message(2));
            Assert.IsTrue(handle.WaitOne(100));
            Assert.IsTrue(handle.WaitOne(100));
            Assert.AreEqual(3, result.Count);

            sharedQueue.Close();
        }

        [Test]
        public void Should_return_message_with_random_sequence_if_first_time_seen()
        {
            Assert.AreEqual(new uint[] { 100 }, Bodies(sut.Filter(Messages(100))));
        }

        [Test]
        public void Should_ignore_messages_with_lower_sequence_as_they_should_have_already_been_processed()
        {
            Assert.AreEqual(new uint[] { 4, 5, 6 }, Bodies(sut.Filter(Messages(4, 5, 6, 3))));            
        }

        [Test]
        public void Should_handle_sequences_using_publisher_endpoint_as_discriminator()
        {
            Assert.AreEqual(new uint[] { 1, 1 }, Bodies(sut.Filter(new[]{Message(1, "queue1"), Message(1, "queue2")})));            
        }

        private static uint[] Bodies(IEnumerable<CurrentMessageInformation> filter)
        {
            return filter.Select(f => f.Body).Cast<uint>().ToArray();
        }

        private static IEnumerable<CurrentMessageInformation> Messages(params uint[] sequenceAndBody)
        {
            return sequenceAndBody.Select(u => Message(u));
        }

        private static CurrentMessageInformation Message(uint sequenceAndBody, string endpoint = null)
        {
            return new CurrentMessageInformation
            {
                Headers = new Hashtable {{Headers.Sequence, BitConverter.GetBytes(sequenceAndBody)}},
                Body = sequenceAndBody,
                Endpoint = new RabbusEndpoint(endpoint ?? "someQueue")
            };
        }
    }
}