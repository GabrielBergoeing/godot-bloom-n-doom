using Godot;
using System;

public partial class TelemetryLogger : Node
{
    [Export] public float SampleInterval = 0.25f;
    [Export] public bool SetRecording = false;

    private FileAccess File;
    private float Timer = 0f;

    private bool IsCapturing = false;

    private string CaptureName;
    private string AbsolutePath;

    public override void _Ready()
    {
        if (SetRecording)
            StartCapture();
    }

    public override void _Process(double delta)
    {
        if (!IsCapturing || File == null || !SetRecording)
            return;

        if (CheckTimerDesync(delta))
            return;

        string row = GetLatestRowData();

        File.StoreLine(row);
        File.Flush();
    }

    public override void _ExitTree()
    {
        StopCapture();
    }

    // Manual stop function
    public void StopCapture()
    {
        if (!IsCapturing)
            return;

        IsCapturing = false;

        if (File != null)
        {
            File.Flush();
            File.Close();
            File = null;
        }

        GD.Print(
            $"Telemetry capture ended: {CaptureName}"
        );
    }

    private void StartCapture()
    {
        string timestamp = DateTime.Now.ToString(
            "yyyyMMdd_HHmmss"
        );

        CaptureName = $"match_{timestamp}";

        string relativePath =
            $"res://telemetry/{CaptureName}.txt";

        AbsolutePath =
            ProjectSettings.GlobalizePath(relativePath);

        DirAccess.MakeDirAbsolute(
            ProjectSettings.GlobalizePath(
                "res://telemetry"
            )
        );

        File = FileAccess.Open(
            AbsolutePath,
            FileAccess.ModeFlags.Write
        );

        if (File == null)
        {
            GD.PrintErr(
                $"Failed to create telemetry file: {AbsolutePath}"
            );

            return;
        }

        // CSV Header
        File.StoreLine(
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

        IsCapturing = true;

        GD.Print(
            $"Telemetry capture started: {AbsolutePath}"
        );
    }

    private bool CheckTimerDesync(double delta)
    {
        Timer += (float)delta;
        if (Timer < SampleInterval)
            return false;

        Timer = 0f;
        return true;
    }

    private string GetLatestRowData()
    {
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

        double memoryMb = (double)Performance.GetMonitor(
            Performance.Monitor.MemoryStatic) / 
            (1024.0 * 1024.0
        );

        float objectCount = (float)Performance.GetMonitor(
            Performance.Monitor.ObjectCount
        );

        float nodeCount = (float)Performance.GetMonitor(
            Performance.Monitor.ObjectNodeCount
        );

        float drawCalls = (float)Performance.GetMonitor(
            Performance.Monitor.RenderTotalDrawCallsInFrame
        );

        return
            $"{timestamp}," +
            $"{fps}," +
            $"{frameTime:F2}," +
            $"{processTime:F4}," +
            $"{physicsTime:F4}," +
            $"{memoryMb:F2}," +
            $"{objectCount}," +
            $"{nodeCount}," +
            $"{drawCalls}";
    }
}