using UnityEngine;

namespace BombIt.Simulation
{
    public class PlayerState
    {
        public int id;
        public Vector2 position;
        public float collisionRadius = 0.35f;
        public bool alive = true;
        public int rangeLevel;
        public int bombCountLevel;
        public int speedLevel;

        public int GetBombRange()
        {
            return BombConstants.baseRange + rangeLevel;
        }
        public int GetMaxBombs()
        {
            return BombConstants.baseBombCount + bombCountLevel;
        }
        public float GetMoveSpeed()
        {
            return PlayerConstants.baseSpeed + speedLevel * PlayerConstants.speedIncrement;
        }
    }
}