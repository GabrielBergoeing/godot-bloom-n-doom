public class PongPacket : NetworkPacket
{
    public override byte PacketId => (byte)NetworkPacketType.Pong;

    public override void Serialize(PacketWriter writer) { }
    public override void Deserialize(PacketReader reader) { }
}