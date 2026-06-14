using Godot;
using System;
using Steamworks;
using System.Collections.Generic;

public partial class SteamNetworkManager : Node
{
    public static SteamNetworkManager Instance;

    public event Action<CSteamID> OnPeerSessionEstablished;

    public NetworkRoot Network => NetworkRoot.Instance;

    private SteamPacketRouter _packetRouter;

    private Callback<P2PSessionRequest_t> _p2pSessionRequest;
    private Callback<P2PSessionConnectFail_t> _p2pConnectFail;

    private readonly Dictionary<ulong, float> _pingSentAt = new();
    private readonly Dictionary<ulong, float> _peerLatencyMs = new();
    private float _pingInterval = 2f;
    private float _pingTimer = 0f;

    /// <summary>Average RTT in ms across all connected peers. Returns -1 if no data.</summary>
    public float AverageLatencyMs
    {
        get
        {
            if (_peerLatencyMs.Count == 0) return -1f;
            float sum = 0f;
            foreach (var v in _peerLatencyMs.Values) sum += v;
            return sum / _peerLatencyMs.Count;
        }
    }


    public override void _Ready()
    {
        Instance = this;
        GD.Print("[SteamNetworkManager] Ready");
    }

    public void Initialize(SteamPacketRouter router)
    {
        _packetRouter = router;
        _p2pSessionRequest = Callback<P2PSessionRequest_t>.Create(OnP2PSessionRequest);
        _p2pConnectFail = Callback<P2PSessionConnectFail_t>.Create(OnP2PConnectFail);

        router.OnPing += HandlePingRequest;
        router.OnPong += HandlePongRequest;
    }

    public override void _Process(double delta)
    {
        if (!Network.IsNetworkRunning())
            return;

        ReceivePackets();
        UpdatePingCycle(delta);
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

    private void HandlePingRequest(CSteamID sender, byte[] data)
    {
        SendPacket(sender, new PongPacket(), EP2PSend.k_EP2PSendUnreliable);
    }

    private void UpdatePingCycle(double delta)
    {
        // Only run if there are peers to ping
        var peers = Network.Connection.GetAllPeers();
        if (peers == null) return;

        _pingTimer -= (float)delta;
        if (_pingTimer > 0f) return;
        _pingTimer = _pingInterval;

        float now = Time.GetTicksMsec() / 1000f;
        foreach (var peer in peers)
        {
            _pingSentAt[peer.m_SteamID] = now;
            SendPacket(peer, new PingPacket(), EP2PSend.k_EP2PSendUnreliable);
        }
    }

    private void HandlePongRequest(CSteamID sender, byte[] data)
    {
        float now = Time.GetTicksMsec() / 1000f;
        if (!_pingSentAt.TryGetValue(sender.m_SteamID, out float sentAt))
            return;

        float rttMs = (now - sentAt) * 1000f;
        _peerLatencyMs[sender.m_SteamID] = rttMs;
    }
}