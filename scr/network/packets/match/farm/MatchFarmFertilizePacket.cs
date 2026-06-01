using Godot;

public class MatchFarmFertilizePacket : NetworkPacket
{
    public override byte PacketId => (byte)NetworkPacketType.MatchFarmFertilize;
    public Vector2I Cell;

    public override void Serialize(PacketWriter writer)
    {
        writer.WriteVector2I(Cell);
    }

    public override void Deserialize(PacketReader reader)
    {
        Cell = reader.ReadVector2I();
    }
}