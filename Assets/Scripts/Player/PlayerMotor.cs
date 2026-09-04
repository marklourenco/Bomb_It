using UnityEngine;

namespace BombIt.Simulation
{
    public static class PlayerMotor
    {
        public static void Move(PlayerState player, GridMap grid, Vector2 inputDirection, float deltaTime)
        {
            if (inputDirection.sqrMagnitude > 1.0f)
            {
                inputDirection = inputDirection.normalized;
            }
            Vector2 delta = inputDirection * player.moveSpeed * deltaTime;
            Vector2 position = player.position;
            float newX = position.x + delta.x;
            if (!IsBlocked(new Vector2(newX, position.y), player.collisionRadius, grid))
            {
                position.x = newX;
            }
            float newY = position.y + delta.y;
            if (!IsBlocked(new Vector2(position.x, newY), player.collisionRadius, grid))
            {
                position.y = newY;
            }
            player.position = position;
        }

        private static bool IsBlocked(Vector2 center, float radius, GridMap grid)
        {
            int minX = Mathf.FloorToInt((center.x - radius) / GridMap.cellSize);
            int maxX = Mathf.FloorToInt((center.x + radius) / GridMap.cellSize);
            int minY = Mathf.FloorToInt((center.y - radius) / GridMap.cellSize);
            int maxY = Mathf.FloorToInt((center.y + radius) / GridMap.cellSize);
            for (int x = minX; x <= maxX; ++x)
            {
                for (int y = minY; y <= maxY; ++y)
                {
                    if (grid.IsSolid(x, y))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}