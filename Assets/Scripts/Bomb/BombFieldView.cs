using UnityEngine;

using BombIt.Simulation;
using System.Collections.Generic;
namespace BombIt.Presentation
{
    public class BombFieldView : MonoBehaviour
    {
        [SerializeField] private Color bombColor = new Color(0.1f, 0.1f, 0.1f);
        [SerializeField] private Color explosionColor = new Color(1.0f, 0.5f, 0.1f);
        [SerializeField] private Color rangeUpgradeColor = new Color(0.9f, 0.2f, 0.2f);
        [SerializeField] private Color bombCountUpgradeColor = new Color(0.2f, 0.6f, 0.9f);
        [SerializeField] private Color speedUpgradeColor = new Color(0.9f, 0.85f, 0.2f);
        private const int spritePixelSize = 32;
        private readonly Dictionary<BombState, GameObject> bombVisuals = new Dictionary<BombState, GameObject>();
        private readonly Dictionary<ExplosionCellState, GameObject> explosionVisuals = new Dictionary<ExplosionCellState, GameObject>();
        private readonly Dictionary<UpgradePickupState, GameObject> pickupVisuals = new Dictionary<UpgradePickupState, GameObject>();

        public void Sync(BombSimulation simulation)
        {
            SyncBombs(simulation.Bombs);
            SyncExplosions(simulation.Explosions);
            SyncPickups(simulation.Pickups);
        }

        private void SyncBombs(IReadOnlyList<BombState> bombs)
        {
            var toRemove = new List<BombState>();
            foreach (var pair in bombVisuals)
            {
                bool stillExists = false;
                foreach (var bomb in bombs)
                {
                    if (bomb == pair.Key)
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
            foreach (var bomb in toRemove)
            {
                Destroy(bombVisuals[bomb]);
                bombVisuals.Remove(bomb);
            }
            foreach (var bomb in bombs)
            {
                if (!bombVisuals.ContainsKey(bomb))
                {
                    var go = new GameObject("Bomb");
                    go.transform.SetParent(transform, false);
                    go.transform.position = GridMap.GridToWorldCenter(bomb.cell.x, bomb.cell.y);
                    go.transform.localScale = new Vector3(0.6f, 0.6f, 1.0f);
                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = PlaceholderSprite.CreateSolid(spritePixelSize);
                    sr.color = bombColor;
                    sr.sortingOrder = 1;
                    bombVisuals.Add(bomb, go);
                }
            }
        }
        private void SyncExplosions(IReadOnlyList<ExplosionCellState> explosions)
        {
            var toRemove = new List<ExplosionCellState>();
            foreach (var pair in explosionVisuals)
            {
                bool stillExists = false;
                foreach (var explosion in explosions)
                {
                    if (explosion == pair.Key)
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
            foreach (var explosion in toRemove)
            {
                Destroy(explosionVisuals[explosion]);
                explosionVisuals.Remove(explosion);
            }
            foreach (var explosion in explosions)
            {
                if (!explosionVisuals.ContainsKey(explosion))
                {
                    var go = new GameObject("Explosion");
                    go.transform.SetParent(transform, false);
                    go.transform.position = GridMap.GridToWorldCenter(explosion.cell.x, explosion.cell.y);
                    go.transform.localScale = new Vector3(GridMap.cellSize, GridMap.cellSize, 1.0f);
                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = PlaceholderSprite.CreateSolid(spritePixelSize);
                    sr.color = explosionColor;
                    sr.sortingOrder = 2;
                    explosionVisuals.Add(explosion, go);
                }
            }
        }

        private void SyncPickups(IReadOnlyList<UpgradePickupState> pickups)
        {
            var toRemove = new List<UpgradePickupState>();
            foreach (var pair in pickupVisuals)
            {
                bool stillExists = false;
                foreach (var pickup in pickups)
                {
                    if (pickup == pair.Key)
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
            foreach (var pickup in toRemove)
            {
                Destroy(pickupVisuals[pickup]);
                pickupVisuals.Remove(pickup);
            }
            foreach (var pickup in pickups)
            {
                if (!pickupVisuals.ContainsKey(pickup))
                {
                    var go = new GameObject($"Upgrade_{pickup.type}");
                    go.transform.SetParent(transform, false);
                    go.transform.position = GridMap.GridToWorldCenter(pickup.cell.x, pickup.cell.y);
                    go.transform.localScale = new Vector3(0.5f, 0.5f, 1.0f);
                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = PlaceholderSprite.CreateSolid(spritePixelSize);
                    sr.color = ColorForUpgrade(pickup.type);
                    sr.sortingOrder = 1;
                    pickupVisuals.Add(pickup, go);
                }
            }
        }

        private Color ColorForUpgrade(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.Range:
                    return rangeUpgradeColor;
                case UpgradeType.BombCount:
                    return bombCountUpgradeColor;
                case UpgradeType.Speed:
                    return speedUpgradeColor;
                default:
                    return Color.white;
            }
        }
    }
}