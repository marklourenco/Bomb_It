using UnityEngine;

namespace BombIt.Networking
{
    public enum MessageType : byte
    {
        ConnectRequest,
        ConnectAccepted,
        ConnectRejected,
        Disconnect,
        PlayerInput,
        StateSnapshot
    }
}