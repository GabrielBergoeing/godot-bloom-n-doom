public class PingPacket : NetworkPacket
{
    public override byte PacketId => (byte)NetworkPacketType.Ping;

    public override void Serialize(PacketWriter writer) { }
    public override void Deserialize(PacketReader reader) { }
}