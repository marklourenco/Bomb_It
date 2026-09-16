using System;
using UnityEngine;

namespace BombIt.Networking
{
    public class PacketReader
    {
        private readonly byte[] buffer;
        private int position;
        public PacketReader(byte[] data)
        {
            buffer = data;
            position = 0;
        }
        public byte ReadByte()
        {
            var value = buffer[position];
            position += 1;
            return value;
        }
        public ushort ReadUShort()
        {
            var value = BitConverter.ToUInt16(buffer, position);
            position += 2;
            return value;
        }
        public int ReadInt()
        {
            var value = BitConverter.ToInt32(buffer, position);
            position += 4;
            return value;
        }
        public float ReadFloat()
        {
            var value = BitConverter.ToSingle(buffer, position);
            position += 4;
            return value;
        }
    }
}