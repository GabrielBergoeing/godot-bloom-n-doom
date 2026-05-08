using Godot;
using System;

public partial class TelemetryLogger : Node
{
    [Export]
    public float SampleInterval = 0.25f;

    private FileAccess _file;
    private float _timer = 0f;

    private bool _isCapturing = false;

    private string _captureName;
    private string _absolutePath;

    public override void _Ready()
    {
        StartCapture();
    }

    private void StartCapture()
    {
        string timestamp = DateTime.Now.ToString(
            "yyyyMMdd_HHmmss"
        );

        _captureName = $"match_{timestamp}";

        string relativePath =
            $"res://telemetry/{_captureName}.txt";

        _absolutePath =
            ProjectSettings.GlobalizePath(relativePath);

        DirAccess.MakeDirAbsolute(
            ProjectSettings.GlobalizePath(
                "res://telemetry"
            )
        );

        _file = FileAccess.Open(
            _absolutePath,
            FileAccess.ModeFlags.Write
        );

        if (_file == null)
        {
            GD.PrintErr(
                $"Failed to create telemetry file: {_absolutePath}"
            );

            return;
        }

        // CSV Header
        _file.StoreLine(
            "timestamp," +
            "fps," +
            "frame_time_ms," +
            "process_time," +
            "physics_time," +
            "memory_mb," +
            "object_count," +
            "node_count," +
            "draw_calls"
        );

        _isCapturing = true;

        GD.Print(
            $"Telemetry capture started: {_absolutePath}"
        );
    }

    public override void _Process(double delta)
    {
        if (!_isCapturing || _file == null)
            return;

        _timer += (float)delta;

        if (_timer < SampleInterval)
            return;

        _timer = 0f;

        ulong timestamp = Time.GetTicksMsec();

        float fps = (float)Performance.GetMonitor(
            Performance.Monitor.TimeFps
        );

        float frameTime = fps > 0
            ? 1000.0f / fps
            : 0f;

        float processTime = (float)Performance.GetMonitor(
            Performance.Monitor.TimeProcess
        );

        float physicsTime = (float)Performance.GetMonitor(
            Performance.Monitor.TimePhysicsProcess
        );

        double memoryMb =
            (double)Performance.GetMonitor(
                Performance.Monitor.MemoryStatic
            ) / (1024.0 * 1024.0);

        float objectCount = (float)Performance.GetMonitor(
            Performance.Monitor.ObjectCount
        );

        float nodeCount = (float)Performance.GetMonitor(
            Performance.Monitor.ObjectNodeCount
        );

        float drawCalls = (float)Performance.GetMonitor(
            Performance.Monitor.RenderTotalDrawCallsInFrame
        );

        string row =
            $"{timestamp}," +
            $"{fps}," +
            $"{frameTime:F2}," +
            $"{processTime:F4}," +
            $"{physicsTime:F4}," +
            $"{memoryMb:F2}," +
            $"{objectCount}," +
            $"{nodeCount}," +
            $"{drawCalls}";

        _file.StoreLine(row);

        // Safer for crash recovery
        _file.Flush();
    }

    // Manual stop function
    public void StopCapture()
    {
        if (!_isCapturing)
            return;

        _isCapturing = false;

        if (_file != null)
        {
            _file.Flush();
            _file.Close();
            _file = null;
        }

        GD.Print(
            $"Telemetry capture ended: {_captureName}"
        );
    }

    public override void _ExitTree()
    {
        StopCapture();
    }
}