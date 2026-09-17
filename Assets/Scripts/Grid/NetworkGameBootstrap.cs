using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using BombIt.Simulation;
using BombIt.Presentation;
using BombIt.Networking;

public class NetworkGameBootstrap : MonoBehaviour
{
    private enum Role
    {
        None,
        Host,
        Client
    }
    [Range(0.0f, 1.0f)]
    [SerializeField] private float boxDensity = 0.6f;
    [Tooltip("Default seed used if the lobby's seed field is left blank. -1 = random seed each run, any other value produces the same layout")]
    [SerializeField] private int seed = -1;
    [SerializeField] private float collisionRadius = 0.35f;
    private Role role = Role.None;
    private GameHost host;
    private GameClient client;
    private int hostPort = NetworkConstants.defaultPort;
    private string clientAddressInput = "127.0.0.1";
    private string clientPortInput = "7777";
    private GridMap grid;
    private GridView gridView;
    private BombFieldView bombFieldView;
    private BombSimulation bombSimulation;
    private readonly List<PlayerState> players = new List<PlayerState>();
    private readonly Dictionary<int, int> lastProcessedBombRequestCount = new Dictionary<int, int>();
    private readonly Dictionary<int, PlayerView> playerViews = new Dictionary<int, PlayerView>();
    private PlayerInputState localInput;
    private int localBombRequestCount;
    private LobbyUI lobbyUI;
    private bool gameStarted;
    private bool gameOver;
    private int winnerId = -1;
    private bool clientGameStarted;
    private bool clientGameOver;
    private int clientWinnerId = -1;
    void Start()
    {
        SetCameraBackground();
        var lobbyGO = new GameObject("Lobby UI");
        lobbyUI = lobbyGO.AddComponent<LobbyUI>();
        lobbyUI.OnStartHostClicked = HandleStartHostRequested;
        lobbyUI.OnConnectClicked = HandleConnectRequested;
        lobbyUI.OnStopClicked = HandleStopRequested;
        lobbyUI.OnStartGameClicked = HandleStartGameRequested;
        lobbyUI.OnPlayAgainClicked = HandlePlayAgainRequested;
        lobbyUI.Build();
    }
    void Update()
    {
        if (role == Role.Host)
        {
            host.Update(Time.deltaTime);
            UpdateHostStatusUI();
        }
        else if (role == Role.Client)
        {
            client.Update(Time.deltaTime);
            UpdateClientStatusUI();
        }
        if (role == Role.Host || role == Role.Client)
        {
            ReadLocalInput();
        }
    }
    private void HandleStartHostRequested(int port, int requestedSeed)
    {
        hostPort = port;
        seed = requestedSeed;
        StartHosting();
    }
    private void HandleConnectRequested(string address, int port)
    {
        clientAddressInput = address;
        clientPortInput = port.ToString();
        StartAsClient();
    }
    private void HandleStopRequested()
    {
        if (role == Role.Host)
        {
            StopHosting();
        }
        else if (role == Role.Client)
        {
            DisconnectClient();
        }
        lobbyUI.ShowLobby();
    }
    private void HandleStartGameRequested()
    {
        if (role == Role.Host)
        {
            gameStarted = true;
        }
    }
    private void HandlePlayAgainRequested()
    {
        if (role == Role.Host && gameOver)
        {
            RestartRound();
        }
    }
    private static string BuildGameOverMessage(int winner)
    {
        return winner >= 0 ? $"Player {winner} wins!" : "It's a draw!";
    }
    private void UpdateHostStatusUI()
    {
        var peerList = new System.Text.StringBuilder();
        foreach (var peer in host.Peers)
        {
            peerList.AppendLine($"Player {peer.playerId}  -  {peer.state}");
        }
        var status = $"Hosting on port {hostPort}. You are player 0.\nLocal network address: {GetLocalIPAddress()}:{hostPort}";
        status += gameStarted ? "\nGame in progress." : "\nWaiting to start - press Start once everyone has joined.";
        lobbyUI.ShowStatus(status, peerList.ToString(), !gameStarted && !gameOver);
        if (gameOver)
        {
            lobbyUI.ShowGameOver(BuildGameOverMessage(winnerId), true);
        }
        else
        {
            lobbyUI.HideGameOver();
        }
    }
    private void UpdateClientStatusUI()
    {
        var status = $"State: {client.State}";
        if (client.State == PeerState.Connected)
        {
            status += $"\nConnected as player {client.LocalPlayerId}";
            status += clientGameStarted ? "\nGame in progress." : "\nWaiting for host to start the game...";
        }
        if (client.WasRejected)
        {
            status += "\nConnection was rejected (host may be full).";
        }
        lobbyUI.ShowStatus(status, "", false);
        if (clientGameOver)
        {
            lobbyUI.ShowGameOver(BuildGameOverMessage(clientWinnerId), false);
        }
        else
        {
            lobbyUI.HideGameOver();
        }
    }
    void FixedUpdate()
    {
        if (role == Role.Host)
        {
            HostTick(Time.fixedDeltaTime);
        }
        else if (role == Role.Client)
        {
            ClientTick();
        }
    }
    private void ReadLocalInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ++localBombRequestCount;
        }
        localInput = new PlayerInputState
        {
            moveX = horizontal,
            moveY = vertical,
            bombRequestCount = localBombRequestCount
        };
    }
    // host
    private void StartHosting()
    {
        grid = new GridMap();
        grid.GenerateClassicLayout(boxDensity, seed >= 0 ? seed : (int?)null);
        BuildSharedPresentation();
        bombSimulation = new BombSimulation(grid);
        players.Clear();
        players.Add(CreatePlayerState(0));
        lastProcessedBombRequestCount.Clear();
        localBombRequestCount = 0;
        gameStarted = false;
        gameOver = false;
        winnerId = -1;
        host = new GameHost();
        host.Start(hostPort);
        role = Role.Host;
        FitCameraToGrid();
    }
    private void StopHosting()
    {
        host.Stop();
        host = null;
        CleanupPresentation();
        players.Clear();
        gameStarted = false;
        gameOver = false;
        winnerId = -1;
        role = Role.None;
    }
    private void RestartRound()
    {
        grid.GenerateClassicLayout(boxDensity, seed >= 0 ? seed : (int?)null);
        gridView.Refresh(grid);
        bombSimulation = new BombSimulation(grid);
        bombFieldView.Sync(bombSimulation.Bombs, bombSimulation.Explosions, bombSimulation.Pickups);
        lastProcessedBombRequestCount.Clear();
        foreach (var player in players)
        {
            player.position = GetSpawnPosition(player.id);
            player.alive = true;
            player.rangeLevel = 0;
            player.bombCountLevel = 0;
            player.speedLevel = 0;
            var view = GetOrCreatePlayerView(player.id);
            view.SyncTo(player);
            view.SetAlive();
        }
        gameOver = false;
        winnerId = -1;
        gameStarted = false;
    }
    private void HostTick(float deltaTime)
    {
        ReconcilePlayers();
        bool gridChanged = false;
        if (gameStarted && !gameOver)
        {
            var hostPlayer = FindPlayer(0);
            if (hostPlayer != null)
            {
                ApplyInputToPlayer(hostPlayer, localInput, deltaTime);
            }
            foreach (var player in players)
            {
                if (player.id == 0)
                {
                    continue;
                }
                PlayerInputState input;
                if (host.TryGetLatestInput(player.id, out input))
                {
                    ApplyInputToPlayer(player, input, deltaTime);
                }
            }
            gridChanged = bombSimulation.Tick(deltaTime, players);
            CheckWinCondition();
        }
        SyncHostPresentation(gridChanged);
        var snapshot = StateSnapshot.Write(grid, players, bombSimulation, gameStarted, gameOver, winnerId);
        host.SendToAll(snapshot, PacketChannel.Unreliable);
    }
    private void CheckWinCondition()
    {
        if (players.Count < 2)
        {
            return;
        }
        int aliveCount = 0;
        int lastAliveId = -1;
        foreach (var player in players)
        {
            if (player.alive)
            {
                ++aliveCount;
                lastAliveId = player.id;
            }
        }
        if (aliveCount <= 1)
        {
            gameOver = true;
            winnerId = aliveCount == 1 ? lastAliveId : -1;
        }
    }
    private void ApplyInputToPlayer(PlayerState player, PlayerInputState input, float deltaTime)
    {
        if (!player.alive)
        {
            return;
        }
        var direction = new Vector2(input.moveX, input.moveY);
        PlayerMotor.Move(player, grid, bombSimulation, direction, deltaTime);
        int lastCount;
        bool hasBaseline = lastProcessedBombRequestCount.TryGetValue(player.id, out lastCount);
        if (hasBaseline && input.bombRequestCount > lastCount)
        {
            var cell = GridMap.WorldToGrid(player.position);
            bombSimulation.TryPlaceBomb(player.id, cell, player.GetBombRange(), player.GetMaxBombs());
        }
        lastProcessedBombRequestCount[player.id] = input.bombRequestCount;
    }
    private void ReconcilePlayers()
    {
        foreach (var peer in host.Peers)
        {
            if (FindPlayer(peer.playerId) == null)
            {
                players.Add(CreatePlayerState(peer.playerId));
            }
        }
        for (int i = players.Count - 1; i >= 0; --i)
        {
            var player = players[i];
            if (player.id == 0)
            {
                continue;
            }
            bool stillConnected = false;
            foreach (var peer in host.Peers)
            {
                if (peer.playerId == player.id)
                {
                    stillConnected = true;
                    break;
                }
            }
            if (!stillConnected)
            {
                players.RemoveAt(i);
                RemovePlayerView(player.id);
                lastProcessedBombRequestCount.Remove(player.id);
            }
        }
    }
    private void SyncHostPresentation(bool gridChanged)
    {
        if (gridChanged)
        {
            gridView.Refresh(grid);
        }
        bombFieldView.Sync(bombSimulation.Bombs, bombSimulation.Explosions, bombSimulation.Pickups);
        foreach (var player in players)
        {
            var view = GetOrCreatePlayerView(player.id);
            view.SyncTo(player);
            if (!player.alive)
            {
                view.SetDead();
            }
        }
    }
    // client
    private void StartAsClient()
    {
        grid = new GridMap();
        BuildSharedPresentation();
        localBombRequestCount = 0;
        clientGameStarted = false;
        clientGameOver = false;
        clientWinnerId = -1;
        client = new GameClient();
        client.Connect(clientAddressInput, int.Parse(clientPortInput), 0);
        role = Role.Client;
        FitCameraToGrid();
    }
    private void DisconnectClient()
    {
        client.Disconnect();
        client = null;
        CleanupPresentation();
        role = Role.None;
    }
    private void ClientTick()
    {
        if (client.State != PeerState.Connected)
        {
            return;
        }
        client.SendInput(localInput);
        ApplySnapshot(client.LatestSnapshot);
    }
    private void ApplySnapshot(SnapshotData snapshot)
    {
        if (snapshot == null)
        {
            return;
        }
        clientGameStarted = snapshot.gameStarted;
        clientGameOver = snapshot.gameOver;
        clientWinnerId = snapshot.winnerId;
        ApplyGridCells(snapshot.gridCells);
        var seenPlayerIds = new HashSet<int>();
        foreach (var playerData in snapshot.players)
        {
            var playerState = new PlayerState
            {
                id = playerData.id,
                position = new Vector2(playerData.posX, playerData.posY),
                alive = playerData.alive,
                rangeLevel = playerData.rangeLevel,
                bombCountLevel = playerData.bombCountLevel,
                speedLevel = playerData.speedLevel
            };
            seenPlayerIds.Add(playerData.id);
            var view = GetOrCreatePlayerView(playerData.id);
            view.SyncTo(playerState);
            if (!playerState.alive)
            {
                view.SetDead();
            }
        }
        RemoveStalePlayerViews(seenPlayerIds);
        var bombs = new List<BombState>();
        foreach (var bombData in snapshot.bombs)
        {
            bombs.Add(new BombState
            {
                id = bombData.id,
                ownerId = bombData.ownerId,
                cell = new Vector2Int(bombData.cellX, bombData.cellY),
                range = bombData.range,
                fuseRemaining = bombData.fuseRemaining
            });
        }
        var explosions = new List<ExplosionCellState>();
        foreach (var explosionData in snapshot.explosions)
        {
            explosions.Add(new ExplosionCellState
            {
                cell = new Vector2Int(explosionData.cellX, explosionData.cellY),
                remaining = explosionData.remaining
            });
        }
        var pickups = new List<UpgradePickupState>();
        foreach (var pickupData in snapshot.pickups)
        {
            pickups.Add(new UpgradePickupState
            {
                id = pickupData.id,
                cell = new Vector2Int(pickupData.cellX, pickupData.cellY),
                type = pickupData.type
            });
        }
        bombFieldView.Sync(bombs, explosions, pickups);
    }
    private void ApplyGridCells(CellContent[] cells)
    {
        for (int y = 0; y < GridMap.height; ++y)
        {
            for (int x = 0; x < GridMap.width; ++x)
            {
                grid.SetCell(x, y, cells[y * GridMap.width + x]);
            }
        }
        gridView.Refresh(grid);
    }
    // helpers
    private Vector2 GetSpawnPosition(int playerId)
    {
        switch (playerId)
        {
            case 0:
                return GridMap.GridToWorldCenter(1, 1);
            case 1:
                return GridMap.GridToWorldCenter(GridMap.width - 2, 1);
            case 2:
                return GridMap.GridToWorldCenter(1, GridMap.height - 2);
            case 3:
                return GridMap.GridToWorldCenter(GridMap.width - 2, GridMap.height - 2);
            default:
                return GridMap.GridToWorldCenter(1, 1);
        }
    }
    private PlayerState CreatePlayerState(int playerId)
    {
        return new PlayerState
        {
            id = playerId,
            position = GetSpawnPosition(playerId),
            collisionRadius = collisionRadius
        };
    }
    private PlayerState FindPlayer(int playerId)
    {
        foreach (var player in players)
        {
            if (player.id == playerId)
            {
                return player;
            }
        }
        return null;
    }
    private PlayerView GetOrCreatePlayerView(int playerId)
    {
        PlayerView view;
        if (playerViews.TryGetValue(playerId, out view))
        {
            return view;
        }
        var go = new GameObject($"Player_{playerId}");
        view = go.AddComponent<PlayerView>();
        view.Build(collisionRadius * 2.0f, playerId);
        playerViews.Add(playerId, view);
        return view;
    }
    private void RemovePlayerView(int playerId)
    {
        PlayerView view;
        if (playerViews.TryGetValue(playerId, out view))
        {
            Destroy(view.gameObject);
            playerViews.Remove(playerId);
        }
    }
    private void RemoveStalePlayerViews(HashSet<int> currentPlayerIds)
    {
        var toRemove = new List<int>();
        foreach (var id in playerViews.Keys)
        {
            if (!currentPlayerIds.Contains(id))
            {
                toRemove.Add(id);
            }
        }
        foreach (var id in toRemove)
        {
            RemovePlayerView(id);
        }
    }
    private void BuildSharedPresentation()
    {
        var gridGO = new GameObject("Grid View");
        gridView = gridGO.AddComponent<GridView>();
        gridView.Build(grid);
        var bombFieldGO = new GameObject("Bomb Field View");
        bombFieldView = bombFieldGO.AddComponent<BombFieldView>();
    }
    private void CleanupPresentation()
    {
        foreach (var view in playerViews.Values)
        {
            Destroy(view.gameObject);
        }
        playerViews.Clear();
        if (gridView != null)
        {
            Destroy(gridView.gameObject);
            gridView = null;
        }
        if (bombFieldView != null)
        {
            Destroy(bombFieldView.gameObject);
            bombFieldView = null;
        }
    }
    private void SetCameraBackground()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            return;
        }
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
    }
    private void FitCameraToGrid()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            Debug.Log("NetworkGameBootstrap: No main camera found in the scene.");
            return;
        }
        cam.orthographic = true;
        float mapWidth = GridMap.width * GridMap.cellSize;
        float mapHeight = GridMap.height * GridMap.cellSize;
        float sizeToFitWidth = (mapWidth / cam.aspect) / 2.0f;
        float sizeToFitHeight = mapHeight / 2.0f;
        cam.orthographicSize = Mathf.Max(sizeToFitWidth, sizeToFitHeight) * 1.05f;
        cam.transform.position = new Vector3(mapWidth / 2.0f, mapHeight / 2.0f, -10.0f);
    }
    private string GetLocalIPAddress()
    {
        using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
        {
            socket.Connect("8.8.8.8", 65530);
            var endPoint = (IPEndPoint)socket.LocalEndPoint;
            return endPoint.Address.ToString();
        }
    }
    void OnDestroy()
    {
        if (host != null)
        {
            host.Stop();
        }
        if (client != null)
        {
            client.Disconnect();
        }
    }
}