using Godot;

public partial class NetworkRoot : Node
{
    public static NetworkRoot Instance;

    public SteamworksLoader Loader { get; private set; }
    public SteamConnectionManager Connection { get; private set; }
    public SteamPacketRouter PacketRouter { get; private set; }
    public NetworkTickManager Tick { get; private set; }
    public SteamNetworkManager Steam { get; private set; }

    public override void _Ready()
    {
        Instance = this;

        Loader = GetNode<SteamworksLoader>("SteamworksLoader");
        Connection = GetNode<SteamConnectionManager>("SteamConnectionManager");
        PacketRouter = GetNode<SteamPacketRouter>("SteamPacketRouter");
        Tick = GetNode<NetworkTickManager>("NetworkTickManager");

        Steam = GetNode<SteamNetworkManager>("SteamNetworkManager");
        Steam.Initialize(PacketRouter, Connection);

        GD.Print("[NetworkRoot] Initialized");
    }

    public bool IsNetworkRunning()
    {
        if(Loader == null)
            return false;
        return Loader.IsSteamAvailable;
    }
}