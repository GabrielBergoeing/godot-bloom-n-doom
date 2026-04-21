using System.Diagnostics;
using Godot;

public partial class PlayerPlantState : PlayerActionState
{
	public PlayerPlantState(Player player, StateMachine sm)
		: base(player, sm) {}

	public override void Enter()
	{
		base.Enter();
		var item = Inventory.GetCurrentStack().Data;

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
	}
}