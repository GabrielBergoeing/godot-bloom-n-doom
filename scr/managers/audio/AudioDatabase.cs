using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class AudioDatabase : Resource
{
    [Export] public AudioClipData[] Clips;
    private Dictionary<string, AudioClipData> _collection;

    public void Initialize()
    {
        _collection = new Dictionary<string, AudioClipData>();

        if (Clips == null)
            return;

        foreach (var data in Clips)
        {
            if (data == null || string.IsNullOrEmpty(data.Key))
                continue;

            if (!_collection.ContainsKey(data.Key))
                _collection.Add(data.Key, data);
        }
    }

    public AudioClipData Get(string name)
    {
        if (_collection == null)
            Initialize();

        return _collection.TryGetValue(name, out var data)
            ? data
            : null;
    }
}