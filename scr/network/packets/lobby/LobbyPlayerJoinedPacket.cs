public class LobbyPlayerJoinedPacket : NetworkPacket
{
    public override byte PacketId => (byte)NetworkPacketType.LobbyPlayerState;

    public ulong SteamId;
    public string Username;

    public override void Serialize(PacketWriter writer)
    {
        writer.WriteULong(SteamId);
        writer.WriteString(Username);
    }

    public override void Deserialize(PacketReader reader)
    {
        SteamId = reader.ReadULong();
        Username = reader.ReadString();
    }
}