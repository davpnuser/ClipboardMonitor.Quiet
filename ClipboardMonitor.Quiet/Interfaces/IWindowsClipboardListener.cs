namespace ClipboardMonitor.Quiet.Interfaces;

public interface IWindowsClipboardListener : IClipboardListener
{
    bool VerifyNewImageData { get; set; }
}