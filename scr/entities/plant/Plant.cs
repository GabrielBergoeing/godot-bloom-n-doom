using Godot;
using System;

public partial class Plant : Node2D
{
	public int OwnerPlayerIndex = -1;
	public Vector2I cellPos;

	public void Init(int playerIndex, Vector2I cell)
	{}

	public void WaterPlant()
	{}

	public void FertilizePlant()
	{}
}
