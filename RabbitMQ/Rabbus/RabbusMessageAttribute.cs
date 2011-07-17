using System;

namespace Rabbus
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class RabbusMessageAttribute : Attribute
    {
        public RabbusMessageAttribute(string exchange)
        {
            Exchange = exchange;
        }

        public string Exchange { get; private set; }
    }
}