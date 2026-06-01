using Godot;

public class MatchFarmPlantPacket : NetworkPacket
{
    public override byte PacketId => (byte)NetworkPacketType.MatchFarmPlant;
    public Vector2I Cell;
    public int PlayerIndex;
    public string SeedId;

    public override void Serialize(PacketWriter writer)
    {
        writer.WriteVector2I(Cell);
        writer.WriteInt(PlayerIndex);
        writer.WriteString(SeedId);
    }

    public override void Deserialize(PacketReader reader)
    {
        Cell = reader.ReadVector2I();
        PlayerIndex = reader.ReadInt();
        SeedId = reader.ReadString();
    }
}