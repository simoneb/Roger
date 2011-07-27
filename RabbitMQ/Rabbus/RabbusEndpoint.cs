namespace Rabbus
{
    public struct RabbusEndpoint
    {
        public RabbusEndpoint(string queue)
        {
            Queue = queue;
        }

        public readonly string Queue;

        public bool IsEmpty()
        {
            return string.IsNullOrWhiteSpace(Queue);
        }

        public static implicit operator string(RabbusEndpoint endpoint)
        {
            return endpoint.Queue;
        }
    }
}