using Godot;
using Steamworks;

public partial class SteamNetworkManager : Node
{
    public static SteamNetworkManager Instance;
    public NetworkRoot Network => NetworkRoot.Instance;

    private SteamPacketRouter _packetRouter;

    public override void _Ready()
    {
        Instance = this;
        GD.Print("[SteamNetworkManager] Ready");
    }

    public void Initialize(SteamPacketRouter router)
    {
        _packetRouter = router;
    }

    public override void _Process(double delta)
    {
        if (!Network.IsNetworkRunning())
            return;

        ReceivePackets();
    }

    public void SendPacket(CSteamID target, NetworkPacket packet, EP2PSend sendType = EP2PSend.k_EP2PSendReliable)
    {
        byte[] data = packet.ToArray();
        bool success = SteamNetworking.SendP2PPacket(
            target,
            data,
            (uint)data.Length,
            sendType
        );

        if (!success)
            GD.PrintErr($"Failed sending packet");
    }

    private void ReceivePackets()
    {
        if (_packetRouter == null)
            return;

        uint packetSize;
        GD.Print("Polling packets");

        while (SteamNetworking.IsP2PPacketAvailable(
            out packetSize
        ))
        {
            byte[] buffer = new byte[packetSize];

            if (SteamNetworking.ReadP2PPacket(
                buffer,
                packetSize,
                out uint bytesRead,
                out CSteamID remoteId
            ))
                _packetRouter.RoutePacket(remoteId, buffer);
        }
    }
}