﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Rabbus.Utilities
{
    internal static class Extensions
    {
        internal static bool IsReply(this Type messageType)
        {
            return messageType.IsDefined(typeof (RabbusReplyAttribute), false);
        }

        internal static IEnumerable<T> ConcatIf<T>(this IEnumerable<T> input, bool condition, T toConcat)
        {
            if (!condition)
                return input;

            return input.Concat(Return(toConcat));
        }

        internal static IEnumerable<T> Return<T>(this T value)
        {
            yield return value;
        }
    }
}