using System.Net;
using UnityEngine;

namespace BombIt.Networking
{
    public class NetworkPeer
    {
        public IPEndPoint endPoint;
        public int playerId;
        public PeerState state;
        public readonly ReliableChannel channel = new ReliableChannel();
        public float timeSinceLastReceive;
        public float timeSinceLastHeartbeatSent;
    }
}