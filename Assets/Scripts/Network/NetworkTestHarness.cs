using UnityEngine;

using BombIt.Networking;
using System.Net;
using System.Text;

public class NetworkTestHarness : MonoBehaviour
{
    [SerializeField] private int localPort = NetworkConstants.defaultPort;
    private UdpSocket socket;
    private string remoteAddressInput = "127.0.0.1";
    private string remotePortInput = "7777";
    private string messageInput = "hello";
    private string log = "";

    void Start()
    {
        socket = new UdpSocket();
    }

    void Update()
    {
        if (socket.IsOpen)
        {
            socket.PollReceived(OnPacketReceived);
        }
    }

    private void OnPacketReceived(IPEndPoint from, byte[] data)
    {
        var text = Encoding.UTF8.GetString(data);
        log = $"[{from}] {text}\n" + log;
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 400, 400));
        if (!socket.IsOpen)
        {
            GUILayout.Label("Local Port:");
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
            GUILayout.Label("Remote Address:");
            remoteAddressInput = GUILayout.TextField(remoteAddressInput);
            GUILayout.Label("Remote Port:");
            remotePortInput = GUILayout.TextField(remotePortInput);
            GUILayout.Label("Message:");
            messageInput = GUILayout.TextField(messageInput);
            if (GUILayout.Button("Send"))
            {
                var remote = new IPEndPoint(IPAddress.Parse(remoteAddressInput), int.Parse(remotePortInput));
                var bytes = Encoding.UTF8.GetBytes(messageInput);
                socket.Send(remote, bytes, bytes.Length);
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