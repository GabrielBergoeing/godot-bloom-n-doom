using System.Diagnostics;
using Godot;

public partial class PlayerPlantState : PlayerActionState
{
	public PlayerPlantState(Player player, StateMachine sm)
		: base(player, sm) {}

	public override void Enter()
	{
		base.Enter();

		GD.Print("Enters State");

		var item = Inventory.GetCurrentStack().Data;
		GD.Print("Gets Stack");

		_ = ExecuteAction(
			duration: 2f,
			cooldown: 0.5f,
			applyAction: (cell) =>
			{
					item.Use(new ItemUseContext
					{
						Player = player,
						Cell = cell
					});
			}
		);

		GD.Print("Execute Action");
	}
}