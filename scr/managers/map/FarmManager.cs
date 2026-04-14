using Godot;
using System;
using System.Collections.Generic;

public partial class FarmManager : Node
{
    public static FarmManager Instance;

    [Export] public TileMapLayer FarmLayer;
    [Export] public TileMapLayer WaterLayer;
    [Export] public Node2D PlantsRoot;
    [Export] public PackedScene PlantScene;

    [Export] public int PreparedSourceId;
    [Export] public Vector2I PreparedAtlas;
    [Export] public int SeedSourceId;
    [Export] public Vector2I SeedAtlas;

    private Dictionary<Vector2I, TileState> tileStates = new();
    private Dictionary<Vector2I, Node2D> plantsByCell = new();
    private HashSet<Vector2I> occupiedCells = new();

    private Dictionary<int, Node2D> playerPlantRoots = new();

    public enum TileState
    {
        NotPrepared,
        Prepared,
        PlantedSeed
    }

    public override void _Ready()
    {
        Instance = this;

        if (PlantsRoot == null)
        {
            PlantsRoot = new Node2D();
            PlantsRoot.Name = "Plants";
            AddChild(PlantsRoot);
        }

        InitializeTileStates(true);
    }


    public void InitializeTileStates(bool clearBefore = false)
    {
        if (clearBefore)
            tileStates.Clear();

        var usedCells = FarmLayer.GetUsedCells();

        foreach (var cell in usedCells)
        {
            tileStates[cell] = TileState.NotPrepared;
        }

        GD.Print("Farm tile states initialized");
    }


    public bool IsPrepared(Vector2I cell) =>
        tileStates.TryGetValue(cell, out var state) && state == TileState.Prepared;

    public bool IsOccupied(Vector2I cell) =>
        occupiedCells.Contains(cell);

    public bool HasPlant(Vector2I cell) =>
        plantsByCell.ContainsKey(cell);

    public int? GetPlantOwner(Vector2I cell)
    {
        if (plantsByCell.TryGetValue(cell, out var plant))
        {
            if (plant is Plant p)
                return p.OwnerPlayerIndex;
        }

        return null;
    }

    public void PrepareTile(Vector2I cell)
    {
        if (!tileStates.TryGetValue(cell, out var state) || state != TileState.NotPrepared)
            return;

        FarmLayer.SetCell(cell, PreparedSourceId, PreparedAtlas);
        tileStates[cell] = TileState.Prepared;
    }

    public void PlantSeed(Vector2I cell, int playerIndex)
    {
        if (!tileStates.ContainsKey(cell) || !IsPrepared(cell) || IsOccupied(cell))
            return;

        tileStates[cell] = TileState.PlantedSeed;

        FarmLayer.SetCell(cell, SeedSourceId, SeedAtlas);

        SpawnPlant(cell, playerIndex);
    }

    public bool TryIrrigatePlant(Vector2I cell)
    {
        if (plantsByCell.TryGetValue(cell, out var plant) && plant is Plant p)
        {
            p.WaterPlant();
            return true;
        }
        return false;
    }

    public bool TryFertilizePlant(Vector2I cell)
    {
        if (plantsByCell.TryGetValue(cell, out var plant) && plant is Plant p)
        {
            p.FertilizePlant();
            return true;
        }
        return false;
    }

    private void SpawnPlant(Vector2I cell, int playerIndex)
    {
        Vector2 worldPos = FarmLayer.MapToLocal(cell);

        var plantInstance = PlantScene.Instantiate<Node2D>();
        plantInstance.GlobalPosition = worldPos;

        var parent = GetPlayerPlantRoot(playerIndex);
        parent.AddChild(plantInstance);

        if (plantInstance is Plant p)
            p.Init(playerIndex, cell);

        plantsByCell[cell] = plantInstance;
        occupiedCells.Add(cell);
    }

    public void RemovePlant(Vector2I cell)
    {
        if (!plantsByCell.TryGetValue(cell, out var plant))
            return;

        plant.QueueFree();

        plantsByCell.Remove(cell);
        occupiedCells.Remove(cell);

        FarmLayer.SetCell(cell, PreparedSourceId, PreparedAtlas);
        tileStates[cell] = TileState.Prepared;
    }

    public bool TryRemovePlant(Vector2I cell, int requesterPlayerIndex)
    {
        if (!plantsByCell.TryGetValue(cell, out var plant) || plant is not Plant p)
            return false;

        if (p.OwnerPlayerIndex != requesterPlayerIndex)
        {
            GD.Print("Can't remove someone else's plant.");
            return false;
        }

        RemovePlant(cell);
        return true;
    }

    private Node2D GetPlayerPlantRoot(int playerIndex)
    {
        if (!playerPlantRoots.TryGetValue(playerIndex, out var root) || root == null)
        {
            root = new Node2D();
            root.Name = $"Player{playerIndex}_Plants";
            PlantsRoot.AddChild(root);
            playerPlantRoots[playerIndex] = root;
        }

        return root;
    }

    public bool IsWaterTile(Vector2I cell)
    {
        var data = WaterLayer.GetCellTileData(cell);

        if (data == null)
            return false;

        return data.GetCustomData("type").AsString() == "water";
    }
}