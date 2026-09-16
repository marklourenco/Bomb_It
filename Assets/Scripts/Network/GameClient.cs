using BombIt.Simulation;
using System.Net;
using UnityEngine;

namespace BombIt.Networking
{
    public class GameClient
    {
        private UdpSocket socket;
        private IPEndPoint hostEndpoint;
        private readonly ReliableChannel channel = new ReliableChannel();
        private PeerState state;
        private int localPlayerId;
        private bool wasRejected;
        private SnapshotData latestSnapshot;
        private float timeSinceLastReceive;
        private float timeSinceLastConnectAttempt;
        private float timeSinceConnectingStarted;
        private float timeSinceLastHeartbeatSent;
        public PeerState State
        {
            get { return state; }
        }
        public int LocalPlayerId
        {
            get { return localPlayerId; }
        }
        public bool WasRejected
        {
            get { return wasRejected; }
        }
        public SnapshotData LatestSnapshot
        {
            get { return latestSnapshot; }
        }
        public void SendInput(PlayerInputState input)
        {
            if (state != PeerState.Connected)
            {
                return;
            }
            SendToHost(Messages.PlayerInput(input), PacketChannel.Unreliable);
        }
        public void Connect(string hostAddress, int hostPort, int localPort)
        {
            socket = new UdpSocket();
            socket.Open(localPort);
            hostEndpoint = new IPEndPoint(IPAddress.Parse(hostAddress), hostPort);
            state = PeerState.Connecting;
            wasRejected = false;
            timeSinceConnectingStarted = 0.0f;
            timeSinceLastConnectAttempt = NetworkConstants.connectRetryInterval;
        }
        public void Disconnect()
        {
            if (state == PeerState.Connected)
            {
                SendToHost(Messages.Disconnect(), PacketChannel.Unreliable);
            }
            state = PeerState.Disconnected;
            if (socket != null)
            {
                socket.Close();
                socket = null;
            }
        }
        public void Update(float deltaTime)
        {
            if (socket == null || !socket.IsOpen)
            {
                return;
            }
            socket.PollReceived(OnPacketReceived);
            if (state == PeerState.Connecting)
            {
                timeSinceConnectingStarted += deltaTime;
                if (timeSinceConnectingStarted >= NetworkConstants.connectGiveUpDuration)
                {
                    Debug.Log("GameClient: giving up, no response from host.");
                    state = PeerState.Disconnected;
                    return;
                }
                timeSinceLastConnectAttempt += deltaTime;
                if (timeSinceLastConnectAttempt >= NetworkConstants.connectRetryInterval)
                {
                    timeSinceLastConnectAttempt = 0.0f;
                    SendRequest();
                }
                return;
            }
            if (state == PeerState.Connected)
            {
                timeSinceLastReceive += deltaTime;
                if (timeSinceLastReceive >= NetworkConstants.peerTimeoutDuration)
                {
                    Debug.Log("GameClient: lost connection to host (timeout).");
                    state = PeerState.Disconnected;
                    return;
                }
                var resends = channel.CollectResends(deltaTime);
                foreach (var packet in resends)
                {
                    socket.Send(hostEndpoint, packet, packet.Length);
                }
                timeSinceLastHeartbeatSent += deltaTime;
                if (timeSinceLastHeartbeatSent >= NetworkConstants.heartbeatInterval)
                {
                    timeSinceLastHeartbeatSent = 0.0f;
                    SendToHost(new byte[0], PacketChannel.Unreliable);
                }
            }
        }
        private void SendRequest()
        {
            var packet = channel.PrepareSend(Messages.ConnectRequest(), PacketChannel.Unreliable);
            socket.Send(hostEndpoint, packet, packet.Length);
        }
        private void SendToHost(byte[] payload, PacketChannel packetChannel)
        {
            var packet = channel.PrepareSend(payload, packetChannel);
            socket.Send(hostEndpoint, packet, packet.Length);
        }
        private void OnPacketReceived(IPEndPoint from, byte[] rawData)
        {
            timeSinceLastReceive = 0.0f;
            var message = channel.Receive(rawData);
            if (!message.isNew || message.payload.Length == 0)
            {
                return;
            }
            var reader = new PacketReader(message.payload);
            var messageType = (MessageType)reader.ReadByte();
            switch (messageType)
            {
                case MessageType.ConnectAccepted:
                    localPlayerId = reader.ReadInt();
                    state = PeerState.Connected;
                    Debug.Log($"GameClient: connected, assigned player id {localPlayerId}");
                    SendToHost(new byte[0], PacketChannel.Unreliable);
                    break;
                case MessageType.ConnectRejected:
                    wasRejected = true;
                    state = PeerState.Disconnected;
                    Debug.Log("GameClient: connection rejected by host (server full?).");
                    break;
                case MessageType.Disconnect:
                    state = PeerState.Disconnected;
                    Debug.Log("GameClient: host disconnected us.");
                    break;
                case MessageType.StateSnapshot:
                    latestSnapshot = StateSnapshot.Read(reader);
                    break;
                default:
                    break;
            }
        }
    }
}