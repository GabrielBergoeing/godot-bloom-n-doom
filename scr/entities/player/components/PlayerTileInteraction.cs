using Godot;
using System;

public partial class PlayerTileInteraction : Sprite2D
{
    private Camera2D cam;
    private Player Player;
    private FarmManager farmManager;

    private Node2D currentOutline;
    private Vector2I currentCell;
    public Vector2I CurrentCell => currentCell;

    [Signal] public delegate void RequestIrrigateEventHandler(Vector2I cell);
    [Signal] public delegate void RequestFertilizeEventHandler(Vector2I cell);
    [Signal] public delegate void RequestPrepareEventHandler(Vector2I cell);
    [Signal] public delegate void RequestRemoveEventHandler(Vector2I cell, int playerIndex);

    public override void _Ready()
    {
        Player = GetParent<Player>();

        var level = SplitScreenManager.Instance.LevelNode;
        farmManager = level.GetNode<FarmManager>("TileMapLayer");

        var remote = Player.GetNode<RemoteTransform2D>("RemoteTransform2D");

        if (remote != null && !remote.RemotePath.IsEmpty)
            cam = GetNode<Camera2D>(remote.RemotePath);
    }

    public override void _Process(double delta)
    {
        if (!IsInstanceValid(Player))
            return;

        Vector2 playerWorldPos = Player.GlobalPosition;

        Vector2I playerCell = farmManager.LocalToMap(playerWorldPos);
        Vector2I frontCell = GetCellInFrontOfPlayer(playerCell);

        currentCell = frontCell;

        Vector2 cellCenter = farmManager.ToGlobal(farmManager.MapToLocal(currentCell));
        GlobalPosition = cellCenter;
    }

    private Vector2I GetCellInFrontOfPlayer(Vector2I playerCell)
    {
        Vector2 facing = (Vector2)Player.Get("FacingDir");

        if (facing == Vector2.Zero)
            facing = Vector2.Down;

        Vector2I offset;

        if (Mathf.Abs(facing.X) > Mathf.Abs(facing.Y))
            offset = facing.X > 0 ? Vector2I.Right : Vector2I.Left;
        else
            offset = facing.Y > 0 ? Vector2I.Down : Vector2I.Up;

        return playerCell + offset;
    }

    public bool CellIsPrepared() => farmManager.IsPrepared(currentCell);
    public bool CellIsOccupied() => farmManager.IsOccupied(currentCell);

    public bool IsCellOwner(int playerIndex)
        => playerIndex == farmManager.GetPlantOwner(currentCell);

    public bool CanPrepare() => !CellIsPrepared() && !CellIsOccupied();
    public bool CanPlant() => CellIsPrepared() && !CellIsOccupied();
    public bool CanIrrigate() => CellIsOccupied();
    public bool CanRemove(int playerIndex)
        => CellIsOccupied() && IsCellOwner(playerIndex);

    public bool CanSabotage(int playerIndex)
        => CellIsOccupied() && !IsCellOwner(playerIndex);

    public bool CanRefillWater() => farmManager.IsWaterTile(currentCell);

    public void IrrigateInCell()
    {
        EmitSignal(SignalName.RequestIrrigate, currentCell);
    }

    public void PrepareInCell()
    {
        EmitSignal(SignalName.RequestPrepare, currentCell);
    }

    public void FertilizeInCell()
    {
        EmitSignal(SignalName.RequestFertilize, currentCell);
    }

    public void RemoveInCell(int playerIndex)
    {
        EmitSignal(SignalName.RequestRemove, currentCell, playerIndex);
    }
}