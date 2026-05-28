public enum NetworkPacketType : byte
{
    Ping = 1,
    ChatMessage = 2,

    LobbyPlayerState = 10,
    LobbyPlayerJoined = 11,
    LobbyPlayerLeft = 12,
    LobbyStartGame = 13,

    MatchStart = 20,
    MatchPlayerSpawn = 21,
    MatchPlayerInput = 22,
    MatchPlayerTransform = 23,
    MatchTimerSync = 24,
    MatchEnd = 25
}