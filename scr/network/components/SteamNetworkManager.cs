using Godot;
using System;
using Steamworks;

public partial class SteamNetworkManager : Node
{
    public static SteamNetworkManager Instance;

    public event Action<CSteamID> OnPeerSessionEstablished;

    public NetworkRoot Network => NetworkRoot.Instance;

    private SteamPacketRouter _packetRouter;

    private Callback<P2PSessionRequest_t> _p2pSessionRequest;
    private Callback<P2PSessionConnectFail_t> _p2pConnectFail;

    public override void _Ready()
    {
        Instance = this;
        GD.Print("[SteamNetworkManager] Ready");
    }

    public void Initialize(SteamPacketRouter router)
    {
        _packetRouter = router;

        _p2pSessionRequest = Callback<P2PSessionRequest_t>.Create(
            OnP2PSessionRequest
        );

        _p2pConnectFail = Callback<P2PSessionConnectFail_t>.Create(
            OnP2PConnectFail
        );
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

    private void OnP2PSessionRequest(P2PSessionRequest_t callback)
    {
        GD.Print($"Accepted P2P session from {callback.m_steamIDRemote}");
        SteamNetworking.AcceptP2PSessionWithUser(callback.m_steamIDRemote);
        OnPeerSessionEstablished?.Invoke(callback.m_steamIDRemote);
    }

    private void OnP2PConnectFail(P2PSessionConnectFail_t callback)
    {
        GD.PrintErr($"P2P connect failed: {callback.m_eP2PSessionError}");
    }

    private void ReceivePackets()
    {
        if (_packetRouter == null)
            return;

        uint packetSize;

        while (SteamNetworking.IsP2PPacketAvailable(
            out packetSize
        ))
        {
            GD.Print($"[SteamNetworkManager] Packet available: {packetSize}");
            byte[] buffer = new byte[packetSize];

            if (SteamNetworking.ReadP2PPacket(
                buffer,
                packetSize,
                out uint bytesRead,
                out CSteamID remoteId
            ))

            GD.Print($"[SteamNetworkManager] Received {bytesRead} bytes from {remoteId}");
                _packetRouter.RoutePacket(remoteId, buffer);
        }
    }
}