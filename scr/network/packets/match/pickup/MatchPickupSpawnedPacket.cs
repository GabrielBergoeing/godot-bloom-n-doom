using Godot;

public class MatchPickupSpawnedPacket : NetworkPacket
{
    public override byte PacketId => (byte)NetworkPacketType.MatchPickupSpawned;
    public string ItemId;
    public Vector2 Position;
    public int NetworkPickupId;

    public override void Serialize(PacketWriter writer)
    {
        writer.WriteString(ItemId);
        writer.WriteVector2(Position);
        writer.WriteInt(NetworkPickupId);
    }

    public override void Deserialize(PacketReader reader)
    {
        ItemId = reader.ReadString();
        Position = reader.ReadVector2();
        NetworkPickupId = reader.ReadInt();
    }
}