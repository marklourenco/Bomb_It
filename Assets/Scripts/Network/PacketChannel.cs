using UnityEngine;

namespace BombIt.Networking
{
    public enum PacketChannel : byte
    {
        Unreliable = 0,
        Reliable = 1
    }
}