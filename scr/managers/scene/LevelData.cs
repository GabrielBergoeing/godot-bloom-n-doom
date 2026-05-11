using Godot;
using Godot.Collections;

[GlobalClass]
public partial class LevelData : Resource
{
    [Export] public PackedScene LevelScene;

    [ExportGroup("Level Settings")]
    [Export(PropertyHint.Range, "1, 10, or_greater")] public float MatchDuration;
    [Export] public string BGMTrack;

    [ExportGroup("Level Item List")]
    [Export] public Array<SpawnEntry> SeedTable = new();
    [Export] public Array<SpawnEntry> ToolTable = new();
    [Export] public Array<SpawnEntry> RareTable = new();
}
