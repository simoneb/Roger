namespace Tests.Integration.Exploratory.Utils
{
    class EventExpectation<THandler> : Expectation
    {
        public override string ToString()
        {
            return string.Format("Event: {0}", typeof (THandler).Name);
        }
    }
}