using Godot;
using System;

[GlobalClass]
public partial class AudioClipData : Resource
{
    [Export] public AudioStream[] Clips = Array.Empty<AudioStream>();

    [Export(PropertyHint.Range, "0,1,0.01")] public float Volume = 1f;
    [Export(PropertyHint.Range, "0,2,0.01")] public float Pitch = 1f;

    public string Key => ResourcePath.GetFile().GetBaseName();

    public AudioStream GetRandomClip()
    {
        if (Clips == null || Clips.Length == 0)
            return null;

        int index = Random.Shared.Next(Clips.Length);
        return Clips[index];
    }
}