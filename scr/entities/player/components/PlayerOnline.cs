using Godot;

public partial class PlayerOnline : Node
{
    private Player _player;
    private SteamMatchManager Match => SteamMatchManager.Instance;

    [Export] public float BroadcastInterval = 0.05f;
    private float _broadcastTimer = 0f;

    public ulong OwnerSteamId { get; private set; }
    public bool IsLocallyControlled { get; private set; }
    public Vector2 FacingDir { get; private set; } = Vector2.Down;

    private Vector2 _targetPosition;
    private float _targetRotation;
    private string _targetAction;
    private bool _hasTarget = false;

    private const float InterpolationSpeed = 15f;

    public override void _Ready()
    {
        _player = GetParent<Player>();

        if (_player == null)
        {
            GD.PrintErr("[PlayerOnline] No Player parent found");
            return;
        }

        if (Match == null)
        {
            GD.PrintErr("[PlayerOnline] SteamMatchManager not found");
            return;
        }
    }

    public override void _ExitTree()
    {
        if (Match == null) return;

        if (!IsLocallyControlled)
        {
            Match.OnPlayerTransformReceived -= OnTransformReceived;
            Match.OnHotbarSyncReceived -= HandleHotbarSync;
        }
        else
        {
            if (_player?.Hotbar != null)
                _player.Hotbar.OnSlotChanged -= BroadcastHotbarState;
        }
    }

    public override void _Process(double delta)
    {
        if (_player == null || Match == null) return;

        if (IsLocallyControlled)
            HandleTransformBroadcast((float)delta);
        else
            HandleTransformInterpolation(delta);
    }

    public void Initialize(ulong ownerSteamId, ulong localSteamId)
    {
        OwnerSteamId = ownerSteamId;
        IsLocallyControlled = ownerSteamId == localSteamId;

        GD.Print($"[PlayerOnline] Initialized — SteamId: {ownerSteamId}, Local: {IsLocallyControlled}");

        if (!IsLocallyControlled)
        {
            Match.OnPlayerTransformReceived += OnTransformReceived;
            Match.OnHotbarSyncReceived += HandleHotbarSync;
        }
        else
        {
            _player.Hotbar.OnSlotChanged += BroadcastHotbarState;
        }

        _player.Input.SetRemoteControlled(!IsLocallyControlled);
    }

    public void RequestPickupSpawn(ItemData data, Vector2 spawnPos)
    {
        Match.RequestPickupSpawn(OwnerSteamId, data.ItemId, spawnPos);
    }

    private void HandleTransformBroadcast(float delta)
    {
        _broadcastTimer -= delta;
        if (_broadcastTimer > 0f) return;

        _broadcastTimer = BroadcastInterval;

        Match.BroadcastTransform(
            _player.PlayerId,
            OwnerSteamId,
            _player.GlobalPosition,
            _player.Rotation,
            _player.Anim.CurrentAction,
            _player.GetFacingDirection()
        );
    }

    private void OnTransformReceived(MatchPlayerTransformPacket packet)
    {
        if (packet.OwnerSteamId != OwnerSteamId) return;

        _targetPosition = packet.Position;
        _targetRotation = packet.Rotation;
        _targetAction = packet.Action;
        _hasTarget = true;
        FacingDir = packet.FacingDir;
    }

    private void HandleTransformInterpolation(double delta)
    {
        if (!_hasTarget) return;

        _player.GlobalPosition = _player.GlobalPosition.Lerp(
            _targetPosition,
            InterpolationSpeed * (float)delta
        );

        _player.Rotation = Mathf.LerpAngle(
            _player.Rotation,
            _targetRotation,
            InterpolationSpeed * (float)delta
        );

        if (_targetAction != null)
            _player.Anim.SetAction(_targetAction);
    }

    private void BroadcastHotbarState()
    {
        var stack = _player.Hotbar.GetCurrentStack();

        GD.Print(
            $"[PlayerOnline] BroadcastHotbarState " +
            $"Owner:{OwnerSteamId} " +
            $"Slot:{_player.Hotbar.CurrentSlot} " +
            $"Item:{stack?.Data?.ItemId} " +
            $"Amount:{stack?.Amount}"
        );

        Match.BroadcastHotbarSlot(
            OwnerSteamId,
            _player.Hotbar.CurrentSlot,
            stack?.Data?.ItemId ?? "",
            stack?.Amount ?? 0
        );
    }

    private void HandleHotbarSync(MatchPlayerHotbarPacket packet)
    {
        if (packet.OwnerSteamId != OwnerSteamId) return;

        GD.Print($"[PlayerOnline] Hotbar sync — slot {packet.SlotIndex}, item '{packet.ItemId}', amount {packet.Amount}");

        if (IsLocallyControlled)
        {
            if (!string.IsNullOrEmpty(packet.ItemId))
            {
                ItemData item = ItemDatabase.Instance?.GetItem(packet.ItemId);
                if (item != null)
                    _player.Hotbar.SetSlotForRemote(packet.SlotIndex, item, packet.Amount);
            }
            else
                _player.Hotbar.SetSlotForRemote(packet.SlotIndex, null, 0);

            _player.Hotbar.SelectSlot(packet.SlotIndex, false);
            return;
        }

        _player.Hotbar.SelectSlot(packet.SlotIndex, false);

        if (!string.IsNullOrEmpty(packet.ItemId))
        {
            ItemData item = ItemDatabase.Instance?.GetItem(packet.ItemId);
            if (item != null)
                _player.Hotbar.SetSlotForRemote(packet.SlotIndex, item, packet.Amount);
            else
                GD.PrintErr($"[PlayerOnline] Hotbar sync item not found: {packet.ItemId}");
        }
        else
            _player.Hotbar.SetSlotForRemote(packet.SlotIndex, null, 0);
    }
}