using Godot;

public class MatchPickupSpawnRequestPacket : NetworkPacket
{
    public override byte PacketId => (byte)NetworkPacketType.MatchPickupSpawnRequest;
    public ulong RequesterSteamId;
    public string ItemId;
    public Vector2 Position;

    public override void Serialize(PacketWriter writer)
    {
        writer.WriteULong(RequesterSteamId);
        writer.WriteString(ItemId);
        writer.WriteVector2(Position);
    }

    public override void Deserialize(PacketReader reader)
    {
        RequesterSteamId = reader.ReadULong();
        ItemId = reader.ReadString();
        Position = reader.ReadVector2();
    }
}