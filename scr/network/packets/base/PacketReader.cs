using Godot;
using System;
using System.IO;
using System.Text;

public class PacketReader : IDisposable
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

    public ulong ReadULong()
    {
        return _reader.ReadUInt64();
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
        byte[] bytes =_reader.ReadBytes(length);

        return Encoding.UTF8.GetString(bytes).Trim();
    }

    public Vector2 ReadVector2()
    {
        return new Vector2(
            _reader.ReadSingle(),
            _reader.ReadSingle()
        );
    }

    public Vector2I ReadVector2I()
    {
        return new Vector2I(
            _reader.ReadInt32(),
            _reader.ReadInt32()
        );
    }

    public void Dispose()
    {
        _reader?.Dispose();
        _stream?.Dispose();
    }
}