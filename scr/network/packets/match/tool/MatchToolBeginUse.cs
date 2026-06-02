public partial class MatchToolBeginUse : NetworkPacket
{
	public override byte PacketId => (byte)NetworkPacketType.MatchToolBeginUse;
	public int PlayerId;
	public int ItemSlot;
	public ulong OwnerSteamId;

	public override void Serialize(PacketWriter writer)
	{
		writer.WriteInt(PlayerId);
		writer.WriteInt(ItemSlot);
		writer.WriteULong(OwnerSteamId);
	}

	public override void Deserialize(PacketReader reader)
	{
		PlayerId = reader.ReadInt();
		ItemSlot = reader.ReadInt();
		OwnerSteamId = reader.ReadULong();
	}
}
