public class PlayerSpawnData
{
    public ulong SteamId;
    public int PlayerId;
    public int CharacterIndex;
    public int SpawnIndex;
    public bool IsLocalOwner;

    public void Serialize(PacketWriter writer)
    {
        writer.WriteULong(SteamId);
        writer.WriteInt(PlayerId);
        writer.WriteInt(CharacterIndex);
        writer.WriteInt(SpawnIndex);
        writer.WriteBool(IsLocalOwner);
    }

    public void Deserialize(PacketReader reader)
    {
        SteamId = reader.ReadULong();
        PlayerId = reader.ReadInt();
        CharacterIndex = reader.ReadInt();
        SpawnIndex = reader.ReadInt();
        IsLocalOwner = reader.ReadBool();
    }
}
