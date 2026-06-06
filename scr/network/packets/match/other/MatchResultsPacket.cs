public class MatchResultsPacket : NetworkPacket
{
    public override byte PacketId => (byte)NetworkPacketType.MatchResults;
    public int WinnerPlayerIndex;
    public int WinnerScore;
    public string WinnerUsername;
    public int WinnerCharacterIndex;

    public override void Serialize(PacketWriter writer)
    {
        writer.WriteInt(WinnerPlayerIndex);
        writer.WriteInt(WinnerScore);
        writer.WriteString(WinnerUsername);
        writer.WriteInt(WinnerCharacterIndex);
    }

    public override void Deserialize(PacketReader reader)
    {
        WinnerPlayerIndex = reader.ReadInt();
        WinnerScore = reader.ReadInt();
        WinnerUsername = reader.ReadString();
        WinnerCharacterIndex = reader.ReadInt();
    }
}