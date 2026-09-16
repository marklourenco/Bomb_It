using System;
using System.Collections.Generic;
using UnityEngine;

namespace BombIt.Networking
{
    public class PacketWriter
    {
        private readonly List<byte> bytes = new List<byte>();
        public void WriteByte(byte value)
        {
            bytes.Add(value);
        }
        public void WriteUShort(ushort value)
        {
            bytes.AddRange(BitConverter.GetBytes(value));
        }
        public void WriteInt(int value)
        {
            bytes.AddRange(BitConverter.GetBytes(value));
        }
        public void WriteFloat(float value)
        {
            bytes.AddRange(BitConverter.GetBytes(value));
        }
        public byte[] ToArray()
        {
            return bytes.ToArray();
        }
    }
}
