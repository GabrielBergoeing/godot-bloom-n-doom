using Godot;
using System;
using System.IO;
using Steamworks;
using System.Runtime.InteropServices;

public partial class SteamworksLoader : Node
{
    public bool IsSteamRunning { get; private set; }
    public bool IsSteamInitialized { get; private set; }

    public bool IsSteamAvailable => IsSteamRunning && IsSteamInitialized;

    public override void _Ready()
    {
        try
        {
            AutoloadSteamLibrary();

            IsSteamRunning = SteamAPI.IsSteamRunning();
            IsSteamInitialized = SteamAPI.Init();

            if (!IsSteamInitialized)
                GD.PrintErr("[SteamworksLoader] Steam initialization failed");
        }
        catch (Exception e)
        {
            GD.PrintErr("[SteamworksLoader] Ready Error: ", e);
        }
    }

    public override void _Process(double delta)
    {
        if (IsSteamInitialized)
            SteamAPI.RunCallbacks();
    }

    public override void _ExitTree()
    {
        if (!IsSteamInitialized)
            return;

        try
        {
            SteamAPI.Shutdown();
        }
        catch (Exception e)
        {
            GD.PrintErr("[SteamworksLoader] Exit Error: ", e);
        }
    }

    private void AutoloadSteamLibrary()
    {
        string libraryName;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            libraryName = "steam_api64.dll";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            libraryName = "libsteam_api.so";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            libraryName = "libsteam_api.dylib";
        else
        {
            GD.PrintErr("[SteamworksLoader] Unsupported platform");
            return;
        }

        string libraryPath = Path.Combine(
            AppContext.BaseDirectory,
            libraryName
        );

        if (!File.Exists(libraryPath))
            return;

        NativeLibrary.Load(libraryPath);
        GD.Print("[SteamworksLoader] Steam native library loaded");
    }
}