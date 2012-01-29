using System.Reflection;

namespace Tests.Integration.Exploratory.Utils
{
    internal class MethodCallExpectation : Expectation
    {
        private readonly object target;
        private readonly MethodInfo method;

        public MethodCallExpectation(object target, MethodInfo method)
        {
            this.target = target;
            this.method = method;
        }

        public bool Equals(MethodCallExpectation other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.target, target) && Equals(other.method, method);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(MethodCallExpectation)) return false;
            return Equals((MethodCallExpectation)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (target.GetHashCode() * 397) ^ method.GetHashCode();
            }
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}", target.GetType().Name, method.Name);
        }
    }
}