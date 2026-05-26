public class LobbyPlayerLeftPacket : NetworkPacket
{
    public override byte PacketId => (byte)NetworkPacketType.LobbyPlayerLeft;

    public ulong SteamId;

    public override void Serialize(PacketWriter writer)
    {
        writer.WriteULong(SteamId);
    }

    public override void Deserialize(PacketReader reader)
    {
        SteamId = reader.ReadULong();
    }
}