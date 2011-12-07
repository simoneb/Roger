using System;

namespace Rabbus
{
    /// <summary>
    /// Wraps a plain <see cref="Guid"/> and provides implicit conversions to string and from plain Guid
    /// </summary>
    public struct RabbusGuid
    {
        private readonly Guid plainGuid;
        public static RabbusGuid Empty = new RabbusGuid();

        public RabbusGuid(string guid) : this(new Guid(guid))
        {
        }

        public RabbusGuid(Guid plainGuid)
        {
            this.plainGuid = plainGuid;
        }

        public static implicit operator string(RabbusGuid guid)
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

        public static RabbusGuid NewGuid()
        {
            return new RabbusGuid(Guid.NewGuid());
        }
    }
}