using Godot;

public partial class DisplayManager : Node
{
    [Export] public bool Fullscreen = true;

    public override void _Ready()
    {
        int screen = DisplayServer.WindowGetCurrentScreen();
        Vector2I size = DisplayServer.ScreenGetSize(screen);

        DisplayServer.WindowSetMode(
            Fullscreen
                ? DisplayServer.WindowMode.Fullscreen
                : DisplayServer.WindowMode.Windowed);

        if (!Fullscreen)
            DisplayServer.WindowSetSize(size);
    }
}