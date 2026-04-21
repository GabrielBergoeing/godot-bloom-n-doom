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
			item.Duration,
			item.Cooldown,
			(cell) =>
			{
				var ctx = new ItemUseContext(player, cell);
				item.Use(ctx);
			}
		);
	}
}