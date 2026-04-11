using Godot;
using System.Collections.Generic;

// 🔥 Data container (faster than dictionary)
public class InputDeviceData
{
    public int DeviceId;
    public string DeviceType;
    public int PlayerId;

    public InputDeviceData(int deviceId, string type, int playerId)
    {
        DeviceId = deviceId;
        DeviceType = type;
        PlayerId = playerId;
    }
}

public partial class InputDeviceManager : Node
{
    public static InputDeviceManager Instance;
    private List<InputDeviceData> devices = new();

    [Export] public PackedScene PlayerScene;

    [Signal]
    public delegate void InputReceivedEventHandler(int playerId, InputEvent @event);

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouse)
            return;

        int deviceId = @event.Device;
        string type = null;

        if (@event is InputEventKey key && key.Pressed)
        {
            type = "Keyboard";
            deviceId = 0;
        }
        else if (@event is InputEventJoypadButton btn && btn.Pressed)
            type = "Controller";
        else
            return;


        var deviceData = GetDevice(deviceId, type);
        if (deviceData == null)
        {
            RegisterDevice(deviceId, type);
            deviceData = GetDevice(deviceId, type);
        }

        EmitSignal(SignalName.InputReceived, deviceData.PlayerId, @event);
    }

    private InputDeviceData GetDevice(int deviceId, string type)
    {
        foreach (var d in devices)
        {
            if (d.DeviceId == deviceId && d.DeviceType == type)
                return d;
        }
        return null;
    }

    private void RegisterDevice(int deviceId, string type)
    {
        int playerId = devices.Count;

        GD.Print($"Registering {type} → Player {playerId}");

        var data = new InputDeviceData(deviceId, type, playerId);
        devices.Add(data);

        SpawnPlayer(playerId, deviceId, type);
    }

    private void SpawnPlayer(int playerId, int deviceId, string type)
    {
        GD.Print($"Spawning Player {playerId} (Device {deviceId}, {type})");

        var node = PlayerScene.Instantiate();
        var player = node as Player;

        GetTree().Root.AddChild(player);

        player.AssignDevice(deviceId, type, playerId);
    }

    public void ResetDevices()
    {
        devices.Clear();
    }
}