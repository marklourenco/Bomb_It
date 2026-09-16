using BombIt.Simulation;
using UnityEngine;

namespace BombIt.Networking
{
    public static class Messages
    {
        public static byte[] ConnectRequest()
        {
            var writer = new PacketWriter();
            writer.WriteByte((byte)MessageType.ConnectRequest);
            return writer.ToArray();
        }
        public static byte[] ConnectAccepted(int assignedPlayerId)
        {
            var writer = new PacketWriter();
            writer.WriteByte((byte)MessageType.ConnectAccepted);
            writer.WriteInt(assignedPlayerId);
            return writer.ToArray();
        }
        public static byte[] ConnectRejected()
        {
            var writer = new PacketWriter();
            writer.WriteByte((byte)MessageType.ConnectRejected);
            return writer.ToArray();
        }
        public static byte[] Disconnect()
        {
            var writer = new PacketWriter();
            writer.WriteByte((byte)MessageType.Disconnect);
            return writer.ToArray();
        }
        public static byte[] PlayerInput(PlayerInputState input)
        {
            var writer = new PacketWriter();
            writer.WriteByte((byte)MessageType.PlayerInput);
            writer.WriteFloat(input.moveX);
            writer.WriteFloat(input.moveY);
            writer.WriteInt(input.bombRequestCount);
            return writer.ToArray();
        }
        public static PlayerInputState ReadPlayerInput(PacketReader reader)
        {
            var input = new PlayerInputState();
            input.moveX = reader.ReadFloat();
            input.moveY = reader.ReadFloat();
            input.bombRequestCount = reader.ReadInt();
            return input;
        }
    }
}