using Godot;

public partial class LobbyPlayerData : Resource
{
    public int PlayerId;
    public int DeviceId;
    public string DeviceType;

    public ulong SteamId;

    public CharacterData SelectedCharacter;
    public bool LockedIn = false;

    public LobbyPlayerData(int playerId, int deviceId, string deviceType, ulong steamId = 0)
    {
        PlayerId = playerId;
        DeviceId = deviceId;
        DeviceType = deviceType;
        SteamId = steamId;
    }
}