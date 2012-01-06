namespace Roger
{
    /// <summary>
    /// Represents an addressable endpoint which can receive messages
    /// </summary>
    public struct RogerEndpoint
    {
        public RogerEndpoint(string queue)
        {
            Queue = queue;
        }

        public readonly string Queue;

        public bool IsEmpty
        {
            get { return string.IsNullOrWhiteSpace(Queue); }
        }

        public static implicit operator string(RogerEndpoint endpoint)
        {
            return endpoint.Queue;
        }

        public override string ToString()
        {
            return this;
        }
    }
}