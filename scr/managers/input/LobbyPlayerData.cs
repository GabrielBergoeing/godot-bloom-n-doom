using Godot;

public partial class LobbyPlayerData : Resource
{
    public int PlayerId;
    public int DeviceId;
    public string DeviceType;

    public CharacterData SelectedCharacter;

    public bool LockedIn = false;

    public LobbyPlayerData(int playerId, int deviceId, string deviceType)
    {
        PlayerId = playerId;
        DeviceId = deviceId;
        DeviceType = deviceType;
    }
}