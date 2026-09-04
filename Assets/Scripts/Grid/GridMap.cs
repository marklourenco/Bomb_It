using UnityEngine;

namespace BombIt.Simulation
{
    public enum CellContent
    {
        Empty,
        Wall,
        Box
    }

    public class GridMap
    {
        public const int width = 13;
        public const int height = 11;
        public const float cellSize = 1.0f;

        private readonly CellContent[,] cells = new CellContent[width, height];

        public CellContent GetCell(int x, int y)
        {
            if (!InBounds(x, y))
            {
                return CellContent.Wall;
            }

            return cells[x, y];
        }

        public void SetCell(int x, int y, CellContent content)
        {
            if (!InBounds(x, y))
            {
                return;
            }

            cells[x, y] = content;
        }

        public bool InBounds(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        public bool IsSolid(int x, int y)
        {
            var content = GetCell(x, y);
            return content == CellContent.Wall || content == CellContent.Box;
        }

        public static Vector2Int WorldToGrid(Vector2 worldPos)
        {
            return new Vector2Int(
                Mathf.FloorToInt(worldPos.x / cellSize),
                Mathf.FloorToInt(worldPos.y / cellSize)
            );
        }

        public static Vector2 GridToWorldCenter(int x, int y)
        {
            return new Vector2(
                (x + 0.5f) * cellSize,
                (y + 0.5f) * cellSize
            );
        }

        public void GenerateClassicLayout(float boxDensity = 0.6f, int? seed = null)
        {
            var rng = seed.HasValue ? new System.Random(seed.Value) : new System.Random();

            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    bool isBorder = x == 0 || y == 0 || x == width - 1 || y == height - 1;
                    bool isPillar = x % 2 == 0 && y % 2 == 0 && !isBorder;

                    cells[x, y] = (isBorder || isPillar) ? CellContent.Wall : CellContent.Empty;
                }
            }

            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    if (cells[x, y] == CellContent.Empty && rng.NextDouble() < boxDensity)
                    {
                        cells[x, y] = CellContent.Box;
                    }
                }
            }

            ClearSpawnCorners();
        }

        private void ClearSpawnCorners()
        {
            ClearArea(1, 1);
            ClearArea(width - 2, 1);
            ClearArea(1, height - 2);
            ClearArea(width - 2, height - 2);
        }

        private void ClearArea(int cx, int cy)
        {
            (int dx, int dy)[] offsets =
            {
                (0, 0), (1, 0), (-1, 0), (0, 1), (0, -1)
            };

            foreach (var (dx, dy) in offsets)
            {
                int x = cx + dx;
                int y = cy + dy;

                if (InBounds(x, y) && cells[x, y] != CellContent.Wall)
                {
                    cells[x, y] = CellContent.Empty;
                }
            }
        }
    }
}