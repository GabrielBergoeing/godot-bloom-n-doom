using Godot;
using System;

public partial class FarmNetworkService : Node
{
    public static FarmNetworkService Instance { get; private set; }

    private SteamMatchManager Match => SteamMatchManager.Instance;
    private NetworkRoot Network => NetworkRoot.Instance;
    private FarmManager Farm => FarmManager.Instance;

    private bool _initialized = false;

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _ExitTree()
    {
        if (!_initialized) return;
        if (Match == null) return;

        Match.OnFarmPrepared -= HandleFarmPrepared;
        Match.OnFarmPlanted -= HandleFarmPlanted;
        Match.OnFarmRemoved -= HandleFarmRemoved;
        Match.OnFarmIrrigated -= HandleFarmIrrigated;
        Match.OnFarmFertilized -= HandleFarmFertilized;
    }

    public void Initialize()
    {
        _initialized = true;

        Match.OnFarmPrepared += HandleFarmPrepared;
        Match.OnFarmPlanted += HandleFarmPlanted;
        Match.OnFarmRemoved += HandleFarmRemoved;
        Match.OnFarmIrrigated += HandleFarmIrrigated;
        Match.OnFarmFertilized += HandleFarmFertilized;

        GD.Print($"[FarmNetworkService] Initialized — IsHost: {Network.Lobby.IsHost}");
    }

    public void RequestPrepareTile(Vector2I cell)
    {
        if (Network.Lobby.IsHost)
            AuthoritativePrepareTile(cell);
        else
            Match.BroadcastFarmPrepared(cell); // peer sends request to host
    }

    public void RequestPlantSeed(Vector2I cell, int playerIndex, SeedData seed)
    {
        if (Network.Lobby.IsHost)
            AuthoritativePlantSeed(cell, playerIndex, seed);
        else
            Match.BroadcastFarmPlanted(cell, playerIndex, seed.ResourcePath);
    }

    public void RequestRemovePlant(Vector2I cell, int playerIndex)
    {
        if (Network.Lobby.IsHost)
            AuthoritativeRemovePlant(cell, playerIndex);
        else
            Match.BroadcastFarmRemoved(cell, playerIndex);
    }

    public void RequestIrrigatePlant(Vector2I cell)
    {
        if (Network.Lobby.IsHost)
            AuthoritativeIrrigatePlant(cell);
        else
            Match.BroadcastFarmIrrigated(cell);
    }

    public void RequestFertilizePlant(Vector2I cell)
    {
        if (Network.Lobby.IsHost)
            AuthoritativeFertilizePlant(cell);
        else
            Match.BroadcastFarmFertilized(cell);
    }

    private void AuthoritativePrepareTile(Vector2I cell)
    {
        if (!Farm.CanPrepareTile(cell)) return;
        Farm.PrepareTile(cell);
        Match.BroadcastFarmPrepared(cell);
        GD.Print($"[FarmNetworkService] Prepared tile {cell}");
    }

    private void AuthoritativePlantSeed(Vector2I cell, int playerIndex, SeedData seed)
    {
        if (!Farm.CanPlantSeed(cell)) return;
        Farm.PlantSeed(cell, playerIndex, seed);
        Match.BroadcastFarmPlanted(cell, playerIndex, seed.ResourcePath);
        GD.Print($"[FarmNetworkService] Planted {seed.ResourcePath} at {cell}");
    }

    private void AuthoritativeRemovePlant(Vector2I cell, int playerIndex)
    {
        if (!Farm.CanRemovePlant(cell, playerIndex)) return;
        Farm.RemovePlant(cell);
        Match.BroadcastFarmRemoved(cell, playerIndex);
        GD.Print($"[FarmNetworkService] Removed plant at {cell}");
    }

    private void AuthoritativeIrrigatePlant(Vector2I cell)
    {
        if (!Farm.CanIrrigatePlant(cell)) return;
        Farm.IrrigatePlant(cell);
        Match.BroadcastFarmIrrigated(cell);
        GD.Print($"[FarmNetworkService] Irrigated plant at {cell}");
    }

    private void AuthoritativeFertilizePlant(Vector2I cell)
    {
        if (!Farm.CanFertilizePlant(cell)) return;
        Farm.FertilizePlant(cell);
        Match.BroadcastFarmFertilized(cell);
        GD.Print($"[FarmNetworkService] Fertilized plant at {cell}");
    }

    private void HandleFarmPrepared(MatchFarmPreparePacket packet)
    {
        if (Network.Lobby.IsHost)
        {
            // Host receives this as a peer request
            AuthoritativePrepareTile(packet.Cell);
            return;
        }
        // Peer applies directly
        Farm.PrepareTile(packet.Cell);
        GD.Print($"[FarmNetworkService] Peer applied prepare at {packet.Cell}");
    }

    private void HandleFarmPlanted(MatchFarmPlantPacket packet)
    {
        if (Network.Lobby.IsHost)
        {
            SeedData seed = GD.Load<SeedData>(packet.SeedId);
            if (seed == null)
            {
                GD.PrintErr($"[FarmNetworkService] Seed not found: {packet.SeedId}");
                return;
            }
            AuthoritativePlantSeed(packet.Cell, packet.PlayerIndex, seed);
            return;
        }

        SeedData peerSeed = GD.Load<SeedData>(packet.SeedId);
        if (peerSeed == null)
        {
            GD.PrintErr($"[FarmNetworkService] Seed not found: {packet.SeedId}");
            return;
        }
        Farm.PlantSeed(packet.Cell, packet.PlayerIndex, peerSeed);
        GD.Print($"[FarmNetworkService] Peer applied plant at {packet.Cell}");
    }

    private void HandleFarmRemoved(MatchFarmRemovePacket packet)
    {
        if (Network.Lobby.IsHost)
        {
            // Need playerIndex for validation — stored in packet if added
            Farm.RemovePlant(packet.Cell);
            Match.BroadcastFarmRemoved(packet.Cell, packet.PlayerIndex);
            return;
        }
        Farm.RemovePlant(packet.Cell);
        GD.Print($"[FarmNetworkService] Peer applied remove at {packet.Cell}");
    }

    private void HandleFarmIrrigated(MatchFarmIrrigatePacket packet)
    {
        if (Network.Lobby.IsHost)
        {
            AuthoritativeIrrigatePlant(packet.Cell);
            return;
        }
        Farm.IrrigatePlant(packet.Cell);
        GD.Print($"[FarmNetworkService] Peer applied irrigate at {packet.Cell}");
    }

    private void HandleFarmFertilized(MatchFarmFertilizePacket packet)
    {
        if (Network.Lobby.IsHost)
        {
            AuthoritativeFertilizePlant(packet.Cell);
            return;
        }
        Farm.FertilizePlant(packet.Cell);
        GD.Print($"[FarmNetworkService] Peer applied fertilize at {packet.Cell}");
    }
}