using Godot;
using System.Collections.Generic;

public partial class SplitScreenManager : Control
{
	public static SplitScreenManager Instance;

	private List<SubViewportContainer> containers = new();

	public override void _Ready()
	{
		Instance = this;
	}

	public void RegisterPlayer(Node player)
	{
		var container = new SubViewportContainer();
		container.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		container.SizeFlagsVertical = SizeFlags.ExpandFill;

		var viewport = new SubViewport();
		viewport.World2D = GetViewport().World2D;
		Vector2 size = GetViewportRect().Size;
		viewport.Size = new Vector2I((int)size.X, (int)size.Y);

		container.AddChild(viewport);

		player.GetParent().RemoveChild(player);
		viewport.AddChild(player);

		AddChild(container);
		containers.Add(container);

		UpdateLayout();
	}

	private void UpdateLayout()
	{
		int total = containers.Count;

		for (int i = 0; i < total; i++)
		{
			var container = containers[i];

			// Reset anchors
			container.AnchorLeft = 0;
			container.AnchorRight = 1;
			container.AnchorTop = 0;
			container.AnchorBottom = 1;

			if (total == 1)
			{
				container.SetAnchorsPreset(LayoutPreset.FullRect);
			}
			else if (total == 2)
			{
				// Split vertically
				container.AnchorLeft = (i == 0) ? 0 : 0.5f;
				container.AnchorRight = (i == 0) ? 0.5f : 1;
				container.AnchorTop = 0;
				container.AnchorBottom = 1;
			}
			else if (total == 3)
			{
				if (i < 2)
				{
					// Top row
					container.AnchorLeft = i * 0.5f;
					container.AnchorRight = (i + 1) * 0.5f;
					container.AnchorTop = 0;
					container.AnchorBottom = 0.5f;
				}
				else
				{
					// Bottom full
					container.AnchorLeft = 0;
					container.AnchorRight = 1;
					container.AnchorTop = 0.5f;
					container.AnchorBottom = 1;
				}
			}
			else
			{
				container.AnchorLeft = (i % 2) * 0.5f;
				container.AnchorRight = container.AnchorLeft + 0.5f;
				container.AnchorTop = (i < 2) ? 0 : 0.5f;
				container.AnchorBottom = container.AnchorTop + 0.5f;
			}
		}
	}
}
