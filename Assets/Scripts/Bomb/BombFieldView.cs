using UnityEngine;

using BombIt.Simulation;
using System.Collections.Generic;
namespace BombIt.Presentation
{
    public class BombFieldView : MonoBehaviour
    {
        private readonly Dictionary<int, GameObject> bombVisuals = new Dictionary<int, GameObject>();
        private readonly Dictionary<Vector2Int, GameObject> explosionVisuals = new Dictionary<Vector2Int, GameObject>();
        private readonly Dictionary<int, GameObject> pickupVisuals = new Dictionary<int, GameObject>();
        public void Sync(IReadOnlyList<BombState> bombs, IReadOnlyList<ExplosionCellState> explosions, IReadOnlyList<UpgradePickupState> pickups)
        {
            SyncBombs(bombs);
            SyncExplosions(explosions);
            SyncPickups(pickups);
        }
        private void SyncBombs(IReadOnlyList<BombState> bombs)
        {
            var toRemove = new List<int>();
            foreach (var pair in bombVisuals)
            {
                bool stillExists = false;
                foreach (var bomb in bombs)
                {
                    if (bomb.id == pair.Key)
                    {
                        stillExists = true;
                        break;
                    }
                }
                if (!stillExists)
                {
                    toRemove.Add(pair.Key);
                }
            }
            foreach (var id in toRemove)
            {
                Destroy(bombVisuals[id]);
                bombVisuals.Remove(id);
            }
            foreach (var bomb in bombs)
            {
                if (!bombVisuals.ContainsKey(bomb.id))
                {
                    var go = new GameObject("Bomb");
                    go.transform.SetParent(transform, false);
                    go.transform.position = GridMap.GridToWorldCenter(bomb.cell.x, bomb.cell.y);
                    go.transform.localScale = new Vector3(0.8f, 0.8f, 1.0f);
                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = SpriteLibrary.GetBomb();
                    sr.color = Color.white;
                    sr.sortingOrder = 1;
                    bombVisuals.Add(bomb.id, go);
                }
            }
        }
        private void SyncExplosions(IReadOnlyList<ExplosionCellState> explosions)
        {
            var toRemove = new List<Vector2Int>();
            foreach (var pair in explosionVisuals)
            {
                bool stillExists = false;
                foreach (var explosion in explosions)
                {
                    if (explosion.cell == pair.Key)
                    {
                        stillExists = true;
                        break;
                    }
                }
                if (!stillExists)
                {
                    toRemove.Add(pair.Key);
                }
            }
            foreach (var cell in toRemove)
            {
                Destroy(explosionVisuals[cell]);
                explosionVisuals.Remove(cell);
            }
            foreach (var explosion in explosions)
            {
                if (!explosionVisuals.ContainsKey(explosion.cell))
                {
                    var go = new GameObject("Explosion");
                    go.transform.SetParent(transform, false);
                    go.transform.position = GridMap.GridToWorldCenter(explosion.cell.x, explosion.cell.y);
                    go.transform.localScale = new Vector3(GridMap.cellSize, GridMap.cellSize, 1.0f);
                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = SpriteLibrary.GetExplosion();
                    sr.color = Color.white;
                    sr.sortingOrder = 2;
                    explosionVisuals.Add(explosion.cell, go);
                }
            }
        }
        private void SyncPickups(IReadOnlyList<UpgradePickupState> pickups)
        {
            var toRemove = new List<int>();
            foreach (var pair in pickupVisuals)
            {
                bool stillExists = false;
                foreach (var pickup in pickups)
                {
                    if (pickup.id == pair.Key)
                    {
                        stillExists = true;
                        break;
                    }
                }
                if (!stillExists)
                {
                    toRemove.Add(pair.Key);
                }
            }
            foreach (var id in toRemove)
            {
                Destroy(pickupVisuals[id]);
                pickupVisuals.Remove(id);
            }
            foreach (var pickup in pickups)
            {
                if (!pickupVisuals.ContainsKey(pickup.id))
                {
                    var go = new GameObject($"Upgrade_{pickup.type}");
                    go.transform.SetParent(transform, false);
                    go.transform.position = GridMap.GridToWorldCenter(pickup.cell.x, pickup.cell.y);
                    go.transform.localScale = new Vector3(0.6f, 0.6f, 1.0f);
                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = SpriteLibrary.GetUpgrade(pickup.type);
                    sr.color = Color.white;
                    sr.sortingOrder = 1;
                    pickupVisuals.Add(pickup.id, go);
                }
            }
        }
    }
}