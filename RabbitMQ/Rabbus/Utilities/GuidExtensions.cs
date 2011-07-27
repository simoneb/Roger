using System;

namespace Rabbus.Utilities
{
    internal static class GuidExtensions
    {
         internal static bool IsEmpty(this Guid guid)
         {
             return guid == Guid.Empty;
         }
    }
}