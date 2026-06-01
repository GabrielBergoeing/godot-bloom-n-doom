using Godot;

public class MatchFarmPreparePacket : NetworkPacket
{
    public override byte PacketId => (byte)NetworkPacketType.MatchFarmPrepare;
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