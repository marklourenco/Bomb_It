using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace BombIt.Simulation
{
    public class BombSimulation
    {
        private readonly GridMap grid;
        private readonly List<BombState> bombs = new List<BombState>();
        private readonly List<ExplosionCellState> explosions = new List<ExplosionCellState>();
        private readonly List<UpgradePickupState> pickups = new List<UpgradePickupState>();
        private readonly System.Random rng = new System.Random();
        private int nextBombId;
        private int nextPickupId;
        public BombSimulation(GridMap gridMap)
        {
            grid = gridMap;
        }
        public IReadOnlyList<BombState> Bombs
        {
            get { return bombs; }
        }
        public IReadOnlyList<ExplosionCellState> Explosions
        {
            get { return explosions; }
        }
        public IReadOnlyList<UpgradePickupState> Pickups
        {
            get { return pickups; }
        }
        public bool TryPlaceBomb(int ownerId, Vector2Int cell, int range, int maxBombsForOwner)
        {
            if (!grid.InBounds(cell.x, cell.y) || grid.IsSolid(cell.x, cell.y))
            {
                return false;
            }
            foreach (var existing in bombs)
            {
                if (existing.cell == cell)
                {
                    return false;
                }
            }
            int ownerBombCount = 0;
            foreach (var existing in bombs)
            {
                if (existing.ownerId == ownerId)
                {
                    ++ownerBombCount;
                }
            }
            if (ownerBombCount >= maxBombsForOwner)
            {
                return false;
            }
            var bomb = new BombState
            {
                id = nextBombId,
                ownerId = ownerId,
                cell = cell,
                range = range,
                fuseRemaining = BombConstants.fuseDuration,
                ownerStillOnCell = true
            };
            ++nextBombId;
            bombs.Add(bomb);
            return true;
        }
        public bool IsCellBlockedForPlayer(Vector2Int cell, int playerId)
        {
            foreach (var bomb in bombs)
            {
                if (bomb.cell != cell)
                {
                    continue;
                }
                if (bomb.ownerId == playerId && bomb.ownerStillOnCell)
                {
                    return false;
                }
                return true;
            }
            return false;
        }
        public bool Tick(float deltaTime, IReadOnlyList<PlayerState> players)
        {
            bool gridChanged = false;
            foreach (var bomb in bombs)
            {
                if (!bomb.ownerStillOnCell)
                {
                    continue;
                }
                var owner = FindPlayer(players, bomb.ownerId);
                if (owner == null)
                {
                    continue;
                }
                if (!PositionOverlapsCell(owner.position, owner.collisionRadius, bomb.cell))
                {
                    bomb.ownerStillOnCell = false;
                }
            }
            var toExplode = new Queue<BombState>();
            var queuedSet = new HashSet<BombState>();
            foreach (var bomb in bombs)
            {
                bomb.fuseRemaining -= deltaTime;
                if (bomb.fuseRemaining <= 0.0f && !queuedSet.Contains(bomb))
                {
                    toExplode.Enqueue(bomb);
                    queuedSet.Add(bomb);
                }
            }
            while (toExplode.Count > 0)
            {
                var bomb = toExplode.Dequeue();
                if (!bombs.Contains(bomb))
                {
                    continue;
                }
                var affectedCells = ComputeExplosionCells(bomb.cell, bomb.range, ref gridChanged);
                foreach (var cell in affectedCells)
                {
                    AddOrRefreshExplosion(cell);
                    foreach (var other in bombs)
                    {
                        if (other != bomb && other.cell == cell && !queuedSet.Contains(other))
                        {
                            queuedSet.Add(other);
                            toExplode.Enqueue(other);
                        }
                    }
                }
                bombs.Remove(bomb);
            }
            for (int i = explosions.Count - 1; i >= 0; --i)
            {
                explosions[i].remaining -= deltaTime;
                if (explosions[i].remaining <= 0.0f)
                {
                    explosions.RemoveAt(i);
                }
            }
            foreach (var player in players)
            {
                if (!player.alive)
                {
                    continue;
                }
                var playerCell = GridMap.WorldToGrid(player.position);
                foreach (var explosion in explosions)
                {
                    if (explosion.cell == playerCell)
                    {
                        player.alive = false;
                        break;
                    }
                }
            }
            CollectPickups(players);
            return gridChanged;
        }
        private void CollectPickups(IReadOnlyList<PlayerState> players)
        {
            for (int i = pickups.Count - 1; i >= 0; --i)
            {
                var pickup = pickups[i];
                foreach (var player in players)
                {
                    if (!player.alive)
                    {
                        continue;
                    }
                    if (GridMap.WorldToGrid(player.position) != pickup.cell)
                    {
                        continue;
                    }
                    ApplyUpgrade(player, pickup.type);
                    pickups.RemoveAt(i);
                    break;
                }
            }
        }
        private static void ApplyUpgrade(PlayerState player, UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.Range:
                    player.rangeLevel = Mathf.Min(player.rangeLevel + 1, PlayerConstants.maxUpgradeLevel);
                    break;
                case UpgradeType.BombCount:
                    player.bombCountLevel = Mathf.Min(player.bombCountLevel + 1, PlayerConstants.maxUpgradeLevel);
                    break;
                case UpgradeType.Speed:
                    player.speedLevel = Mathf.Min(player.speedLevel + 1, PlayerConstants.maxUpgradeLevel);
                    break;
            }
        }
        private void TryDropUpgrade(Vector2Int cell)
        {
            if (rng.NextDouble() >= BombConstants.upgradeDropChance)
            {
                return;
            }
            var values = System.Enum.GetValues(typeof(UpgradeType));
            var type = (UpgradeType)values.GetValue(rng.Next(values.Length));
            pickups.Add(new UpgradePickupState { id = nextPickupId, cell = cell, type = type });
            ++nextPickupId;
        }
        private static bool PositionOverlapsCell(Vector2 position, float radius, Vector2Int cell)
        {
            float cellMinX = cell.x * GridMap.cellSize;
            float cellMaxX = cellMinX + GridMap.cellSize;
            float cellMinY = cell.y * GridMap.cellSize;
            float cellMaxY = cellMinY + GridMap.cellSize;
            bool overlapsX = position.x + radius > cellMinX && position.x - radius < cellMaxX;
            bool overlapsY = position.y + radius > cellMinY && position.y - radius < cellMaxY;
            return overlapsX && overlapsY;
        }
        private static PlayerState FindPlayer(IReadOnlyList<PlayerState> players, int id)
        {
            foreach (var player in players)
            {
                if (player.id == id)
                {
                    return player;
                }
            }
            return null;
        }
        private void AddOrRefreshExplosion(Vector2Int cell)
        {
            foreach (var explosion in explosions)
            {
                if (explosion.cell == cell)
                {
                    explosion.remaining = BombConstants.explosionDuration;
                    return;
                }
            }
            explosions.Add(new ExplosionCellState { cell = cell, remaining = BombConstants.explosionDuration });
        }
        private List<Vector2Int> ComputeExplosionCells(Vector2Int origin, int range, ref bool gridChanged)
        {
            var result = new List<Vector2Int> { origin };
            Vector2Int[] directions =
            {
                new Vector2Int(0, 1),
                new Vector2Int(0, -1),
                new Vector2Int(-1, 0),
                new Vector2Int(1, 0)
            };
            foreach (var direction in directions)
            {
                for (int step = 1; step <= range; ++step)
                {
                    int x = origin.x + direction.x * step;
                    int y = origin.y + direction.y * step;
                    if (!grid.InBounds(x, y))
                    {
                        break;
                    }
                    var content = grid.GetCell(x, y);
                    if (content == CellContent.Wall)
                    {
                        break;
                    }
                    result.Add(new Vector2Int(x, y));
                    if (content == CellContent.Box)
                    {
                        grid.SetCell(x, y, CellContent.Empty);
                        gridChanged = true;
                        TryDropUpgrade(new Vector2Int(x, y));
                        break;
                    }
                }
            }
            return result;
        }
    }
}