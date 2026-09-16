using UnityEngine;
using BombIt.Simulation;
using BombIt.Presentation;
using System.Collections.Generic;

public class GameBootstrap : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    [SerializeField] private float boxDensity = 0.6f;
    [Tooltip("-1 = random seed each run. any other value produces the same layout")]
    [SerializeField] private int seed = -1;
    private GridMap grid;
    private GridView gridView;
    private PlayerController player;
    private BombSimulation bombSimulation;
    private BombFieldView bombFieldView;
    private readonly List<PlayerState> players = new List<PlayerState>();
    void Start()
    {
        grid = new GridMap();
        grid.GenerateClassicLayout(boxDensity, seed >= 0 ? seed : (int?)null);
        var viewGO = new GameObject("Grid View");
        gridView = viewGO.AddComponent<GridView>();
        gridView.Build(grid);
        bombSimulation = new BombSimulation(grid);
        var bombFieldGO = new GameObject("Bomb Field View");
        bombFieldView = bombFieldGO.AddComponent<BombFieldView>();
        SpawnPlayer();
        FitCameraToGrid();
    }
    private void SpawnPlayer()
    {
        var playerGO = new GameObject("Player");
        player = playerGO.AddComponent<PlayerController>();
        player.Build(grid, bombSimulation, GridMap.GridToWorldCenter(1, 1), 0);
        players.Add(player.GetState());
    }
    void FixedUpdate()
    {
        if (player == null)
        {
            return;
        }
        player.Tick(Time.fixedDeltaTime);
        bool gridChanged = bombSimulation.Tick(Time.fixedDeltaTime, players);
        bombFieldView.Sync(bombSimulation.Bombs, bombSimulation.Explosions, bombSimulation.Pickups);
        if (gridChanged)
        {
            gridView.Refresh(grid);
        }
    }
    private void FitCameraToGrid()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            Debug.Log("GameBootstrap: No main camera found in the scene.");
            return;
        }
        cam.orthographic = true;
        float mapWidth = GridMap.width * GridMap.cellSize;
        float mapHeight = GridMap.height * GridMap.cellSize;
        float sizeToFitWidth = (mapWidth / cam.aspect) / 2.0f;
        float sizeToFitHeight = mapHeight / 2.0f;
        cam.orthographicSize = Mathf.Max(sizeToFitWidth, sizeToFitHeight) * 1.05f;
        cam.transform.position = new Vector3(mapWidth / 2.0f, mapHeight / 2.0f, -10.0f);
        Debug.Log($"GameBootstrap: Camera fit to {mapWidth}x{mapHeight} grid with orthographic size {cam.orthographicSize}");
    }
}