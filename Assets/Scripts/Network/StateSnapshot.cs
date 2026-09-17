using BombIt.Simulation;
using System.Collections.Generic;
using UnityEngine;

namespace BombIt.Networking
{
    public class SnapshotPlayerData
    {
        public int id;
        public float posX;
        public float posY;
        public bool alive;
        public int rangeLevel;
        public int bombCountLevel;
        public int speedLevel;
    }
    public class SnapshotBombData
    {
        public int id;
        public int ownerId;
        public int cellX;
        public int cellY;
        public int range;
        public float fuseRemaining;
    }
    public class SnapshotExplosionData
    {
        public int cellX;
        public int cellY;
        public float remaining;
    }
    public class SnapshotPickupData
    {
        public int id;
        public int cellX;
        public int cellY;
        public UpgradeType type;
    }
    public class SnapshotData
    {
        public CellContent[] gridCells;
        public bool gameStarted;
        public bool gameOver;
        public int winnerId;
        public readonly List<SnapshotPlayerData> players = new List<SnapshotPlayerData>();
        public readonly List<SnapshotBombData> bombs = new List<SnapshotBombData>();
        public readonly List<SnapshotExplosionData> explosions = new List<SnapshotExplosionData>();
        public readonly List<SnapshotPickupData> pickups = new List<SnapshotPickupData>();
    }
    public static class StateSnapshot
    {
        public static byte[] Write(GridMap grid, IReadOnlyList<PlayerState> players, BombSimulation bombSimulation, bool gameStarted, bool gameOver, int winnerId)
        {
            var writer = new PacketWriter();
            writer.WriteByte((byte)MessageType.StateSnapshot);
            writer.WriteByte((byte)(gameStarted ? 1 : 0));
            writer.WriteByte((byte)(gameOver ? 1 : 0));
            writer.WriteByte((byte)(winnerId + 1));
            for (int y = 0; y < GridMap.height; ++y)
            {
                for (int x = 0; x < GridMap.width; ++x)
                {
                    writer.WriteByte((byte)grid.GetCell(x, y));
                }
            }
            writer.WriteByte((byte)players.Count);
            foreach (var player in players)
            {
                writer.WriteByte((byte)player.id);
                writer.WriteFloat(player.position.x);
                writer.WriteFloat(player.position.y);
                writer.WriteByte((byte)(player.alive ? 1 : 0));
                writer.WriteByte((byte)player.rangeLevel);
                writer.WriteByte((byte)player.bombCountLevel);
                writer.WriteByte((byte)player.speedLevel);
            }
            writer.WriteByte((byte)bombSimulation.Bombs.Count);
            foreach (var bomb in bombSimulation.Bombs)
            {
                writer.WriteInt(bomb.id);
                writer.WriteByte((byte)bomb.ownerId);
                writer.WriteByte((byte)bomb.cell.x);
                writer.WriteByte((byte)bomb.cell.y);
                writer.WriteByte((byte)bomb.range);
                writer.WriteFloat(bomb.fuseRemaining);
            }
            writer.WriteByte((byte)bombSimulation.Explosions.Count);
            foreach (var explosion in bombSimulation.Explosions)
            {
                writer.WriteByte((byte)explosion.cell.x);
                writer.WriteByte((byte)explosion.cell.y);
                writer.WriteFloat(explosion.remaining);
            }
            writer.WriteByte((byte)bombSimulation.Pickups.Count);
            foreach (var pickup in bombSimulation.Pickups)
            {
                writer.WriteInt(pickup.id);
                writer.WriteByte((byte)pickup.cell.x);
                writer.WriteByte((byte)pickup.cell.y);
                writer.WriteByte((byte)pickup.type);
            }
            return writer.ToArray();
        }
        public static SnapshotData Read(PacketReader reader)
        {
            var data = new SnapshotData();
            data.gameStarted = reader.ReadByte() != 0;
            data.gameOver = reader.ReadByte() != 0;
            data.winnerId = reader.ReadByte() - 1;
            data.gridCells = new CellContent[GridMap.width * GridMap.height];
            for (int y = 0; y < GridMap.height; ++y)
            {
                for (int x = 0; x < GridMap.width; ++x)
                {
                    data.gridCells[y * GridMap.width + x] = (CellContent)reader.ReadByte();
                }
            }
            int playerCount = reader.ReadByte();
            for (int i = 0; i < playerCount; ++i)
            {
                var player = new SnapshotPlayerData();
                player.id = reader.ReadByte();
                player.posX = reader.ReadFloat();
                player.posY = reader.ReadFloat();
                player.alive = reader.ReadByte() != 0;
                player.rangeLevel = reader.ReadByte();
                player.bombCountLevel = reader.ReadByte();
                player.speedLevel = reader.ReadByte();
                data.players.Add(player);
            }
            int bombCount = reader.ReadByte();
            for (int i = 0; i < bombCount; ++i)
            {
                var bomb = new SnapshotBombData();
                bomb.id = reader.ReadInt();
                bomb.ownerId = reader.ReadByte();
                bomb.cellX = reader.ReadByte();
                bomb.cellY = reader.ReadByte();
                bomb.range = reader.ReadByte();
                bomb.fuseRemaining = reader.ReadFloat();
                data.bombs.Add(bomb);
            }
            int explosionCount = reader.ReadByte();
            for (int i = 0; i < explosionCount; ++i)
            {
                var explosion = new SnapshotExplosionData();
                explosion.cellX = reader.ReadByte();
                explosion.cellY = reader.ReadByte();
                explosion.remaining = reader.ReadFloat();
                data.explosions.Add(explosion);
            }
            int pickupCount = reader.ReadByte();
            for (int i = 0; i < pickupCount; ++i)
            {
                var pickup = new SnapshotPickupData();
                pickup.id = reader.ReadInt();
                pickup.cellX = reader.ReadByte();
                pickup.cellY = reader.ReadByte();
                pickup.type = (UpgradeType)reader.ReadByte();
                data.pickups.Add(pickup);
            }
            return data;
        }
    }
}