public class LobbyStartGamePacket : NetworkPacket
{
    public override byte PacketId => (byte)NetworkPacketType.LobbyStartGame;

    public int Seed;

    public override void Serialize(PacketWriter writer)
    {
        writer.WriteInt(Seed);
    }

    public override void Deserialize(PacketReader reader)
    {
        Seed = reader.ReadInt();
    }
}