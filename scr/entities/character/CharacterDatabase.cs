using Godot;
using System;

[GlobalClass]
public partial class CharacterDatabase : Resource
{
    [Export] public CharacterData[] Characters;

    public CharacterData GetCharacter(int characterId)
    {
        foreach (CharacterData data in Characters)
        {
            if(data.CharacterID == characterId)
                return data;
        }
        return Characters[0];
    }
}
