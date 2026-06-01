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
    private readonly Dictionary<ulong, Player> _playersBySteamId = new();

    private bool _initialized = false;
    private int _nextPickupId = 0;

    private Node _levelNode;

    public override void _Ready()
    {
        Instance = this;
        GD.Print($"[PickupNetworkService] Ready — IsOnline: {NetworkRoot.Instance?.IsOnline}");
    }

    public override void _ExitTree()
    {
        if (!_initialized) return; // scene-placed node never initialized, skip
        _playersBySteamId.Clear();

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

    public void Initialize(Node levelNode)
    {
        _initialized = true;
        GD.Print($"[PickupNetworkService] Initialize — IsHost: {Network.IsOnline && Network.Lobby.IsHost}, Event: {Event != null}");

        _levelNode = levelNode;

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

        Event.OnPickupSpawned += OnHostPickupSpawned;
        Event.OnPickupDropped += OnHostPickupSpawned;
        GD.Print("[PickupNetworkService] Subscribed to EventManager");
    }

    public void RegisterPlayer(Player player)
    {
        if (player.OwnerSteamId == 0) return;
        _playersBySteamId[player.OwnerSteamId] = player;
        GD.Print($"[PickupNetworkService] Registered player {player.PlayerId} with SteamId {player.OwnerSteamId}");
    }

    private void OnHostPickupSpawned(Pickup pickup)
    {
        GD.Print($"[PickupNetworkService] Host registering pickup {pickup.ItemData?.ItemId}");
        int id = _nextPickupId++;
        pickup.NetworkPickupId = id;
        _activePickups[id] = pickup;

        Match.BroadcastPickupSpawned(pickup.ItemData.ItemId, pickup.Position, id);
        GD.Print($"[PickupNetworkService] Registered and broadcast pickup {id} ({pickup.ItemData.ItemId})");
    }

    private void HandlePickupSpawnRequest(MatchPickupSpawnRequestPacket packet)
    {
        if (!Network.Lobby.IsHost)
        {
            GD.Print("[PickupNetworkService] Ignoring spawn request on peer");
            return;
        }

        ItemData item = ItemDatabase.Instance?.GetItem(packet.ItemId);
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
        {
            GD.PrintErr($"[PickupNetworkService] Pickup {networkPickupId} not in active pickups");
            return false;
        }

        ItemData data = pickup.ItemData;
        if (!collector.Hotbar.CanAddItem(data))
        {
            GD.Print($"[PickupNetworkService] Hotbar full for player {collector.PlayerId}");
            return false;
        }

        collector.Hotbar.AddItem(data);
        for (int i = 0; i < collector.Hotbar.SlotCount; i++)
        {
            var s = collector.Hotbar.GetStackAt(i);

            GD.Print(
                $"[PickupNetworkService] Slot {i}: " +
                $"{s?.Data?.ItemId} x{s?.Amount}"
            );
        }

        foreach (var p in _playersBySteamId.Values)
            p.PickupsInRange.Remove(pickup);

        RemovePickupNode(networkPickupId);
        Match.BroadcastPickupCollected(networkPickupId);

        if (!collector.IsLocallyControlled)
        {
            var stack = collector.Hotbar.GetCurrentStack();
            Match.BroadcastHotbarSlot(
                collector.OwnerSteamId,
                collector.Hotbar.CurrentSlot,
                stack?.Data?.ItemId ?? "",
                stack?.Amount ?? 0
            );
            GD.Print($"[PickupNetworkService] Broadcast hotbar for remote collector {collector.OwnerSteamId}");
        }

        GD.Print($"[PickupNetworkService] Pickup {networkPickupId} collected by Player {collector.PlayerId}");
        return true;
    }

    private void HandleRemotePickupSpawned(MatchPickupSpawnedPacket packet)
    {
        if (Network.Lobby.IsHost) return;

        ItemData data = ItemDatabase.Instance?.GetItem(packet.ItemId);
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
        if (_levelNode == null) return null;

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

        GD.Print($"[PickupNetworkService] Collect request from {packet.RequesterSteamId}, pickup {packet.NetworkPickupId}");
        GD.Print($"[PickupNetworkService] Known players: {string.Join(", ", _playersBySteamId.Keys)}");

        if (!_playersBySteamId.TryGetValue(packet.RequesterSteamId, out Player player))
        {
            GD.PrintErr($"[PickupNetworkService] No player found for SteamId {packet.RequesterSteamId}");
            return;
        }

        CollectPickup(packet.NetworkPickupId, player);
    }
}