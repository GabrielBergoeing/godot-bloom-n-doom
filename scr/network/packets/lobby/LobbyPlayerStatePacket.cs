public class LobbyPlayerStatePacket : NetworkPacket
{
    public override byte PacketId => (byte)NetworkPacketType.LobbyPlayerState;

    public ulong SteamId;
    public string Username;
    public int PlayerId;
    public int CharacterIndex;
    public bool LockedIn;
    public int SlotIndex;

    public override void Serialize(PacketWriter writer)
    {
        writer.WriteULong(SteamId);
        writer.WriteString(Username);
        writer.WriteInt(PlayerId);
        writer.WriteInt(CharacterIndex);
        writer.WriteBool(LockedIn);
        writer.WriteInt(SlotIndex);
    }

    public override void Deserialize(PacketReader reader)
    {
        SteamId = reader.ReadULong();
        Username = reader.ReadString();
        PlayerId = reader.ReadInt();
        CharacterIndex = reader.ReadInt();
        LockedIn = reader.ReadBool();
        SlotIndex = reader.ReadInt();
    }

    public static LobbyPlayerStatePacket FromBytes(byte[] data)
    {
        PacketReader reader = new PacketReader(data);
        reader.ReadByte();

        LobbyPlayerStatePacket packet = new LobbyPlayerStatePacket();
        packet.Deserialize(reader);

        return packet;
    }
}