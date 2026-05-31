using Godot;

public class MatchPlayerTransformPacket : NetworkPacket
{
    public override byte PacketId => (byte)NetworkPacketType.MatchPlayerTransform;
    public int PlayerId;
    public ulong OwnerSteamId;
    public Vector2 Position;
    public float Rotation;
    public string Action;

    public override void Serialize(PacketWriter writer)
    {
        writer.WriteInt(PlayerId);
        writer.WriteULong(OwnerSteamId);
        writer.WriteVector2(Position);
        writer.WriteFloat(Rotation);
        writer.WriteString(Action ?? "idle");
    }

    public override void Deserialize(PacketReader reader)
    {
        PlayerId = reader.ReadInt();
        OwnerSteamId = reader.ReadULong();
        Position = reader.ReadVector2();
        Rotation = reader.ReadFloat();
        Action = reader.ReadString();
    }
}