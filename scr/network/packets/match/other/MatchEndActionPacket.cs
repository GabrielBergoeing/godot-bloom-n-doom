using Godot;
using System;

public partial class MatchEndActionPacket : NetworkPacket
{
    public override byte PacketId => (byte)NetworkPacketType.MatchEndAction;
    public int Action; // 0 = CharacterSelect, 1 = StageSelect, 2 = MainMenu

    public override void Serialize(PacketWriter writer)
    {
        writer.WriteInt(Action);
    }

    public override void Deserialize(PacketReader reader)
    {
        Action = reader.ReadInt();
    }
}