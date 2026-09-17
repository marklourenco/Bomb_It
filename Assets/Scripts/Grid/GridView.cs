using UnityEngine;
using BombIt.Simulation;

namespace BombIt.Presentation
{
    public class GridView : MonoBehaviour
    {
        // The crate sprite was re-cropped in the Sprite Editor to 32x23 pixels
        // (instead of the 32x32 every other tile uses), so on its own it would
        // only cover part of a cell's height. Stretching it by this factor makes
        // it fill the full cell height again.
        private const float boxSpritePixelWidth = 32.0f;
        private const float boxSpritePixelHeight = 23.0f;
        private const float boxHeightStretch = boxSpritePixelWidth / boxSpritePixelHeight;
        private SpriteRenderer[,] tileRenderers;
        public void Build(GridMap grid)
        {
            tileRenderers = new SpriteRenderer[GridMap.width, GridMap.height];
            for (int x = 0; x < GridMap.width; ++x)
            {
                for (int y = 0; y < GridMap.height; ++y)
                {
                    var go = new GameObject($"Tile_{x}_{y}");
                    go.transform.SetParent(transform, false);
                    var sr = go.AddComponent<SpriteRenderer>();
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
                    var content = grid.GetCell(x, y);
                    var sr = tileRenderers[x, y];
                    var center = GridMap.GridToWorldCenter(x, y);
                    switch (content)
                    {
                        case CellContent.Wall:
                            sr.sprite = SpriteLibrary.GetWall();
                            sr.transform.position = center;
                            sr.transform.localScale = new Vector3(GridMap.cellSize, GridMap.cellSize, 1.0f);
                            sr.sortingOrder = -10;
                            break;
                        case CellContent.Box:
                            sr.sprite = SpriteLibrary.GetBox();
                            sr.transform.position = new Vector3(center.x, center.y - GridMap.cellSize * 0.5f, 0.0f);
                            sr.transform.localScale = new Vector3(GridMap.cellSize, GridMap.cellSize * boxHeightStretch, 1.0f);
                            sr.sortingOrder = -1;
                            break;
                        default:
                            sr.sprite = SpriteLibrary.GetFloor(x, y);
                            sr.transform.position = center;
                            sr.transform.localScale = new Vector3(GridMap.cellSize, GridMap.cellSize, 1.0f);
                            sr.sortingOrder = -10;
                            break;
                    }
                    sr.color = Color.white;
                }
            }
        }
    }
}