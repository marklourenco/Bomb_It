using UnityEngine;

using BombIt.Simulation;
using BombIt.Presentation;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4.0f;
    [SerializeField] private float collisionRadius = 0.35f;
    private GridMap grid;
    private PlayerState state;
    private PlayerView view;
    private Vector2 inputDirection;

    public void Build(GridMap gridMap, Vector2 spawnPosition)
    {
        grid = gridMap;
        state = new PlayerState
        {
            position = spawnPosition,
            moveSpeed = moveSpeed,
            collisionRadius = collisionRadius
        };
        view = gameObject.AddComponent<PlayerView>();
        view.Build(collisionRadius * 2.0f);
        view.SyncTo(state);
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        inputDirection = new Vector2(horizontal, vertical);
    }

    void FixedUpdate()
    {
        PlayerMotor.Move(state, grid, inputDirection, Time.fixedDeltaTime);
        view.SyncTo(state);
    }
}
