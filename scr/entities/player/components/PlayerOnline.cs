using Godot;
using System;

public partial class PlayerOnline : Node
{
    private Player _player;
    private SteamMatchManager _match;

    [Export] private float BroadcastInterval = 0.05f;
    private float _broadcastTimer = 0f;

    public ulong OwnerSteamId { get; private set; }
	public bool IsLocallyControlled { get; private set; }

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

        if (!_player.IsLocallyControlled)
        {
            _match.OnPlayerTransformReceived += OnTransformReceived;
            GD.Print($"[PlayerOnline] Transform signal in player with Steam ID {_player.OwnerSteamId}");
        }
    }

    public override void _ExitTree()
    {
        if (_match != null && !_player.IsLocallyControlled)
            _match.OnPlayerTransformReceived -= OnTransformReceived;
    }

    public override void _Process(double delta)
    {
        if (_player == null) return;

        if (_player.IsLocallyControlled)
            HandleBroadcast((float)delta);
        else
            HandleInterpolation(delta);
    }

    private void HandleBroadcast(float delta)
    {
        _broadcastTimer -= delta;
        if (_broadcastTimer > 0f) return;

        _broadcastTimer = BroadcastInterval;
        GD.Print("[PlayerOnline] Broadcasting position");

        _match.BroadcastTransform(
            _player.PlayerId,
            _player.GlobalPosition,
            _player.Rotation,
            _player.Anim.CurrentAction
        );
    }

    private void OnTransformReceived(MatchPlayerTransformPacket packet)
    {
        if (packet.PlayerId != _player.PlayerId) return;

        _targetPosition = packet.Position;
        _targetRotation = packet.Rotation;
        _targetAction = packet.Action;
        _hasTarget = true;
    }

    private void HandleInterpolation(double delta)
    {
        if (!_hasTarget) return;

        GD.Print("[PlayerOnline] Interpolating Position");

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
}