using UnityEngine;

namespace BombIt.Networking
{
    public static class SequenceUtility
    {
        public static bool IsMoreRecent(ushort a, ushort b)
        {
            return (a > b && (a - b) <= 32768) || (a < b && (b - a) > 32768);
        }

        public static int ForwardDistance(ushort newer, ushort older)
        {
            int diff = newer - older;
            if (diff < 0)
            {
                diff += 65536;
            }
            return diff;
        }
    }
}