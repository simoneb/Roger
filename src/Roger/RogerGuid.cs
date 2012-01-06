using System;

namespace Roger
{
    /// <summary>
    /// Wraps a plain <see cref="Guid"/> and provides implicit conversions to string and from plain Guid
    /// </summary>
    public struct RogerGuid
    {
        private readonly Guid plainGuid;
        public static RogerGuid Empty = new RogerGuid();

        public RogerGuid(string guid) : this(new Guid(guid))
        {
        }

        public RogerGuid(Guid plainGuid)
        {
            this.plainGuid = plainGuid;
        }

        public static implicit operator string(RogerGuid guid)
        {
            return guid.ToString();
        }

        public override string ToString()
        {
            return plainGuid.ToString();
        }

        public bool IsEmpty
        {
            get { return plainGuid == Guid.Empty; }
        }

        public static RogerGuid NewGuid()
        {
            return new RogerGuid(Guid.NewGuid());
        }
    }
}