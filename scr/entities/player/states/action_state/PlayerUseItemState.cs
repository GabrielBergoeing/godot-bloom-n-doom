using System.Diagnostics;
using Godot;

public partial class PlayerUseItemState : PlayerActionState
{
	public PlayerUseItemState(Player player, StateMachine sm): base(player, sm) 
	{}

	public override void Enter()
	{
		base.Enter();
		var item = Inventory.GetCurrentStack().Data;

		_ = ExecuteAction(
			item.Duration,
			item.Cooldown,
			() =>
			{
				var ctx = new ItemUseContext(player, tile);
				SFX.PlaySound(item.SoundName);
				item.Use(ctx);
			}
		);
	}
}