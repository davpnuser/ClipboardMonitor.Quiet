using ClipboardMonitor.Quiet.ClipboardListenerImp;
using ClipboardMonitor.Quiet.Interfaces;
using System.Runtime.InteropServices;

namespace ClipboardMonitor.Quiet.Factories;

public static class ClipboardListenerFactory
{
    public static IClipboardListener CreateClipboardListener()
    {
        if (OperatingSystem.IsWindows())
        {
            return new WindowsClipboardListener();
        }
        else if (OperatingSystem.IsLinux())
        {
            return new LinuxClipboardListener();
        }

        throw new PlatformNotSupportedException("Clipboard listener not supported on this platform.");
    }

    public static T CreateClipboardListener<T>() where T : IClipboardListener
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && typeof(T) == typeof(IWindowsClipboardListener)
                ? (T)(IClipboardListener)new WindowsClipboardListener()
                : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && typeof(T) == typeof(ILinuxClipboardListener)
                    ? (T)(IClipboardListener)new LinuxClipboardListener()
                    : throw new NotSupportedException("Unsupported platform or interface requested.");
    }
}