using Godot;
using System;

[GlobalClass]
public partial class ToolData : ItemData
{
    public event Action<ItemUseContext> OnBeginToolUse;
    public event Action<ItemUseContext> OnEndToolUse;

    public virtual void BeginUse(ItemUseContext ctx)
    {
        OnBeginToolUse?.Invoke(ctx);
    }

    public virtual void EndUse(ItemUseContext ctx)
    {
        OnEndToolUse?.Invoke(ctx);
    }

    public virtual void TickUse(ItemUseContext ctx,double delta)
    {}
}