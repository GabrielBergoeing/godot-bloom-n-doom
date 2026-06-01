public class MatchTimerSyncPacket : NetworkPacket
{
    public override byte PacketId => (byte)NetworkPacketType.MatchTimerSync;
    public float TimeRemaining;

    public override void Serialize(PacketWriter writer)
    {
        writer.WriteFloat(TimeRemaining);
    }

    public override void Deserialize(PacketReader reader)
    {
        TimeRemaining = reader.ReadFloat();
    }
}