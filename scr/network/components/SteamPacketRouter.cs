using Godot;
using Steamworks;
using System;
using System.Collections.Generic;

public partial class SteamPacketRouter : Node
{
    private readonly Dictionary<
        byte,
        Action<CSteamID, byte[]>
    > _handlers = new();

    public override void _Ready()
    {
        RegisterHandlers();

        GD.Print("[SteamPacketRouter] Ready");
    }

    private void RegisterHandlers()
    {
        RegisterHandler(
            NetworkPacketType.Ping,
            HandlePing
        );
    }

    public void RegisterHandler(
        byte packetId,
        Action<CSteamID, byte[]> handler
    )
    {
        _handlers[packetId] = handler;
    }

    public void RoutePacket(
        CSteamID sender,
        byte[] data
    )
    {
        if (data.Length == 0)
            return;

        byte packetId = data[0];

        if (!_handlers.TryGetValue(
            packetId,
            out var handler
        ))
        {
            GD.PrintErr(
                $"[SteamPacketRouter] No handler for packet {packetId}"
            );

            return;
        }

        handler.Invoke(sender, data);
    }

    private void HandlePing(
        CSteamID sender,
        byte[] data
    )
    {
        GD.Print(
            $"[SteamPacketRouter] Ping received from {sender}"
        );
    }
}