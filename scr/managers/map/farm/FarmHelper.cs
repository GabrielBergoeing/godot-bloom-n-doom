using Godot;
using Godot.Collections;

public class FarmHelper
{
    public const int TERRAIN_SET = 0;

    public const int GRASS = 0;
    public const int CONCRETE = 1;
    public const int WATER = 2;
    public const int DIRT = 3;

    private readonly TileMapLayer tileMap;

    public FarmHelper(TileMapLayer map)
    {
        tileMap = map;
    }

    private static readonly Vector2I[] NeighborOffsets =
    {
        Vector2I.Zero,
        Vector2I.Up,
        Vector2I.Right,
        Vector2I.Down,
        Vector2I.Left,
        Vector2I.Up + Vector2I.Right,
        Vector2I.Up + Vector2I.Left,
        Vector2I.Down + Vector2I.Right,
        Vector2I.Down + Vector2I.Left,
    };

    public bool IsCellTerrain(Vector2I cell, int terrain)
    {
        var data = tileMap.GetCellTileData(cell);

        if (data == null)
            return false;

        return data.GetTerrainSet() == TERRAIN_SET &&
               data.GetTerrain() == terrain;
    }

    public Array<Vector2I> GetRelatedNeighbours(Vector2I cell, int terrain)
    {
        Array<Vector2I> result = new();

        foreach (var offset in NeighborOffsets)
        {
            var neighbor = cell + offset;

            if (IsCellTerrain(neighbor, terrain))
                result.Add(neighbor);
        }

        return result;
    }

    public void UpdateTerrain(Vector2I cell, int terrain)
    {
        var cells = GetRelatedNeighbours(cell, terrain);

        if (!cells.Contains(cell))
            cells.Add(cell);

        tileMap.SetCellsTerrainConnect(cells, TERRAIN_SET, terrain);
    }
}