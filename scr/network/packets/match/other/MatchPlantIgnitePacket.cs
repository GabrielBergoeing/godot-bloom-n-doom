using Godot;
using System;

public partial class MatchPlantIgnitePacket : NetworkPacket
{
    public override byte PacketId => (byte)NetworkPacketType.MatchPlantIgnite;
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