using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

namespace BombIt.Networking
{
    public class UdpSocket
    {
        private Socket socket;
        private Thread receiveThread;
        // volatile means that the value can be changed by a different thread at any time
        private volatile bool running;
        // "concurrent" is the thread-safe version of queue
        private readonly ConcurrentQueue<ReceivedPacket> receivedQueue = new ConcurrentQueue<ReceivedPacket>();

        public bool IsOpen
        {
            get { return socket != null; }
        }

        public int LocalPort
        {
            get { return socket == null ? 0 : ((IPEndPoint)socket.LocalEndPoint).Port; }
        }

        public void Open(int localPort)
        {
            if (IsOpen)
            {
                Debug.Log("UdpSocket: already open, ignoring Open() call");
                return;
            }
            // AddressFamily.InterNetwork = IPv4 (simpler and universally supported)
            // SocketType.Dgram = UDP (each send is a single independent packet)
            // ProtocolType.Udp = UDP protocol (udp does not guarantee delivery or order, so it never blocks waiting to resend or reorder packets
            // which is why I use it, as it fits the low-latency reqs a fast-paced game would need)
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(new IPEndPoint(IPAddress.Any, localPort));
            running = true;
            receiveThread = new Thread(ReceiveLoop);
            receiveThread.IsBackground = true;
            receiveThread.Start();
            Debug.Log($"UdpSocket: opened on local port {LocalPort}");
        }

        public void Send(IPEndPoint remote, byte[] data, int length)
        {
            if (!IsOpen)
            {
                return;
            }
            try
            {
                socket.SendTo(data, 0, length, SocketFlags.None, remote);
            }
            catch (SocketException exception)
            {
                Debug.Log($"UdpSocket: send failed, {exception.Message}");
            }
        }

        public void PollReceived(Action<IPEndPoint, byte[]> onPacket)
        {
            while (receivedQueue.TryDequeue(out var packet))
            {
                onPacket(packet.from, packet.data);
            }
        }

        public void Close()
        {
            running = false;
            if (socket != null)
            {
                socket.Close();
                socket = null;
            }
            if (receiveThread != null)
            {
                // wait to receive thread but move on after 200ms, just in case
                receiveThread.Join(200);
                receiveThread = null;
            }
        }

        private void ReceiveLoop()
        {
            var buffer = new byte[NetworkConstants.maxPacketSize];
            EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            while (running)
            {
                int bytesRead;
                try
                {
                    bytesRead = socket.ReceiveFrom(buffer, ref remoteEndPoint);
                }
                catch (SocketException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                var data = new byte[bytesRead];
                Array.Copy(buffer, data, bytesRead);
                receivedQueue.Enqueue(new ReceivedPacket
                {
                    from = (IPEndPoint)remoteEndPoint,
                    data = data
                });
            }
        }
    }
}