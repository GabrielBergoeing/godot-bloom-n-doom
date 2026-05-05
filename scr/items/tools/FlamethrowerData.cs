using Godot;
using System;

[GlobalClass]
public partial class FlamethrowerData : ItemData
{
    [Export] public PackedScene FirePrefab;

    public override bool CanUse(ItemUseContext ctx)
    {
        return true;
    }

    public override void Use(ItemUseContext ctx)
    {
        // optional: single burst
    }

    public override void StartUse(ItemUseContext ctx)
    {
        // start firing loop (handled by state)
    }

    public override void StopUse(ItemUseContext ctx)
    {
        // stop firing
    }
}