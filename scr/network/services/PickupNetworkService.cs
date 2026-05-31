using Godot;
using System;
using System.Collections.Generic;

public partial class PickupNetworkService : Node
{
    public static PickupNetworkService Instance { get; private set; }

    private SteamMatchManager Match => SteamMatchManager.Instance;
    private EventManager Event => EventManager.Instance;
    private NetworkRoot Network => NetworkRoot.Instance;

    private readonly Dictionary<int, Pickup> _activePickups = new();
    private int _nextPickupId = 0;

    private Node _levelNode;

    public override void _Ready()
    {
        Instance = this;
        GD.Print($"[PickupNetworkService] _Ready — IsOnline: {Network.IsOnline}");
    }

    public override void _ExitTree()
    {
        if (Event != null && Network.IsOnline && Network.Lobby.IsHost)
        {
            Event.OnPickupSpawned -= OnHostPickupSpawned;
            Event.OnPickupDropped -= OnHostPickupSpawned;
        }

        if (Match != null)
        {
            Match.OnPickupSpawned -= HandleRemotePickupSpawned;
            Match.OnPickupCollected -= HandleRemotePickupCollected;
            Match.OnPickupSpawnRequested -= HandlePickupSpawnRequest;
            Match.OnPickupCollectRequested -= HandlePickupCollectRequest;
        }
    }

    // In PickupNetworkService.Initialize:
    public void Initialize(Node levelNode)
    {
        GD.Print($"[PickupNetworkService] Initialize — IsHost: {Network.IsOnline && Network.Lobby.IsHost}, Event: {Event != null}");
        Match.OnPickupSpawned += HandleRemotePickupSpawned;
        Match.OnPickupCollected += HandleRemotePickupCollected;
        Match.OnPickupSpawnRequested += HandlePickupSpawnRequest;
        Match.OnPickupCollectRequested += HandlePickupCollectRequest;
        
        if (!Network.IsOnline || !Network.Lobby.IsHost) return;

        if (Event == null)
        {
            GD.PrintErr("[PickupNetworkService] EventManager is null during Initialize");
            return;
        }

        _levelNode = levelNode;
        Event.OnPickupSpawned += OnHostPickupSpawned;
        Event.OnPickupDropped += OnHostPickupSpawned;
        GD.Print("[PickupNetworkService] Subscribed to EventManager");
    }

    private void OnHostPickupSpawned(Pickup pickup)
    {
        int id = _nextPickupId++;
        pickup.NetworkPickupId = id;
        _activePickups[id] = pickup;

        Match.BroadcastPickupSpawned(pickup.ItemData.ItemId, pickup.Position, id);
        GD.Print($"[PickupNetworkService] Registered and broadcast pickup {id} ({pickup.ItemData.ItemId})");
    }

    private void HandlePickupSpawnRequest(MatchPickupSpawnRequestPacket packet)
    {
        if (!Network.Lobby.IsHost) return;

        ItemData item = ItemDatabase.Instance.GetItem(packet.ItemId);
        if (item == null)
        {
            GD.PrintErr($"[PickupNetworkService] Spawn request for unknown item: {packet.ItemId}");
            return;
        }

        GD.Print($"[PickupNetworkService] Host handling spawn request for {packet.ItemId} at {packet.Position}");
        Event.SpawnItem(item, packet.Position);
    }

    public bool CollectPickup(int networkPickupId, Player collector)
    {
        if (!Network.Lobby.IsHost) return false;

        if (!_activePickups.TryGetValue(networkPickupId, out Pickup pickup))
            return false;

        ItemData data = pickup.ItemData;
        if (!collector.Hotbar.CanAddItem(data))
            return false;

        collector.Hotbar.AddItem(data);

        foreach (var node in GetTree().GetNodesInGroup("players"))
            if (node is Player p)
                p.PickupsInRange.Remove(pickup);

        RemovePickupNode(networkPickupId);
        Match.BroadcastPickupCollected(networkPickupId);

        GD.Print($"[PickupNetworkService] Pickup {networkPickupId} collected by Player {collector.PlayerId}");
        return true;
    }

    private void HandleRemotePickupSpawned(MatchPickupSpawnedPacket packet)
    {
        if (Network.Lobby.IsHost) return;

        ItemData data = ItemDatabase.Instance.GetItem(packet.ItemId);
        if (data == null)
        {
            GD.PrintErr($"[PickupNetworkService] Unknown item: {packet.ItemId}");
            return;
        }

        CreatePickupNode(data, packet.Position, packet.NetworkPickupId);
        GD.Print($"[PickupNetworkService] Peer received pickup {packet.NetworkPickupId}");
    }

    private void HandleRemotePickupCollected(MatchPickupCollectedPacket packet)
    {
        if (Network.Lobby.IsHost) return;
        RemovePickupNode(packet.NetworkPickupId);
        GD.Print($"[PickupNetworkService] Peer removed pickup {packet.NetworkPickupId}");
    }

    private Pickup CreatePickupNode(ItemData data, Vector2 position, int networkId)
    {
        if (data.PickupScene == null) return null;

        var pickup = data.PickupScene.Instantiate<Pickup>();
        pickup.SetItemData(data);
        pickup.NetworkPickupId = networkId;
        pickup.Position = position;

        _levelNode.AddChild(pickup);
        _activePickups[networkId] = pickup;

        return pickup;
    }

    private void RemovePickupNode(int networkId)
    {
        if (!_activePickups.TryGetValue(networkId, out Pickup pickup)) return;
        pickup.QueueFree();
        _activePickups.Remove(networkId);
    }

    private void HandlePickupCollectRequest(MatchPickupCollectRequestPacket packet)
    {
        if (!Network.Lobby.IsHost) return;

        // Find the player by SteamId
        var players = GetTree().GetNodesInGroup("players");
        foreach (var node in players)
        {
            if (node is Player player && player.OwnerSteamId == packet.RequesterSteamId)
            {
                CollectPickup(packet.NetworkPickupId, player);
                return;
            }
        }

        GD.PrintErr($"[PickupNetworkService] Collect request from unknown player {packet.RequesterSteamId}");
    }
}