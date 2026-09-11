using UnityEngine;
using System.Net;
using System.Text;
using BombIt.Networking;
public class NetworkTestHarness : MonoBehaviour
{
    [SerializeField] private int localPort = NetworkConstants.defaultPort;
    [Range(0.0f, 0.9f)]
    [SerializeField] private float simulatedLossChance = 0.3f;
    private UdpSocket socket;
    private readonly ReliableChannel channel = new ReliableChannel();
    private IPEndPoint remoteEndpoint;
    private string remoteAddressInput = "127.0.0.1";
    private string remotePortInput = "7777";
    private string messageInput = "hello";
    private bool sendReliable = true;
    private string log = "";
    private const float heartbeatInterval = 1.0f;
    private float heartbeatTimer;
    void Start()
    {
        socket = new UdpSocket();
    }
    void Update()
    {
        if (!socket.IsOpen)
        {
            return;
        }
        socket.PollReceived(OnPacketReceived);
        var resends = channel.CollectResends(Time.deltaTime);
        foreach (var packet in resends)
        {
            var header = PacketHeader.ReadFrom(packet, 0);
            log = $"resending seq={header.sequence}\n" + log;
            SendRaw(packet);
        }
        heartbeatTimer += Time.deltaTime;
        if (heartbeatTimer >= heartbeatInterval)
        {
            heartbeatTimer = 0.0f;
            SendHeartbeat();
        }
    }
    private void SendHeartbeat()
    {
        if (remoteEndpoint == null)
        {
            return;
        }
        var packet = channel.PrepareSend(new byte[0], PacketChannel.Unreliable);
        SendRaw(packet);
    }
    private void OnPacketReceived(IPEndPoint from, byte[] data)
    {
        remoteEndpoint = from;
        var message = channel.Receive(data);
        if (message.payload.Length == 0)
        {
            return;
        }
        var text = Encoding.UTF8.GetString(message.payload);
        var tag = message.isNew ? "new" : "duplicate/old";
        log = $"[{from}] seq={message.header.sequence} ack={message.header.ack} ({tag}) \"{text}\"\n" + log;
    }
    private void SendRaw(byte[] packet)
    {
        if (remoteEndpoint == null)
        {
            return;
        }
        if (Random.value < simulatedLossChance)
        {
            log = "simulated loss, packet dropped before sending\n" + log;
            return;
        }
        socket.Send(remoteEndpoint, packet, packet.Length);
    }
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 400, 500));
        if (!socket.IsOpen)
        {
            GUILayout.Label("Local port:");
            var portText = GUILayout.TextField(localPort.ToString());
            if (int.TryParse(portText, out var parsedPort))
            {
                localPort = parsedPort;
            }
            if (GUILayout.Button("Open Socket"))
            {
                socket.Open(localPort);
            }
        }
        else
        {
            GUILayout.Label($"Socket open on port {socket.LocalPort}");
            GUILayout.Label("Remote address:");
            remoteAddressInput = GUILayout.TextField(remoteAddressInput);
            GUILayout.Label("Remote port:");
            remotePortInput = GUILayout.TextField(remotePortInput);
            GUILayout.Label("Message:");
            messageInput = GUILayout.TextField(messageInput);
            sendReliable = GUILayout.Toggle(sendReliable, "Send reliably");
            GUILayout.Label($"Simulated loss: {(simulatedLossChance * 100.0f):F0}%");
            simulatedLossChance = GUILayout.HorizontalSlider(simulatedLossChance, 0.0f, 0.9f);
            if (GUILayout.Button("Send"))
            {
                remoteEndpoint = new IPEndPoint(IPAddress.Parse(remoteAddressInput), int.Parse(remotePortInput));
                var bytes = Encoding.UTF8.GetBytes(messageInput);
                var packetChannel = sendReliable ? PacketChannel.Reliable : PacketChannel.Unreliable;
                var packet = channel.PrepareSend(bytes, packetChannel);
                SendRaw(packet);
            }
            GUILayout.Label($"Pending reliable messages (not yet acked): {channel.PendingCount}");
            if (GUILayout.Button("Clear Log"))
            {
                log = "";
            }
        }
        GUILayout.Label(log);
        GUILayout.EndArea();
    }
    void OnDestroy()
    {
        if (socket != null)
        {
            socket.Close();
        }
    }
}
