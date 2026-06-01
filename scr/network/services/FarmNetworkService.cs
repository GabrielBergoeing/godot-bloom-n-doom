using Godot;
using System;

public partial class FarmNetworkService : Node
{
    public static FarmNetworkService Instance { get; private set; }

    private SteamMatchManager Match => SteamMatchManager.Instance;
    private NetworkRoot Network => NetworkRoot.Instance;
    private FarmManager Farm => FarmManager.Instance;

    public override void _Ready()
    {
        Instance = this;
    }

    public void PrepareTile(Vector2I cell)
    {
        if (!Network.Lobby.IsHost)
            return;

        if (!Farm.CanPrepareTile(cell))
            return;

        Farm.PrepareTile(cell);

        Match.BroadcastFarmPrepared(cell);
    }

    public void PlantSeed(Vector2I cell, int playerIndex, SeedData seed)
    {
        if (!Network.Lobby.IsHost)
            return;

        if (!Farm.CanPlantSeed(cell))
            return;

        Farm.PlantSeed(cell, playerIndex, seed);

        Match.BroadcastFarmPlanted(
            cell,
            playerIndex,
            seed.ResourcePath
        );
    }

    public void RemovePlant(Vector2I cell, int playerIndex)
    {
        if (!Network.Lobby.IsHost)
            return;

        if (!Farm.CanRemovePlant(cell, playerIndex))
            return;

        Farm.RemovePlant(cell);

        Match.BroadcastFarmRemoved(cell);
    }

    public void IrrigatePlant(Vector2I cell)
    {
        if (!Network.Lobby.IsHost)
            return;

        if (!Farm.CanIrrigatePlant(cell))
            return;

        Farm.IrrigatePlant(cell);

        Match.BroadcastFarmIrrigated(cell);
    }

    public void FertilizePlant(Vector2I cell)
    {
        if (!Network.Lobby.IsHost)
            return;

        if (!Farm.CanFertilizePlant(cell))
            return;

        Farm.FertilizePlant(cell);

        Match.BroadcastFarmFertilized(cell);
    }
}