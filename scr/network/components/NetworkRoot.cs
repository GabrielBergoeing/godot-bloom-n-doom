using Godot;

public partial class NetworkRoot : Node
{
    public static NetworkRoot Instance;

    public SteamworksLoader SteamworksLoader { get; private set; }
    public SteamConnectionManager SteamConnectionManager { get; private set; }
    public SteamPacketRouter SteamPacketRouter { get; private set; }
    public NetworkTickManager NetworkTickManager { get; private set; }
    public SteamNetworkManager SteamNetworkManager { get; private set; }

    public override void _Ready()
    {
        Instance = this;

        SteamworksLoader = GetNode<SteamworksLoader>("SteamworksLoader");
        SteamConnectionManager = GetNode<SteamConnectionManager>("SteamConnectionManager");
        SteamPacketRouter = GetNode<SteamPacketRouter>("SteamPacketRouter");
        NetworkTickManager = GetNode<NetworkTickManager>("NetworkTickManager");

        SteamNetworkManager = GetNode<SteamNetworkManager>("SteamNetworkManager");
        SteamNetworkManager.Initialize(SteamPacketRouter, SteamConnectionManager);

        GD.Print("[NetworkRoot] Initialized");
    }
}