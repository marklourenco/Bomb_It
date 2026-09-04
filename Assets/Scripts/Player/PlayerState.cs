using UnityEngine;

namespace BombIt.Simulation
{
    public class PlayerState
    {
        public int id;
        public Vector2 position;
        public float moveSpeed = 4.0f;
        public float collisionRadius = 0.35f;
        public bool alive = true;
    }
}