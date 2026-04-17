using Godot;

public enum DeviceType
{
    Keyboard,
    Controller
}

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