using Godot;
using System.IO;
using System.Text;

public class PacketWriter
{
    private readonly MemoryStream _stream;
    private readonly BinaryWriter _writer;

    public PacketWriter(byte packetId)
    {
        _stream = new MemoryStream();
        _writer = new BinaryWriter(_stream);

        WriteByte(packetId);
    }

    public void WriteByte(byte value)
    {
        _writer.Write(value);
    }

    public void WriteInt(int value)
    {
        _writer.Write(value);
    }

    public void WriteFloat(float value)
    {
        _writer.Write(value);
    }

    public void WriteBool(bool value)
    {
        _writer.Write(value);
    }

    public void WriteString(string value)
    {
        value ??= "";

        byte[] bytes =
            Encoding.UTF8.GetBytes(value);

        _writer.Write(bytes.Length);
        _writer.Write(bytes);
    }

    public void WriteVector2(Vector2 value)
    {
        _writer.Write(value.X);
        _writer.Write(value.Y);
    }

    public byte[] ToArray()
    {
        return _stream.ToArray();
    }
}