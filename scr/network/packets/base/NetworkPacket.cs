public abstract class NetworkPacket
{
    public abstract byte PacketId { get; }

    public abstract void Serialize(PacketWriter writer);
    public abstract void Deserialize(PacketReader reader);

    public byte[] ToArray()
    {
        using PacketWriter writer = new PacketWriter(PacketId);

        Serialize(writer);
        return writer.ToArray();
    }
}