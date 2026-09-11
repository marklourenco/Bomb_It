using UnityEngine;

namespace BombIt.Networking
{
    public struct ReceivedMessage
    {
        public bool isNew;
        public PacketHeader header;
        public byte[] payload;
    }
}