using BombIt.Networking;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class ConnectionTestHarness : MonoBehaviour
{
    private enum Role
    {
        None,
        Host,
        Client
    }
    private Role role = Role.None;
    private GameHost host;
    private GameClient client;
    private int hostPort = NetworkConstants.defaultPort;
    private string clientAddressInput = "127.0.0.1";
    private string clientPortInput = "6767";
    void Update()
    {
        if (role == Role.Host)
        {
            host.Update(Time.deltaTime);
        }
        else if (role == Role.Client)
        {
            client.Update(Time.deltaTime);
        }
    }
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 420, 500));
        if (role == Role.None)
        {
            GUILayout.Label("Host port:");
            var portText = GUILayout.TextField(hostPort.ToString());
            int parsedPort;
            if (int.TryParse(portText, out parsedPort))
            {
                hostPort = parsedPort;
            }
            if (GUILayout.Button("Start Host"))
            {
                host = new GameHost();
                host.Start(hostPort);
                role = Role.Host;
            }
            GUILayout.Space(20);
            GUILayout.Label("Host address to connect to:");
            clientAddressInput = GUILayout.TextField(clientAddressInput);
            GUILayout.Label("Host port to connect to:");
            clientPortInput = GUILayout.TextField(clientPortInput);
            if (GUILayout.Button("Connect as Client"))
            {
                client = new GameClient();
                client.Connect(clientAddressInput, int.Parse(clientPortInput), 0);
                role = Role.Client;
            }
        }
        else if (role == Role.Host)
        {
            GUILayout.Label($"Hosting on port {hostPort}. You are player 0.");
            GUILayout.Label($"Your local network address: {GetLocalIPAddress()}:{hostPort}");
            GUILayout.Label("On the same wifi/LAN, the other player can connect to that address directly.");
            GUILayout.Label("To play over the internet, forward this UDP port to this machine in your router, then share your public IP (search \"what is my ip\") instead of the local one above.");
            foreach (var peer in host.Peers)
            {
                GUILayout.Label($"Player {peer.playerId}  -  {peer.state}  -  {peer.endPoint}");
            }
            if (GUILayout.Button("Stop Host"))
            {
                host.Stop();
                host = null;
                role = Role.None;
            }
        }
        else if (role == Role.Client)
        {
            GUILayout.Label($"State: {client.State}");
            if (client.State == PeerState.Connected)
            {
                GUILayout.Label($"Connected as player {client.LocalPlayerId}");
            }
            if (client.WasRejected)
            {
                GUILayout.Label("Connection was rejected (host may be full).");
            }
            if (GUILayout.Button("Disconnect"))
            {
                client.Disconnect();
                client = null;
                role = Role.None;
            }
        }
        GUILayout.EndArea();
    }
    private string GetLocalIPAddress()
    {
        using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
        {
            socket.Connect("8.8.8.8", 65530);
            var endPoint = (IPEndPoint)socket.LocalEndPoint;
            return endPoint.Address.ToString();
        }
    }
    void OnDestroy()
    {
        if (host != null)
        {
            host.Stop();
        }
        if (client != null)
        {
            client.Disconnect();
        }
    }
}