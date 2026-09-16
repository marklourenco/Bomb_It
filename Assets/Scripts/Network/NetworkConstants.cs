using UnityEngine;

namespace BombIt.Networking
{
    public static class NetworkConstants
    {
        // 1200 to avoid fragmentation (staying away from 1500)
        public const int maxPacketSize = 1200;
        // 7777 is not assigned to anything
        public const int defaultPort = 7777;
        public const int maxPlayers = 4;
        public const float peerTimeoutDuration = 5.0f;
        public const float heartbeatInterval = 1.0f;
        public const float connectRetryInterval = 0.5f;
        public const float connectGiveUpDuration = 5.0f;
    }
}