using Godot;
using System;

[GlobalClass]
public partial class CharacterDatabase : Resource
{
    [Export] public CharacterData[] Characters;
}
