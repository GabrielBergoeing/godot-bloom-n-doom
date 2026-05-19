using Godot;
using System;
using System.IO;
using Steamworks;
using System.Runtime.InteropServices;

public partial class SteamworksHelper : Node
{
    public override void _Ready()
    {
        try
        {
            AutoloadSteamLibrary();

            bool running = SteamAPI.IsSteamRunning();
            GD.Print($"[SteamworksHelper] Steam running: {running}");

            bool initialized = SteamAPI.Init();

            if (initialized)
            {
                GD.Print("[SteamworksHelper] Steam initialized");
                GD.Print($"[SteamworksHelper] User: {SteamFriends.GetPersonaName()}");
            }
            else
                GD.PrintErr("[SteamworksHelper] Steam initialization failed");
        }
        catch (Exception e)
        {
            GD.PrintErr("[SteamworksHelper] Ready Error: ", e);
        }
    }

    public override void _ExitTree()
    {
        try
        {
            SteamAPI.Shutdown();
        }
        catch (Exception e)
        {
            GD.PrintErr("[SteamworksHelper] Exit Error: ", e);
        }
    }

    private void AutoloadSteamLibrary()
    {
        string platformFolder;
        string libraryName;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            platformFolder = "win64";
            libraryName = "steam_api64.dll";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            platformFolder = "linux64";
            libraryName = "libsteam_api.so";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            platformFolder = "macos";
            libraryName = "libsteam_api.dylib";
        }
        else
        {
            GD.PrintErr("[SteamworksHelper] Unsupported platform");
            return;
        }

        string libraryPath = Path.Combine(
            AppContext.BaseDirectory,
            "api",
            "steamworks",
            platformFolder,
            libraryName
        );

        GD.Print($"[SteamworksHelper] Loading Steam library: {libraryPath}");

        if (!File.Exists(libraryPath))
        {
            GD.PrintErr($"[SteamworksHelper] Steam library not found: {libraryPath}");
            return;
        }

        NativeLibrary.Load(libraryPath);
        GD.Print("[SteamworksHelper] Steam native library loaded");
    }
}