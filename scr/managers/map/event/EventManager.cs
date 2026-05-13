using Godot;
using Godot.Collections;

public partial class EventManager : Node
{
	public GameManager Game => GameManager.Instance;
	public FarmManager Farm => FarmManager.Instance;

	private LevelData _data;

	private double seedTimer;
	private double toolTimer;
	private double rareTimer;

	public override void _Ready()
	{
		_data = Game.CurrentLevel;
	}

	public override void _Process(double delta)
	{
		seedTimer += delta;
		toolTimer += delta;
		rareTimer += delta;

		if (seedTimer >= _data.SeedInterval)
		{
			SpawnRandomItem(_data.SeedTable);
			seedTimer = 0;
		}

		if (toolTimer >= _data.ToolInterval)
		{
			SpawnRandomItem(_data.ToolTable);
			toolTimer = 0;
		}

		if (rareTimer >= _data.RareInterval)
		{
			SpawnRandomItem(_data.RareTable);
			rareTimer = 0;
		}
	}

	private void SpawnRandomItem(Array<SpawnEntry> table)
	{
		if (table.Count == 0)
			return;

		Vector2? spawnPos = GetRandomFreePosition();
		if (spawnPos == null)
			return;

		ItemData item = GetWeightedItem(table);
		if (item == null || item.PickupScene == null)
			return;

		var pickup =
			item.PickupScene.Instantiate<Pickup>();
		pickup.SetItemData(item);

		var level =
			SplitScreenManager.Instance.LevelNode;

		if (level == null)
			return;

		level.AddChild(pickup);
		pickup.Position = spawnPos.Value;
	}

	private ItemData GetWeightedItem(Array<SpawnEntry> table)
	{
		int totalWeight = 0;

		foreach (var entry in table)
			totalWeight += Mathf.Max(entry.Weight, 0);

		if (totalWeight <= 0)
			return null;

		int roll = GD.RandRange(0, totalWeight - 1);

		int cumulative = 0;

		foreach (var entry in table)
		{
			cumulative += entry.Weight;

			if (roll < cumulative)
				return entry.Item;
		}

		return null;
	}

	private Vector2? GetRandomFreePosition()
	{
		if (Farm == null)
			return null;

		Rect2I bounds = Farm.GetUsedRect();

		for (int i = 0; i < _data.SpawnAttempts; i++)
		{
			int x = GD.RandRange(
				bounds.Position.X,
				bounds.End.X - 1
			);

			int y = GD.RandRange(
				bounds.Position.Y,
				bounds.End.Y - 1
			);

			Vector2I cell = new Vector2I(x, y);

			if (!CanSpawnAt(cell))
				continue;

			Vector2 worldPos =
				Farm.ToGlobal(
					Farm.MapToLocal(cell)
				);

			if (IsPositionFree(worldPos))
				return worldPos;
		}

		return null;
	}

	private bool CanSpawnAt(Vector2I cell)
	{
		if (Farm.IsWaterTile(cell))
			return false;

		if (Farm.IsOccupied(cell))
			return false;
		return true;
	}

	private bool IsPositionFree(Vector2 pos)
	{
		var space = GetViewport().World2D.DirectSpaceState;

		var query = new PhysicsPointQueryParameters2D
		{
			Position = pos,
			CollideWithAreas = true,
			CollideWithBodies = true
		};

		var results = space.IntersectPoint(query);

		foreach (var result in results)
		{
			var collider = result["collider"].AsGodotObject();

			if (collider is Pickup)
				return false;

			if (collider is Player)
				return false;

			if (collider is Plant)
				return false;
		}

		return true;
	}
}
