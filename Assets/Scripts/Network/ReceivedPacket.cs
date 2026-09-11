using System.Net;
using UnityEngine;

namespace BombIt.Networking
{
    public struct ReceivedPacket
    {
        // ip address
        public IPEndPoint from;
        // data is serialized into a byte array
        public byte[] data;
    }
}