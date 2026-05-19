using Godot;
using Steamworks;

public partial class SteamNetworkManager : Node
{
    public static SteamNetworkManager Instance;

    public bool IsHost { get; private set; }
    public bool IsConnected => CurrentLobbyId.IsValid();

    public CSteamID CurrentLobbyId { get; private set; }

    private SteamPacketRouter _packetRouter;
    private SteamConnectionManager _connectionManager;

    public override void _Ready()
    {
        Instance = this;

        GD.Print("[SteamNetworkManager] Ready");
    }

    public void Initialize(SteamPacketRouter packetRouter, SteamConnectionManager connectionManager)
    {
        _packetRouter = packetRouter;
        _connectionManager = connectionManager;

        GD.Print("[SteamNetworkManager] Initialized");
    }

    public override void _Process(double delta)
    {
        ReceivePackets();
    }

    public void StartHost()
    {
        IsHost = true;

        GD.Print("[SteamNetworkManager] Hosting session");
    }

    public void JoinLobby(CSteamID lobbyId)
    {
        CurrentLobbyId = lobbyId;

        GD.Print(
            $"[SteamNetworkManager] Joined lobby {lobbyId}"
        );
    }

    public void SendPacket(
        CSteamID target,
        byte[] data,
        EP2PSend sendType = EP2PSend.k_EP2PSendReliable
    )
    {
        bool success =
            SteamNetworking.SendP2PPacket(
                target,
                data,
                (uint)data.Length,
                sendType
            );

        if (!success)
        {
            GD.PrintErr(
                $"[SteamNetworkManager] Failed sending packet to {target}"
            );
        }
    }

    public void SendPingToSelf()
    {
        CSteamID self = SteamUser.GetSteamID();

        byte[] packet =
        {
            NetworkPacketType.Ping
        };

        GD.Print($"[SteamNetworkManager] Sending ping to self: {self}");

        SendPacket(self, packet);
    }

    public void SendChatTest()
    {
        CSteamID self = SteamUser.GetSteamID();

        PacketWriter writer =
            new PacketWriter(
                NetworkPacketType.ChatMessage
            );

        writer.WriteString("Hello from Steam packet!");
        SendPacket(self, writer.ToArray());

        GD.Print("[SteamNetworkManager] Chat packet sent");
    }

    private void ReceivePackets()
    {
        if (_packetRouter == null)
            return;

        uint packetSize;

        while (
            SteamNetworking.IsP2PPacketAvailable(
                out packetSize
            )
        )
        {
            byte[] buffer = new byte[packetSize];

            if (
                SteamNetworking.ReadP2PPacket(
                    buffer,
                    packetSize,
                    out uint bytesRead,
                    out CSteamID remoteId
                )
            )
            {
                _packetRouter.RoutePacket(
                    remoteId,
                    buffer
                );
            }
        }
    }
}