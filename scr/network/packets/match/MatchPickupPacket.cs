using Godot;

public class MatchPickupPacket : NetworkPacket
{
    public override byte PacketId => (byte)NetworkPacketType.MatchPickupSpawned;
    public int ItemId;
    public Vector2 Position;
    public int NetworkPickupId; // unique id to track across machines

    public override void Serialize(PacketWriter writer)
    {
        writer.WriteInt(ItemId);
        writer.WriteVector2(Position);
        writer.WriteInt(NetworkPickupId);
    }

    public override void Deserialize(PacketReader reader)
    {
        ItemId = reader.ReadInt();
        Position = reader.ReadVector2();
        NetworkPickupId = reader.ReadInt();
    }
}