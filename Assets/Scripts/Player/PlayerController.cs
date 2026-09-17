using UnityEngine;

using BombIt.Simulation;
using BombIt.Presentation;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float collisionRadius = 0.35f;
    private GridMap grid;
    private BombSimulation bombs;
    private PlayerState state;
    private PlayerView view;
    private Vector2 inputDirection;
    private bool wasAlive = true;
    public void Build(GridMap gridMap, BombSimulation bombSimulation, Vector2 spawnPosition, int ownerId)
    {
        grid = gridMap;
        bombs = bombSimulation;
        state = new PlayerState
        {
            id = ownerId,
            position = spawnPosition,
            collisionRadius = collisionRadius
        };
        view = gameObject.AddComponent<PlayerView>();
        view.Build(collisionRadius * 2.0f, ownerId);
        view.SyncTo(state);
    }
    public PlayerState GetState()
    {
        return state;
    }
    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        inputDirection = new Vector2(horizontal, vertical);
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryPlaceBomb();
        }
    }
    public void Tick(float deltaTime)
    {
        if (!state.alive)
        {
            if (wasAlive)
            {
                view.SetDead();
                wasAlive = false;
            }
            return;
        }
        PlayerMotor.Move(state, grid, bombs, inputDirection, deltaTime);
        view.SyncTo(state);
    }
    private void TryPlaceBomb()
    {
        if (!state.alive)
        {
            return;
        }
        var cell = GridMap.WorldToGrid(state.position);
        bombs.TryPlaceBomb(state.id, cell, state.GetBombRange(), state.GetMaxBombs());
    }
}