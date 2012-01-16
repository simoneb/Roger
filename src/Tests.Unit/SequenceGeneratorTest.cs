using System;
using System.Collections.Generic;
using System.Linq;
using MbUnit.Framework;
using Roger.Internal.Impl;
using Tests.Unit.SupportClasses;

namespace Tests.Unit
{
    [TestFixture]
    public class SequenceGeneratorTest
    {
        private ThreadSafeIncrementalSequenceGenerator sut;

        [SetUp]
        public void Setup()
        {
            sut = new ThreadSafeIncrementalSequenceGenerator();
        }

        [Test]
        public void Should_generate_sequential_numbers_for_same_message_type()
        {
            var expectedSequence = new[] {1u, 2u, 3u};
        
            var result = expectedSequence.Select(i => sut.Next(typeof (MyMessage)));

            Assert.AreElementsEqual(expectedSequence, result);
        }

        [Test]
        public void Should_discriminate_between_concrete_message_types_belonging_to_different_hierarchies()
        {
            var result = SequenceFor(typeof (MyMessage), typeof (MyOtherMessage));

            Assert.AreElementsEqual(new[]{1u, 1u}, result);
        }

        [Test]
        public void Should_treat_types_belonging_to_same_hiearchy_as_same_message()
        {
            var result = SequenceFor(typeof(MyMessage), typeof(MyDerivedMessage));

            Assert.AreElementsEqual(new[] { 1u, 2u }, result);
        }

        private IEnumerable<uint> SequenceFor(params Type[] types)
        {
            return types.Select(type => sut.Next(type));
        }
    }
}