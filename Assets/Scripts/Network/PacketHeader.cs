using System;
using UnityEngine;

namespace BombIt.Networking
{
    public struct PacketHeader
    {
        public const int size = 9;
        public ushort sequence;
        public ushort ack;
        public uint ackBits;
        public PacketChannel channel;

        public void WriteTo(byte[] buffer, int offset)
        {
            BitConverter.GetBytes(sequence).CopyTo(buffer, offset);
            BitConverter.GetBytes(ack).CopyTo(buffer, offset + 2);
            BitConverter.GetBytes(ackBits).CopyTo(buffer, offset + 4);
            buffer[offset + 8] = (byte)channel;
        }

        public static PacketHeader ReadFrom(byte[] buffer, int offset)
        {
            return new PacketHeader
            {
                sequence = BitConverter.ToUInt16(buffer, offset),
                ack = BitConverter.ToUInt16(buffer, offset + 2),
                ackBits = BitConverter.ToUInt32(buffer, offset + 4),
                channel = (PacketChannel)buffer[offset + 8]
            };
        }
    }
}
