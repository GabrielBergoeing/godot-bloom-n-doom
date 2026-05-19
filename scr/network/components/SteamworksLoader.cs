using Godot;
using System;
using System.IO;
using Steamworks;
using System.Runtime.InteropServices;

public partial class SteamworksLoader : Node
{
    public override void _Ready()
    {
        try
        {
            AutoloadSteamLibrary();

            bool running = SteamAPI.IsSteamRunning();
            GD.Print($"[SteamworksLoader] Steam running: {running}");

            bool initialized = SteamAPI.Init();

            if (initialized)
            {
                GD.Print("[SteamworksLoader] Steam initialized");
                GD.Print($"[SteamworksLoader] User: {SteamFriends.GetPersonaName()}");
            }
            else
                GD.PrintErr("[SteamworksLoader] Steam initialization failed");
        }
        catch (Exception e)
        {
            GD.PrintErr("[SteamworksLoader] Ready Error: ", e);
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
            GD.PrintErr("[SteamworksLoader] Exit Error: ", e);
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
            GD.PrintErr("[SteamworksLoader] Unsupported platform");
            return;
        }

        string libraryPath = Path.Combine(
            AppContext.BaseDirectory,
            "api",
            "steamworks",
            platformFolder,
            libraryName
        );

        GD.Print($"[SteamworksLoader] Loading Steam library: {libraryPath}");

        if (!File.Exists(libraryPath))
        {
            GD.PrintErr($"[SteamworksLoader] Steam library not found: {libraryPath}");
            return;
        }

        NativeLibrary.Load(libraryPath);
        GD.Print("[SteamworksLoader] Steam native library loaded");
    }
}