using Godot;

[GlobalClass]
public partial class SpawnEntry : Resource
{
    [Export] public ItemData Item;
    [Export] public int Weight = 1;
}