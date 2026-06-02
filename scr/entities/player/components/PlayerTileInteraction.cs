using Godot;
using System;

public partial class PlayerTileInteraction : Sprite2D
{
    private FarmManager Farm => FarmManager.Instance;
    private FarmNetworkService FarmNetwork => FarmNetworkService.Instance;

    private Camera2D cam;
    private Player Player;

    private Node2D currentOutline;
    private Vector2I currentCell;
    public Vector2I CurrentCell => currentCell;

    [Signal] public delegate void RequestIrrigateEventHandler(Vector2I cell);
    [Signal] public delegate void RequestPrepareEventHandler(Vector2I cell);
    [Signal] public delegate void RequestPlantEventHandler(Vector2I cell, int playerIndex, SeedData seed);
    [Signal] public delegate void RequestFertilizeEventHandler(Vector2I cell);
    [Signal] public delegate void RequestRemoveEventHandler(Vector2I cell, int playerIndex);

    public override void _Ready()
    {
        Player = GetParent<Player>();

        var level = SplitScreenManager.Instance.LevelNode;
        ConnectFarmSignals();
        var remote = Player.GetNode<RemoteTransform2D>("RemoteTransform2D");

        if (remote != null && !remote.RemotePath.IsEmpty)
            cam = GetNode<Camera2D>(remote.RemotePath);
    }

    public override void _Process(double delta)
    {
        if (!IsInstanceValid(Player))
            return;

        Vector2 playerWorldPos = Player.GlobalPosition;

        Vector2I playerCell = Farm.LocalToMap(playerWorldPos);
        Vector2I frontCell = GetCellInFrontOfPlayer(playerCell);

        currentCell = frontCell;

        Vector2 cellCenter = Farm.ToGlobal(Farm.MapToLocal(currentCell));
        GlobalPosition = cellCenter;
    }

    public void IrrigateInCell() => 
        EmitSignal(SignalName.RequestIrrigate, currentCell);

    public void PrepareInCell() => 
        EmitSignal(SignalName.RequestPrepare, currentCell);

    public void PlantInCell(int playerIndex, SeedData seed) =>
        EmitSignal(SignalName.RequestPlant, currentCell, playerIndex, seed);

    public void FertilizeInCell() => 
        EmitSignal(SignalName.RequestFertilize, currentCell);

    public void RemoveInCell(int playerIndex) => 
        EmitSignal(SignalName.RequestRemove, currentCell, playerIndex);


    public bool CellIsPrepared() => 
        Farm.IsPrepared(currentCell);

    public bool CellIsOccupied() => 
        Farm.IsOccupied(currentCell);

    public bool IsCellOwner(int playerIndex) => 
        playerIndex == Farm.GetPlantOwner(currentCell);


    public bool CanPrepare() => 
        !CellIsPrepared() && Farm.IsGrass(currentCell);

    public bool CanPlant() => 
        CellIsPrepared() && !CellIsOccupied();

    public bool CanInteractPlant(int playerIndex) => 
        CellIsOccupied() && IsCellOwner(playerIndex);

    public bool CanSabotage(int playerIndex) => 
        CellIsOccupied() && !IsCellOwner(playerIndex);

    public bool CanRefillWater() => 
        Farm.IsWaterTile(currentCell);

    private void ConnectFarmSignals()
    {
        if (NetworkRoot.Instance.IsOnline)
        {
            Connect(SignalName.RequestPrepare,
                new Callable(FarmNetwork, nameof(FarmNetworkService.RequestPrepareTile)));
            Connect(SignalName.RequestPlant,
                new Callable(FarmNetwork, nameof(FarmNetworkService.RequestPlantSeed)));
            Connect(SignalName.RequestRemove,
                new Callable(FarmNetwork, nameof(FarmNetworkService.RequestRemovePlant)));
            Connect(SignalName.RequestIrrigate,
                new Callable(FarmNetwork, nameof(FarmNetworkService.RequestIrrigatePlant)));
            Connect(SignalName.RequestFertilize,
                new Callable(FarmNetwork, nameof(FarmNetworkService.RequestFertilizePlant)));
        }
        else
        {
            Connect(SignalName.RequestIrrigate,
                new Callable(Farm, nameof(FarmManager.TryIrrigatePlant)));
            Connect(SignalName.RequestPrepare,
                new Callable(Farm, nameof(FarmManager.TryPrepareTile)));
            Connect(SignalName.RequestPlant,
                new Callable(Farm, nameof(FarmManager.TryPlantSeed)));
            Connect(SignalName.RequestFertilize,
                new Callable(Farm, nameof(FarmManager.TryFertilizePlant)));
            Connect(SignalName.RequestRemove,
                new Callable(Farm, nameof(FarmManager.TryRemovePlant)));
        }
    }

    private Vector2I GetCellInFrontOfPlayer(Vector2I playerCell)
    {
        Vector2 facing = (Vector2)Player.GetFacingDirection();

        if (facing == Vector2.Zero)
            facing = Vector2.Down;

        Vector2I offset;

        if (Mathf.Abs(facing.X) > Mathf.Abs(facing.Y))
            offset = facing.X > 0 ? Vector2I.Right : Vector2I.Left;
        else
            offset = facing.Y > 0 ? Vector2I.Down : Vector2I.Up;

        return playerCell + offset;
    }
}