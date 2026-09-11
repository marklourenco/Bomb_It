using System;
using System.Collections.Generic;
namespace BombIt.Networking
{
    public class ReliableChannel
    {
        private const float resendInterval = 0.25f;
        private ushort localSequenceCounter;
        private bool hasReceivedAny;
        private ushort remoteSequence;
        private uint receivedBits;
        private readonly List<PendingMessage> pendingReliable = new List<PendingMessage>();
        public int PendingCount
        {
            get { return pendingReliable.Count; }
        }
        private class PendingMessage
        {
            public ushort sequence;
            public byte[] packetBytes;
            public float timeSinceLastSend;
        }
        public byte[] PrepareSend(byte[] payload, PacketChannel channel)
        {
            var header = new PacketHeader
            {
                sequence = localSequenceCounter,
                ack = remoteSequence,
                ackBits = receivedBits,
                channel = channel
            };
            var packet = new byte[PacketHeader.size + payload.Length];
            header.WriteTo(packet, 0);
            Array.Copy(payload, 0, packet, PacketHeader.size, payload.Length);
            if (channel == PacketChannel.Reliable)
            {
                pendingReliable.Add(new PendingMessage
                {
                    sequence = localSequenceCounter,
                    packetBytes = packet,
                    timeSinceLastSend = 0.0f
                });
            }
            ++localSequenceCounter;
            return packet;
        }
        public ReceivedMessage Receive(byte[] rawPacket)
        {
            var header = PacketHeader.ReadFrom(rawPacket, 0);
            var payload = new byte[rawPacket.Length - PacketHeader.size];
            Array.Copy(rawPacket, PacketHeader.size, payload, 0, payload.Length);
            bool isNew = UpdateRemoteSequence(header.sequence);
            AcknowledgePending(header.ack, header.ackBits);
            return new ReceivedMessage
            {
                isNew = isNew,
                header = header,
                payload = payload
            };
        }
        public List<byte[]> CollectResends(float deltaTime)
        {
            var resends = new List<byte[]>();
            foreach (var pending in pendingReliable)
            {
                pending.timeSinceLastSend += deltaTime;
                if (pending.timeSinceLastSend >= resendInterval)
                {
                    pending.timeSinceLastSend = 0.0f;
                    resends.Add(pending.packetBytes);
                }
            }
            return resends;
        }
        private bool UpdateRemoteSequence(ushort sequence)
        {
            if (!hasReceivedAny)
            {
                hasReceivedAny = true;
                remoteSequence = sequence;
                receivedBits = 0;
                return true;
            }
            if (SequenceUtility.IsMoreRecent(sequence, remoteSequence))
            {
                int shift = SequenceUtility.ForwardDistance(sequence, remoteSequence);
                receivedBits = ShiftLeftSafe(receivedBits, shift);
                if (shift <= 32)
                {
                    receivedBits |= 1u << (shift - 1);
                }
                remoteSequence = sequence;
                return true;
            }
            int distanceBehind = SequenceUtility.ForwardDistance(remoteSequence, sequence);
            if (distanceBehind == 0 || distanceBehind > 32)
            {
                return false;
            }
            bool alreadyMarked = (receivedBits & (1u << (distanceBehind - 1))) != 0;
            receivedBits |= 1u << (distanceBehind - 1);
            return !alreadyMarked;
        }
        private static uint ShiftLeftSafe(uint value, int shift)
        {
            return shift >= 32 ? 0u : value << shift;
        }
        private void AcknowledgePending(ushort ack, uint ackBits)
        {
            for (int i = pendingReliable.Count - 1; i >= 0; --i)
            {
                if (IsAcked(pendingReliable[i].sequence, ack, ackBits))
                {
                    pendingReliable.RemoveAt(i);
                }
            }
        }
        private static bool IsAcked(ushort sequence, ushort ack, uint ackBits)
        {
            if (sequence == ack)
            {
                return true;
            }
            if (!SequenceUtility.IsMoreRecent(ack, sequence))
            {
                return false;
            }
            int distance = SequenceUtility.ForwardDistance(ack, sequence);
            return distance <= 32 && (ackBits & (1u << (distance - 1))) != 0;
        }
    }
}
