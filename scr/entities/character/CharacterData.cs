using Godot;

[GlobalClass]
public partial class CharacterData : Resource
{
    [ExportGroup("Character Details")]
    [Export] public string CharacterName = "";
    [Export] public Texture2D Illustration;
    [Export] public SpriteFrames Sprites;

    [ExportGroup("UI Colors")]
    [Export] public Color EmptyColor = Colors.DarkGray;
    [Export] public Color ActiveColor = Colors.White;
    [Export] public Color LockedColor = Colors.Gold;
}