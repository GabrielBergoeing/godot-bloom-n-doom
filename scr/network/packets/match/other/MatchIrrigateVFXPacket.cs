public partial class MatchIrrigateVFXPacket : NetworkPacket
{
	public override byte PacketId => (byte)NetworkPacketType.MatchIrrigateVFX;
	public int PlayerId;
	public ulong OwnerSteamId;

	public override void Serialize(PacketWriter writer)
	{
		writer.WriteInt(PlayerId);
		writer.WriteULong(OwnerSteamId);
	}

	public override void Deserialize(PacketReader reader)
	{
		PlayerId = reader.ReadInt();
		OwnerSteamId = reader.ReadULong();
	}
}
