public class MatchPickupCollectedPacket : NetworkPacket
{
    public override byte PacketId => (byte)NetworkPacketType.MatchPickupCollected;
    public int NetworkPickupId;

    public override void Serialize(PacketWriter writer)
    {
        writer.WriteInt(NetworkPickupId);
    }

    public override void Deserialize(PacketReader reader)
    {
        NetworkPickupId = reader.ReadInt();
    }
}