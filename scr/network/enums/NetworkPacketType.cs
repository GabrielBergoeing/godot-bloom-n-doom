public enum NetworkPacketType : byte
{
    Ping = 1,
    ChatMessage = 2,

    LobbyPlayerState = 10,
    LobbyPlayerJoined = 11,
    LobbyPlayerLeft = 12,
    LobbyStartGame = 13
}