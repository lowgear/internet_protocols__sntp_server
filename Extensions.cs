using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SNTP_server
{
    public static class Extensions
    {
        public static void CopyAt(this byte[] bytes, IEnumerable<byte> source, int position)
        {
            foreach (var t in source.Zip(Enumerable.Range(position, int.MaxValue), Tuple.Create))
                bytes[t.Item2] = t.Item1;
        }

        public static byte Reversed(this byte v)
        {
            byte r = v;
            int s = 7;
            for (v >>= 1; v != 0; v >>= 1)
            {
                r <<= 1;
                r |= (byte)(v & 1);
                s--;
            }
            r <<= s;
            return r;
        }
    }
}