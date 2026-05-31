using Godot;

public partial class PlayerOnline : Node
{
    private Player _player;
    private SteamMatchManager _match;

    [Export] public float BroadcastInterval = 0.05f;
    private float _broadcastTimer = 0f;

    // Ownership — moved from Player
    public ulong OwnerSteamId { get; private set; }
    public bool IsLocallyControlled { get; private set; }
    public Vector2 FacingDir { get; private set; } = Vector2.Down;

    // Remote interpolation targets
    private Vector2 _targetPosition;
    private float _targetRotation;
    private string _targetAction;
    private bool _hasTarget = false;

    private const float InterpolationSpeed = 15f;

    public override void _Ready()
    {
        _player = GetParent<Player>();
        _match = SteamMatchManager.Instance;

        if (_player == null)
        {
            GD.PrintErr("[PlayerOnline] No Player parent found");
            return;
        }

        if (_match == null)
        {
            GD.PrintErr("[PlayerOnline] SteamMatchManager not found");
            return;
        }
    }

    public override void _ExitTree()
    {
        if (_match == null) return;

        if (!IsLocallyControlled)
        {
            _match.OnPlayerTransformReceived -= OnTransformReceived;
            _match.OnHotbarSyncReceived -= OnHotbarSyncReceived;
        }
        else
        {
            if (_player?.Hotbar != null)
                _player.Hotbar.OnSlotChanged -= BroadcastHotbarState;
        }
    }

    public override void _Process(double delta)
    {
        if (_player == null || _match == null) return;

        if (IsLocallyControlled)
            HandleBroadcast((float)delta);
        else
            HandleInterpolation(delta);
    }

    public void Initialize(ulong ownerSteamId, ulong localSteamId)
    {
        OwnerSteamId = ownerSteamId;
        IsLocallyControlled = ownerSteamId == localSteamId;

        GD.Print($"[PlayerOnline] Initialized — SteamId: {ownerSteamId}, Local: {IsLocallyControlled}");

        if (!IsLocallyControlled)
        {
            _match.OnPlayerTransformReceived += OnTransformReceived;
            _match.OnHotbarSyncReceived += OnHotbarSyncReceived;
        }
        else
        {
            _player.Hotbar.OnSlotChanged += BroadcastHotbarState;
        }

        _player.Input.SetRemoteControlled(!IsLocallyControlled);
    }

    private void HandleBroadcast(float delta)
    {
        _broadcastTimer -= delta;
        if (_broadcastTimer > 0f) return;

        _broadcastTimer = BroadcastInterval;

        _match.BroadcastTransform(
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

    private void HandleInterpolation(double delta)
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
        _match.BroadcastHotbarSlot(
            OwnerSteamId,
            _player.Hotbar.CurrentSlot,
            stack?.Data?.ItemId ?? "",
            stack?.Amount ?? 0
        );
    }

    private void OnHotbarSyncReceived(MatchPlayerHotbarPacket packet)
    {
        if (packet.OwnerSteamId != OwnerSteamId) return;
        _player.Hotbar.SelectSlot(packet.SlotIndex);
    }
}