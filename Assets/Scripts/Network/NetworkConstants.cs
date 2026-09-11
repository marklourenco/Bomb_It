using UnityEngine;

namespace BombIt.Networking
{
    public static class NetworkConstants
    {
        // 1200 to avoid fragmentation (staying away from 1500)
        public const int maxPacketSize = 1200;
        // 7777 is not assigned to anything
        public const int defaultPort = 7777;
    }
}