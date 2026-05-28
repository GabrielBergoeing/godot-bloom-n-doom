using Godot;
using System;
using System.Collections.Generic;

public class MatchStartPacket : NetworkPacket
{
    public override byte PacketId => (byte)NetworkPacketType.MatchStart;
    public List<PlayerSpawnData> Players = new();

    public override void Serialize(PacketWriter writer)
    {
        writer.WriteInt(Players.Count);
        foreach (var p in Players)
            p.Serialize(writer);
    }

    public override void Deserialize(PacketReader reader)
    {
        int count = reader.ReadInt();
        for (int i = 0; i < count; i++)
        {
            var p = new PlayerSpawnData();
            p.Deserialize(reader);
            Players.Add(p);
        }
    }
}
