using Godot;

public partial class NetworkRoot : Node
{
    public static NetworkRoot Instance;

    public SteamworksLoader Loader { get; private set; }
    public SteamConnectionManager Connection { get; private set; }
    public SteamPacketRouter PacketRouter { get; private set; }
    public NetworkTickManager Tick { get; private set; }
    public SteamNetworkManager Steam { get; private set; }
    public SteamLobbyManager Lobby { get; private set; }

    public bool IsOnline { get; private set; } = false;

    public override void _Ready()
    {
        Instance = this;

        Loader = GetNode<SteamworksLoader>("SteamworksLoader");
        Connection = GetNode<SteamConnectionManager>("SteamConnectionManager");
        PacketRouter = GetNode<SteamPacketRouter>("SteamPacketRouter");
        Tick = GetNode<NetworkTickManager>("NetworkTickManager");

        Steam = GetNode<SteamNetworkManager>("SteamNetworkManager");
        Steam.Initialize(PacketRouter);

        Lobby = GetNode<SteamLobbyManager>("SteamLobbyManager");
        Lobby.Initialize(PacketRouter);

        GD.Print("[NetworkRoot] Initialized");
    }

    public bool IsNetworkRunning()
    {
        if(Loader == null)
            return false;
        return Loader.IsSteamAvailable;
    }

    public ulong GetSteamID()
    {
        if(!IsNetworkRunning())
            return 0;
        return Lobby.LocalSteamId;
    }

    public void SetOnlineMode(bool mode)
    {
        if(IsNetworkRunning())
            IsOnline = mode;
    }
}