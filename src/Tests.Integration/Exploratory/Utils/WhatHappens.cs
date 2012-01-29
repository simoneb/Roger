using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MbUnit.Framework;

namespace Tests.Integration.Exploratory.Utils
{
    class WhatHappens
    {
        private readonly List<Expectation> expectations = new List<Expectation>();
        private readonly List<Expectation> executed = new List<Expectation>();

        public THandler CaptureEvent<THandler>()
        {
            var expectation = new EventExpectation<THandler>();

            Arrange(expectation);

            var delegateType = typeof (THandler);
            var delegateParams = delegateType.GetMethod("Invoke").GetParameters().ToArray();

            return Expression.Lambda<THandler>(Expression.Call(Expression.Constant(this), 
                                                               typeof (WhatHappens).GetMethod("Act"), 
                                                               Expression.Constant(expectation)),
                                               delegateParams.Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToArray()).Compile();
        }

        private void Arrange(Expectation expectation)
        {
            expectations.Add(expectation);
        }

        public void Act(Expectation expectation)
        {
            executed.Add(expectation);
        }

        public void Verify()
        {
            Assert.AreElementsEqual(expectations, executed);
        }

        public void CaptureMethod<T>(Action<T> call)
        {
            Arrange(new MethodCallExpectation(call.Target, call.Method));
        }

        public void CaptureMethod<T1, T2>(Action<T1, T2> call)
        {
            Arrange(new MethodCallExpectation(call.Target, call.Method));
        }
    }
}