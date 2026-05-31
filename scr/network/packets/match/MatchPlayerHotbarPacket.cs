public class MatchPlayerHotbarPacket : NetworkPacket
{
    public override byte PacketId => (byte)NetworkPacketType.MatchPlayerHotbar;
    public ulong OwnerSteamId;
    public int SlotIndex;
    public string ItemId;
    public int Amount;

    public override void Serialize(PacketWriter writer)
    {
        writer.WriteULong(OwnerSteamId);
        writer.WriteInt(SlotIndex);
        writer.WriteString(ItemId);
        writer.WriteInt(Amount);
    }

    public override void Deserialize(PacketReader reader)
    {
        OwnerSteamId = reader.ReadULong();
        SlotIndex = reader.ReadInt();
        ItemId = reader.ReadString();
        Amount = reader.ReadInt();
    }
}
