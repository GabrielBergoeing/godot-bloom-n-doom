public class MatchPickupCollectRequestPacket : NetworkPacket
{
    public override byte PacketId => 
        (byte)NetworkPacketType.MatchPickupCollectRequest;
    public int NetworkPickupId;
    public ulong RequesterSteamId;

    public override void Serialize(PacketWriter writer)
    {
        writer.WriteInt(NetworkPickupId);
        writer.WriteULong(RequesterSteamId);
    }

    public override void Deserialize(PacketReader reader)
    {
        NetworkPickupId = reader.ReadInt();
        RequesterSteamId = reader.ReadULong();
    }
}