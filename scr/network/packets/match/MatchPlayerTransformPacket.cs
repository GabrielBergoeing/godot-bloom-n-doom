using Godot;

public class MatchPlayerTransformPacket : NetworkPacket
{
    public override byte PacketId => (byte)NetworkPacketType.MatchPlayerTransform;
    public int PlayerId;
    public Vector2 Position;
    public float Rotation;

    public override void Serialize(PacketWriter writer)
    {
        writer.WriteInt(PlayerId);
        writer.WriteVector2(Position);
        writer.WriteFloat(Rotation);
    }

    public override void Deserialize(PacketReader reader)
    {
        PlayerId = reader.ReadInt();
        Position = reader.ReadVector2();
        Rotation = reader.ReadFloat();
    }
}