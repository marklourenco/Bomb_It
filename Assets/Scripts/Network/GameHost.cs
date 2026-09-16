using BombIt.Simulation;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace BombIt.Networking
{
    public class GameHost
    {
        private UdpSocket socket;
        private readonly Dictionary<IPEndPoint, NetworkPeer> peersByEndpoint = new Dictionary<IPEndPoint, NetworkPeer>();
        private readonly Dictionary<int, NetworkPeer> peersById = new Dictionary<int, NetworkPeer>();
        private readonly Dictionary<int, PlayerInputState> latestInputByPlayer = new Dictionary<int, PlayerInputState>();
        public bool IsRunning
        {
            get { return socket != null && socket.IsOpen; }
        }
        public IEnumerable<NetworkPeer> Peers
        {
            get { return peersById.Values; }
        }
        public bool TryGetLatestInput(int playerId, out PlayerInputState input)
        {
            return latestInputByPlayer.TryGetValue(playerId, out input);
        }
        public void SendToAll(byte[] payload, PacketChannel channel)
        {
            foreach (var peer in peersById.Values)
            {
                SendTo(peer, payload, channel);
            }
        }
        public void Start(int port)
        {
            socket = new UdpSocket();
            socket.Open(port);
        }
        public void Stop()
        {
            if (socket == null)
            {
                return;
            }
            foreach (var peer in peersById.Values)
            {
                SendTo(peer, Messages.Disconnect(), PacketChannel.Unreliable);
            }
            socket.Close();
            socket = null;
            peersByEndpoint.Clear();
            peersById.Clear();
        }
        public void Update(float deltaTime)
        {
            if (!IsRunning)
            {
                return;
            }
            socket.PollReceived(OnPacketReceived);
            var peersToRemove = new List<NetworkPeer>();
            foreach (var peer in peersById.Values)
            {
                peer.timeSinceLastReceive += deltaTime;
                if (peer.timeSinceLastReceive >= NetworkConstants.peerTimeoutDuration)
                {
                    Debug.Log($"GameHost: player {peer.playerId} timed out.");
                    peersToRemove.Add(peer);
                    continue;
                }
                var resends = peer.channel.CollectResends(deltaTime);
                foreach (var packet in resends)
                {
                    socket.Send(peer.endPoint, packet, packet.Length);
                }
                peer.timeSinceLastHeartbeatSent += deltaTime;
                if (peer.timeSinceLastHeartbeatSent >= NetworkConstants.heartbeatInterval)
                {
                    peer.timeSinceLastHeartbeatSent = 0.0f;
                    SendTo(peer, new byte[0], PacketChannel.Unreliable);
                }
            }
            foreach (var peer in peersToRemove)
            {
                RemovePeer(peer);
            }
        }
        private void OnPacketReceived(IPEndPoint from, byte[] rawData)
        {
            NetworkPeer peer;
            if (!peersByEndpoint.TryGetValue(from, out peer))
            {
                HandleNewConnection(from, rawData);
                return;
            }
            peer.timeSinceLastReceive = 0.0f;
            var message = peer.channel.Receive(rawData);
            if (!message.isNew || message.payload.Length == 0)
            {
                return;
            }
            HandleMessage(peer, message.payload);
        }
        private void HandleNewConnection(IPEndPoint from, byte[] rawData)
        {
            if (peersByEndpoint.Count >= NetworkConstants.maxPlayers - 1)
            {
                RejectConnection(from, rawData);
                return;
            }
            var peer = new NetworkPeer();
            peer.endPoint = from;
            peer.state = PeerState.Connecting;
            var message = peer.channel.Receive(rawData);
            if (message.payload.Length == 0)
            {
                return;
            }
            var reader = new PacketReader(message.payload);
            var messageType = (MessageType)reader.ReadByte();
            if (messageType != MessageType.ConnectRequest)
            {
                return;
            }
            int assignedId = AssignPlayerId();
            if (assignedId < 0)
            {
                return;
            }
            peer.playerId = assignedId;
            peer.state = PeerState.Connected;
            peersByEndpoint.Add(from, peer);
            peersById.Add(assignedId, peer);
            SendTo(peer, Messages.ConnectAccepted(assignedId), PacketChannel.Reliable);
            Debug.Log($"GameHost: player {assignedId} connected from {from}");
        }
        private void RejectConnection(IPEndPoint from, byte[] rawData)
        {
            var tempChannel = new ReliableChannel();
            var message = tempChannel.Receive(rawData);
            if (message.payload.Length == 0)
            {
                return;
            }
            var reader = new PacketReader(message.payload);
            var messageType = (MessageType)reader.ReadByte();
            if (messageType != MessageType.ConnectRequest)
            {
                return;
            }
            var packet = tempChannel.PrepareSend(Messages.ConnectRejected(), PacketChannel.Unreliable);
            socket.Send(from, packet, packet.Length);
        }
        private void HandleMessage(NetworkPeer peer, byte[] payload)
        {
            var reader = new PacketReader(payload);
            var messageType = (MessageType)reader.ReadByte();
            switch (messageType)
            {
                case MessageType.Disconnect:
                    Debug.Log($"GameHost: player {peer.playerId} disconnected.");
                    RemovePeer(peer);
                    break;
                case MessageType.PlayerInput:
                    latestInputByPlayer[peer.playerId] = Messages.ReadPlayerInput(reader);
                    break;
                default:
                    break;
            }
        }
        private void RemovePeer(NetworkPeer peer)
        {
            peersByEndpoint.Remove(peer.endPoint);
            peersById.Remove(peer.playerId);
            latestInputByPlayer.Remove(peer.playerId);
        }
        private int AssignPlayerId()
        {
            for (int id = 1; id < NetworkConstants.maxPlayers; ++id)
            {
                if (!peersById.ContainsKey(id))
                {
                    return id;
                }
            }
            return -1;
        }
        private void SendTo(NetworkPeer peer, byte[] payload, PacketChannel packetChannel)
        {
            var packet = peer.channel.PrepareSend(payload, packetChannel);
            socket.Send(peer.endPoint, packet, packet.Length);
        }
    }
}