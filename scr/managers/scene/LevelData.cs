using Godot;
using Godot.Collections;

[GlobalClass]
public partial class LevelData : Resource
{
    [Export] public PackedScene LevelScene;

    [ExportGroup("Level Settings")]
    [Export(PropertyHint.Range, "1, 300, or_greater")] public float MatchDuration;
    [Export] public string BGMTrack;

    [ExportGroup("Level Spawn Tables")]
    [Export] public Array<SpawnEntry> SeedTable = new();
    [Export] public Array<SpawnEntry> ToolTable = new();
    [Export] public Array<SpawnEntry> RareTable = new();

    [ExportGroup("Level Spawn Intervals")]
	[Export] public float SeedInterval = 8f;
	[Export] public float ToolInterval = 15f;
	[Export] public float RareInterval = 50f;
	[Export] public int SpawnAttempts = 50;
}
