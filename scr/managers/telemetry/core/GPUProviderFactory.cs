using Godot;

public static class GPUProviderFactory
{
    public static IGPUProvider Create()
    {
        string os = OS.GetName();

        if (os == "Linux")
            return new LinuxGPUProvider();

        if (os == "Windows")
            return new WindowsGPUProvider();

        return new NullGPUProvider();
    }
}