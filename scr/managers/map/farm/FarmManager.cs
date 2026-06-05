using Godot;
using System.Collections.Generic;

public partial class FarmManager : TileMapLayer
{
    public static FarmManager Instance;

    [Export] public Node2D PlantsRoot;

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

    public Vector2I WorldToCell(Vector2 worldPos) =>
        LocalToMap(worldPos);

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

    public Plant TryGetPlant(Vector2I cell)
    {
        if (plantsByCell.TryGetValue(cell, out var node) && node is Plant plant)
            return plant;

        return null;
    }

    public bool CanPrepareTile(Vector2I cell)
    {
        return !IsPrepared(cell) &&
               !IsOccupied(cell);
    }

    public bool CanPlantSeed(Vector2I cell)
    {
        return IsPrepared(cell) &&
               !IsOccupied(cell);
    }

    public bool CanRemovePlant(Vector2I cell, int requesterPlayerIndex)
    {
        if (!plantsByCell.TryGetValue(cell, out var plant) ||
            plant is not Plant p)
            return false;

        return p.OwnerPlayerIndex == requesterPlayerIndex;
    }

    public bool CanIrrigatePlant(Vector2I cell)
    {
        return TryGetPlant(cell) != null;
    }

    public bool CanFertilizePlant(Vector2I cell)
    {
        return TryGetPlant(cell) != null;
    }

    public void PrepareTile(Vector2I cell)
    {
        helper.UpdateTerrain(cell, FarmHelper.DIRT);
    }

    public void PlantSeed(Vector2I cell, int playerIndex, SeedData data)
    {
        Vector2 worldPos = MapToLocal(cell);

        var plantInstance = data.PlantScene.Instantiate<Plant>();
        plantInstance.GlobalPosition = worldPos;

        var parent = GetPlayerPlantRoot(playerIndex);
        parent.AddChild(plantInstance);

        if (plantInstance is Plant p)
            p.Init(playerIndex, cell, data);

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

        helper.UpdateTerrain(cell, FarmHelper.DIRT);
    }

    public void IrrigatePlant(Vector2I cell)
    {
        if (TryGetPlant(cell) is Plant plant)
            plant.WaterPlant();
    }

    public void FertilizePlant(Vector2I cell)
    {
        if (TryGetPlant(cell) is Plant plant)
            plant.FertilizePlant();
    }

    public bool TryPrepareTile(Vector2I cell)
    {
        if (!CanPrepareTile(cell))
            return false;

        PrepareTile(cell);
        return true;
    }

    public bool TryPlantSeed(Vector2I cell, int playerIndex, SeedData data)
    {
        if (!CanPlantSeed(cell))
            return false;

        PlantSeed(cell, playerIndex, data);
        return true;
    }

    public bool TryRemovePlant(Vector2I cell, int requesterPlayerIndex)
    {
        if (!CanRemovePlant(cell, requesterPlayerIndex))
        {
            GD.Print("Can't remove someone else's plant.");
            return false;
        }

        RemovePlant(cell);
        return true;
    }

    public bool TryIrrigatePlant(Vector2I cell)
    {
        if (!CanIrrigatePlant(cell))
            return false;

        IrrigatePlant(cell);
        return true;
    }

    public bool TryFertilizePlant(Vector2I cell)
    {
        if (!CanFertilizePlant(cell))
            return false;

        FertilizePlant(cell);
        return true;
    }

    public List<Plant> GetAdjacentPlants(Vector2I cell)
    {
        List<Plant> plants = new();

        Vector2I[] offsets =
        {
            Vector2I.Up,
            Vector2I.Down,
            Vector2I.Left,
            Vector2I.Right
        };

        foreach (var offset in offsets)
        {
            Plant neighbor = TryGetPlant(cell + offset);

            if (neighbor == null)
                continue;

            plants.Add(neighbor);
        }

        return plants;
    }

    public Dictionary<int, int> GetAllPlantScores()
    {
        Dictionary<int, int> scores = new();

        foreach (Node2D node in plantsByCell.Values)
        {
            if (node is not Plant plant) continue;
            if (!GodotObject.IsInstanceValid(plant)) continue;
            if (plant.IsQueuedForDeletion()) continue;

            int owner = plant.OwnerPlayerIndex;

            scores.TryAdd(owner, 0);
            scores[owner] += plant.GetScore();
        }

        return scores;
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