using Godot;
using System.Collections.Generic;

public partial class Player : Entity
{
	[Export] public float MoveSpeed = 200f;
	[Export] private PackedScene HotbarScene;
	[Export] private PackedScene TileInteractionScene;

	public PlayerInput Input { get; private set; }
	public PlayerHotbar Hotbar { get; private set; }
	public PlayerTileInteraction Tile { get; private set; }

	public int PlayerId { get; private set; }
	public List<Pickup> PickupsInRange = new();

	// States
	public PlayerIdleState IdleState { get; private set; }
	public PlayerMoveState MoveState { get; private set; }
	public PlayerPlantState PlantState { get; private set; }
	public PlayerPrepareGroundState PrepareGroundState { get; private set; }

	public override void _Ready()
	{
		base._Ready();
		AddToGroup("players");

		GetPlayerSystems();
		PreparePlayerState();
		stateMachine.Initialize(IdleState);
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		Hotbar.HandleInput(Input);
	}

	public override void _PhysicsProcess(double delta)
	{
		MoveAndSlide();
	}

	public void AssignDevice(int deviceId, string deviceType, int playerId)
	{
		PlayerId = playerId;
		Input.AssignDevice(deviceId, deviceType, playerId);
	}

	public Vector2 GetFacingDirection() =>
		Input.FacingDir;

	public Pickup GetPickupNearby()
	{
		Pickup closest = null;
		float minDist = float.MaxValue;

		foreach (var p in PickupsInRange)
		{
			float dist = GlobalPosition.DistanceTo(p.GlobalPosition);
			if (dist < minDist)
			{
				minDist = dist;
				closest = p;
			}
		}

		return closest;
	}

	public bool TryPickupNearby()
	{
		var pickup = GetPickupNearby();
		if (pickup == null) return false;

		if (!Hotbar.CanAddItem(pickup.ItemData))
			return false;

		pickup.Pick(this);
		return true;
	}

	private void GetPlayerSystems() 
	{
		Input = GetNode<PlayerInput>("PlayerInput");

		if (HotbarScene != null)
		{
			Hotbar = HotbarScene.Instantiate<PlayerHotbar>();
			AddChild(Hotbar);
		}

		if (TileInteractionScene != null)
		{
			Tile = TileInteractionScene.Instantiate<PlayerTileInteraction>();
			AddChild(Tile);
		}

		var onHand = GetNode<PlayerOnHand>("PlayerOnHand");
    	onHand?.Initialize(this, Hotbar);
	}

	private void PreparePlayerState()
	{
		IdleState = new PlayerIdleState(this, stateMachine);
		MoveState = new PlayerMoveState(this, stateMachine);
		PlantState = new PlayerPlantState(this, stateMachine);
		PrepareGroundState = new PlayerPrepareGroundState(this, stateMachine);
	}
}
