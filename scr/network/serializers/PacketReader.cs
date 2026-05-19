using Godot;
using System.IO;
using System.Text;

public class PacketReader
{
    private readonly MemoryStream _stream;
    private readonly BinaryReader _reader;

    public PacketReader(byte[] data)
    {
        _stream = new MemoryStream(data);
        _reader = new BinaryReader(_stream);
    }

    public byte ReadByte()
    {
        return _reader.ReadByte();
    }

    public int ReadInt()
    {
        return _reader.ReadInt32();
    }

    public float ReadFloat()
    {
        return _reader.ReadSingle();
    }

    public bool ReadBool()
    {
        return _reader.ReadBoolean();
    }

    public string ReadString()
    {
        int length = _reader.ReadInt32();

        byte[] bytes =
            _reader.ReadBytes(length);

        return Encoding.UTF8.GetString(bytes);
    }

    public Vector2 ReadVector2()
    {
        return new Vector2(
            _reader.ReadSingle(),
            _reader.ReadSingle()
        );
    }
}