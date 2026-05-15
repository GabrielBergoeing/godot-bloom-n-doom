using Godot;
using System;

public partial class PlayerSFX : AudioStreamPlayer
{
	private AudioManager Audio => AudioManager.Instance;
	private FarmManager Farm => FarmManager.Instance;
	private Node2D _pos;
	
	[Export] private string _prefix = "sfx_player_";
	[Export] private string _walk = "walk";
	[Export] private string _walk_grass = "walk_grass";
	[Export] private string _pickup = "pickup";
	[Export] private string _drop = "drop";
	[Export] private string _prepare = "prepare";
	[Export] private string _refill = "refill";
	[Export] private string _water = "water";

	public override void _Ready()
	{
		_pos = GetParent<Node2D>();
	}

	public void PlayOnWalk()
	{
		Vector2I cell = Farm.WorldToCell(_pos.GlobalPosition);
		if(Farm.IsGrass(cell) || Farm.IsPrepared(cell))
			PlaySound(_prefix + _walk_grass);
		else
			PlaySound(_prefix + _walk);
	}

	public void PlayOnPickup() => 
		PlaySound(_prefix + _pickup);

	public void PlayOnDrop() => 
		PlaySound(_prefix + _drop);

	public void PlayOnPrepare() => 
		PlaySound(_prefix + _prepare);

	public void PlayOnRefill() => 
		PlaySound(_prefix + _refill);

	public void PlayOnWater() => 
		PlaySound(_prefix + _water);

	public void PlaySound(string soundName) =>
		Audio.PlaySFX(soundName, this);
}
