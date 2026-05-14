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

    public void TryPrepareTile(Vector2I cell)
    {
        if (IsPrepared(cell) || IsOccupied(cell))
            return;

        helper.UpdateTerrain(cell, FarmHelper.DIRT);
    }

    public bool TryPlantSeed(Vector2I cell, int playerIndex, SeedData data)
    {
        if (!IsPrepared(cell) || IsOccupied(cell))
            return false;

        PlantSeed(cell, playerIndex, data);
        return true;
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

    public List<Plant> GetAdjacentPlants(Vector2I cell)
    {
        List<Plant> plants = new();
        Vector2I[] offsets = {Vector2I.Up, Vector2I.Down, Vector2I.Left, Vector2I.Right};

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
        foreach (Node2D playerRoot in playerPlantRoots.Values)
        {
            foreach (Node child in playerRoot.GetChildren())
            {
                if (child is not Plant plant)
                    continue;

                int score = plant.GetScore();
                int playerId = plant.OwnerPlayerIndex;
                
                if (!scores.ContainsKey(playerId))
                    scores[playerId] = 0;
                scores[playerId] += score;
            }
        }
        return scores;
    }

    private void PlantSeed(Vector2I cell, int playerIndex, SeedData data)
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