using Godot;

public class MatchFarmRemovePacket : NetworkPacket
{
    public override byte PacketId => (byte)NetworkPacketType.MatchFarmRemove;
    public Vector2I Cell;
    public int PlayerIndex;

    public override void Serialize(PacketWriter writer)
    {
        writer.WriteVector2I(Cell);
        writer.WriteInt(PlayerIndex);
    }

    public override void Deserialize(PacketReader reader)
    {
        Cell = reader.ReadVector2I();
        PlayerIndex = reader.ReadInt();
    }
}