using UnityEngine;
using BombIt.Simulation;

namespace BombIt.Presentation
{
    public class GridView : MonoBehaviour
    {
        [SerializeField] private Color floorColor = new Color(0.85f, 0.85f, 0.85f);
        [SerializeField] private Color wallColor = new Color(0.25f, 0.25f, 0.25f);
        [SerializeField] private Color boxColor = new Color(0.55f, 0.35f, 0.15f);

        private SpriteRenderer[,] tileRenderers;
        private Sprite pixelSprite;

        public void Build(GridMap grid)
        {
            pixelSprite = PlaceholderSprite.CreateSolid(spritePixelSize);
            tileRenderers = new SpriteRenderer[GridMap.width, GridMap.height];

            for (int x = 0; x < GridMap.width; ++x)
            {
                for (int y = 0; y < GridMap.height; ++y)
                {
                    var go = new GameObject($"Tile_{x}_{y}");
                    go.transform.SetParent(transform, false);
                    go.transform.position = GridMap.GridToWorldCenter(x, y);
                    go.transform.localScale = new Vector3(GridMap.cellSize, GridMap.cellSize, 1.0f);

                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = pixelSprite;
                    sr.sortingOrder = -10;
                    tileRenderers[x, y] = sr;
                }
            }

            Refresh(grid);
        }

        public void Refresh(GridMap grid)
        {
            for (int x = 0; x < GridMap.width; ++x)
            {
                for (int y = 0; y < GridMap.height; ++y)
                {
                    tileRenderers[x, y].color = grid.GetCell(x, y) switch
                    {
                        CellContent.Wall => wallColor,
                        CellContent.Box => boxColor,
                        _ => floorColor
                    };
                }
            }
        }

        private const int spritePixelSize = 32;
    }
}