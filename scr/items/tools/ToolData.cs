using Godot;

[GlobalClass]
public partial class ToolData : ItemData
{
    public virtual void BeginUse(ItemUseContext ctx) {}
    public virtual void EndUse(ItemUseContext ctx) {}

    public virtual void TickUse(
        ItemUseContext ctx,
        double delta)
    {}
}