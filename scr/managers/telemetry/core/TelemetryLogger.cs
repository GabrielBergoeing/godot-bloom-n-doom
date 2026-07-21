using Godot;
using System;
using System.Diagnostics;

public partial class TelemetryLogger : Node
{
    [Export] private float SampleInterval = 0.25f;
    [Export] private bool SetRecording = false;
    [Export] private float FlushInterval = 5.0f;
    
    private Process CurrentProcess;
    private TimeSpan LastCPUTime;
    private DateTime LastTime;

    private IGPUProvider GPUProvider;

    private FileAccess File;
    private float Timer = 0f;
    private float FlushTimer = 0f;
    private bool IsCapturing = false;

    private string CaptureName;
    private string AbsolutePath;

    public override void _Ready()
    {
        SetGPUProvider();
        if (!OS.IsDebugBuild())
            SetRecording = true;

        if (SetRecording)
            StartCapture();
    }

    public override void _Process(double delta)
    {
        if (!IsCapturing || File == null || !SetRecording)
            return;

        UpdateFlushTimer(delta);
        if (!CheckTimerDesync(delta))
            return;

        string row = GetLatestRowData();
        File.StoreLine(row);
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

        GD.Print($"[TelemetryLogger] Capture ended: {CaptureName}");
    }

    private void SetGPUProvider()
    {
        GPUProvider = GPUProviderFactory.Create();
        GD.Print($"[TelemetryLogger] GPU provider set as: {GPUProvider.GetGPUName()}");
    }

    private void StartCapture()
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        AbsolutePath = DeterminePathing(timestamp);

        File = FileAccess.Open(
            AbsolutePath,
            FileAccess.ModeFlags.Write
        );

        if (File == null)
        {
            GD.PrintErr($"[TelemetryLogger] Failed to create telemetry file: {AbsolutePath}");
            return;
        }

        // CSV Header
        File.StoreLine(
            "timestamp," +
            "fps," +
            "cpu_usage," +
            "gpu_usage," +
            "frame_time_ms," +
            "process_time," +
            "physics_time," +
            "memory_mb," +
            "object_count," +
            "node_count," +
            "draw_calls," +
            "latency_ms," +
            "host_status"
        );

        IsCapturing = true;
        GetOSProcessRegister();
        GD.Print($"[TelemetryLogger] Capture started: {AbsolutePath}");
    }

    private bool CheckTimerDesync(double delta)
    {
        Timer += (float)delta;
        if (Timer < SampleInterval)
            return false;

        Timer = 0f;
        return true;
    }

    private void UpdateFlushTimer(double delta)
    {
        FlushTimer += (float)delta;

        if (FlushTimer < FlushInterval)
            return;

        FlushTimer = 0f;

        if (File != null)
            File.Flush();
    }

    private void GetOSProcessRegister()
    {
        CurrentProcess = Process.GetCurrentProcess();
        LastCPUTime = CurrentProcess.TotalProcessorTime;
        LastTime = DateTime.UtcNow;
    }

    private string DeterminePathing(string timestamp)
    {
        CaptureName = $"match_{timestamp}";
        string basePath;

        if(OS.IsDebugBuild())
            basePath = $"res://telemetry";
        else
            basePath = $"user://telemetry";

        DirAccess.MakeDirAbsolute(
            ProjectSettings.GlobalizePath(basePath)
        );
        return ProjectSettings.GlobalizePath(basePath + $"/{CaptureName}.txt");
    }

    private float GetCpuUsagePercent()
    {
        CurrentProcess.Refresh();

        DateTime time = DateTime.UtcNow;
        TimeSpan cpuTime = CurrentProcess.TotalProcessorTime;

        double cpuUsedMs = (cpuTime - LastCPUTime).TotalMilliseconds;
        double elapsedMs = (time - LastTime).TotalMilliseconds;

        LastCPUTime = cpuTime;
        LastTime = time;

        if (elapsedMs <= 0)
            return 0f;

        float cpuUsage = (float)(
            cpuUsedMs /
            (elapsedMs * System.Environment.ProcessorCount) * 100.0
        );

        return cpuUsage;
    }

    private string GetLatestRowData()
    {
        ulong timestamp = Time.GetTicksMsec();

        float fps = (float)Performance.GetMonitor(
            Performance.Monitor.TimeFps
        );

        float cpuUsage = GetCpuUsagePercent();

        float gpuUsage = GPUProvider.GetGPUUsagePercent();

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

        string latency = "N/A";
        if (NetworkRoot.Instance?.IsOnline == true && SteamNetworkManager.Instance != null)
        {
            float avg = SteamNetworkManager.Instance.AverageLatencyMs;
            latency = avg >= 0f ? $"{avg:F1}" : "N/A";
        }

        bool isHost = false;
        if (NetworkRoot.Instance?.IsOnline == true && SteamNetworkManager.Instance != null)
            isHost = NetworkRoot.Instance.IsHost();

        return
            $"{timestamp}," +
            $"{fps}," +
            $"{cpuUsage:F2}," +
            $"{gpuUsage:F2}," +
            $"{frameTime:F2}," +
            $"{processTime:F4}," +
            $"{physicsTime:F4}," +
            $"{memoryMb:F2}," +
            $"{objectCount}," +
            $"{nodeCount}," +
            $"{drawCalls}," +
            $"{latency}" +
            $"{isHost}";
    }
}