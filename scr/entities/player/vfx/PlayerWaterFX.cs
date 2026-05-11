using Godot;
using System;

public partial class PlayerWaterFX : Node2D
{
    private GpuParticles2D Particles;
    private Player Player;

    public override void _Ready()
    {
        Particles = GetNode<GpuParticles2D>("GPUParticles2D");
        Player = GetParent<Player>();

        var material = new ParticleProcessMaterial();
        material.Direction = new Vector3(0, 1, 0);
        material.InitialVelocityMin = 80;
        material.InitialVelocityMax = 120;

        Particles.ProcessMaterial = material;
    }

    public void Play()
    {
        Vector2 dir = Player.GetFacingDirection();
        if (dir == Vector2.Zero)
            dir = Vector2.Down;

        Rotation = dir.Angle() - Mathf.Pi / 2f;
        Particles.Restart();
    }
}