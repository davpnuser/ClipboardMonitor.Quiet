using ClipboardMonitor.Quiet.Enums;
using ClipboardMonitor.Quiet.EventArguments;

namespace ClipboardMonitor.Quiet.Interfaces;

public interface IClipboardListener
{
    event EventHandler<ClipboardChangedEventArgs>? ClipboardChanged;
    bool IsMonitoring { get; }

    void SetNotificationType(NotificationType notificationType);
    void Start();
    void Stop();
    bool ClearClipboardContent();
}
