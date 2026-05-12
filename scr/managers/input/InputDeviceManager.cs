using Godot;
using System.Collections.Generic;

public partial class InputDeviceManager : Node
{
    public static InputDeviceManager Instance;

    public List<LobbyPlayerData> LobbyPlayers { get; private set; } = new();

    [Signal] public delegate void PlayerJoinedEventHandler(LobbyPlayerData player);
    [Signal] public delegate void PlayerLeftEventHandler(LobbyPlayerData player);

    private int _nextPlayerId = 0;

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouse)
            return;

        int deviceId = @event.Device;
        string type = "";

        bool joinPressed = false;

        // Keyboard
        if (@event is InputEventKey key && key.Pressed)
        {
            deviceId = 0;
            type = "Keyboard";

            if (key.Keycode == Key.Enter)
                joinPressed = true;
        }

        // Controller
        if (@event is InputEventJoypadButton btn && btn.Pressed)
        {
            type = "Controller";

            if (btn.ButtonIndex == JoyButton.Start)
                joinPressed = true;
        }

        if (!joinPressed)
            return;

        if (AlreadyRegistered(deviceId, type))
            return;

        RegisterPlayer(deviceId, type);
    }

    private bool AlreadyRegistered(int deviceId, string type)
    {
        foreach (var p in LobbyPlayers)
        {
            if (p.DeviceId == deviceId &&
                p.DeviceType == type)
                return true;
        }

        return false;
    }

    private void RegisterPlayer(int deviceId, string type)
    {
        int playerId = _nextPlayerId++;

        var player = new LobbyPlayerData(
            playerId,
            deviceId,
            type
        );

        LobbyPlayers.Add(player);

        GD.Print($"Player Joined: {playerId}");

        EmitSignal(
            SignalName.PlayerJoined,
            player
        );
    }

    public void RemovePlayer(LobbyPlayerData player)
    {
        if (!LobbyPlayers.Contains(player))
            return;

        LobbyPlayers.Remove(player);

        GD.Print($"Player Left: {player.PlayerId}");

        EmitSignal(
            SignalName.PlayerLeft,
            player
        );
    }

    public void ResetLobby()
    {
        LobbyPlayers.Clear();
    }
}