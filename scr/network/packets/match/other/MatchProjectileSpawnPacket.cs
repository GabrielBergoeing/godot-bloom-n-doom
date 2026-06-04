using Godot;

public class MatchProjectileSpawnPacket : NetworkPacket
{
    public override byte PacketId => (byte)NetworkPacketType.MatchProjectileSpawn;
    public int PlayerId;
    public ulong OwnerSteamId;
    public Vector2 Position;
    public Vector2 Direction;
    public Vector2 InheritedVelocity;

    public override void Serialize(PacketWriter writer)
    {
        writer.WriteInt(PlayerId);
        writer.WriteULong(OwnerSteamId);
        writer.WriteVector2(Position);
        writer.WriteVector2(Direction);
        writer.WriteVector2(InheritedVelocity);
    }

    public override void Deserialize(PacketReader reader)
    {
        PlayerId = reader.ReadInt();
        OwnerSteamId = reader.ReadULong();
        Position = reader.ReadVector2();
        Direction = reader.ReadVector2();
        InheritedVelocity = reader.ReadVector2();
    }
}