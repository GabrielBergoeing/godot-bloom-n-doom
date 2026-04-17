using Godot;
using System.Collections.Generic;

public partial class FarmManager : TileMapLayer
{
    public static FarmManager Instance;

    [Export] public TileMapLayer WaterLayer;
    [Export] public Node2D PlantsRoot;
    [Export] public PackedScene PlantScene;

    [Export] public int SeedSourceId;
    [Export] public Vector2I SeedAtlas;

    private Dictionary<Vector2I, Node2D> plantsByCell = new();
    private HashSet<Vector2I> occupiedCells = new();
    private Dictionary<int, Node2D> playerPlantRoots = new();

    private FarmHelper helper;

    public override void _Ready()
    {
        Instance = this;

        helper = new FarmHelper(this);

        if (PlantsRoot == null)
        {
            PlantsRoot = new Node2D();
            PlantsRoot.Name = "Plants";
            AddChild(PlantsRoot);
        }
    }
    
    public bool IsGrass(Vector2I cell) =>
        helper.IsCellTerrain(cell, FarmHelper.GRASS);

    public bool IsPrepared(Vector2I cell) =>
        helper.IsCellTerrain(cell, FarmHelper.DIRT);

    public bool IsOccupied(Vector2I cell) =>
        occupiedCells.Contains(cell);
    
    public bool IsWaterTile(Vector2I cell) =>
        helper.IsCellTerrain(cell, FarmHelper.WATER);

    public bool HasPlant(Vector2I cell) =>
        plantsByCell.ContainsKey(cell);

    public int? GetPlantOwner(Vector2I cell)
    {
        if (plantsByCell.TryGetValue(cell, out var plant) && plant is Plant p)
            return p.OwnerPlayerIndex;

        return null;
    }

    public void TryPrepareTile(Vector2I cell)
    {
        if (IsPrepared(cell) || IsOccupied(cell))
            return;

        helper.UpdateTerrain(cell, FarmHelper.DIRT);

        GD.Print($"Prepared terrain at {cell}");
    }

    public void PlantSeed(Vector2I cell, int playerIndex)
    {
        if (!IsPrepared(cell) || IsOccupied(cell))
            return;

        SetCell(cell, SeedSourceId, SeedAtlas);
        SpawnPlant(cell, playerIndex);
    }

    public void RemovePlant(Vector2I cell)
    {
        if (!plantsByCell.TryGetValue(cell, out var plant))
            return;

        plant.QueueFree();

        plantsByCell.Remove(cell);
        occupiedCells.Remove(cell);

        helper.UpdateTerrain(cell, FarmHelper.DIRT);
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
        Vector2 worldPos = MapToLocal(cell);

        var plantInstance = PlantScene.Instantiate<Node2D>();
        plantInstance.GlobalPosition = worldPos;

        var parent = GetPlayerPlantRoot(playerIndex);
        parent.AddChild(plantInstance);

        if (plantInstance is Plant p)
            p.Init(playerIndex, cell);

        plantsByCell[cell] = plantInstance;
        occupiedCells.Add(cell);
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
}