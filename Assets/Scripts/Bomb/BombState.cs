using UnityEngine;

namespace BombIt.Simulation
{
    public class BombState
    {
        public int id;
        public int ownerId;
        public Vector2Int cell;
        public int range;
        public float fuseRemaining;
        public bool ownerStillOnCell;
    }
}