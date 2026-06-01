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
    MatchEnd = 25,

    MatchPlayerHotbar = 26,
    MatchPickupSpawned = 27,
    MatchPickupCollected = 28,
    MatchPickupSpawnRequest = 29,
    MatchPickupCollectRequest = 30,

    MatchFarmPrepare = 31,
    MatchFarmPlant = 32,
    MatchFarmRemove = 33,
    MatchFarmIrrigate = 34,
    MatchFarmFertilize = 35,
}