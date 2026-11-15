using ClipboardMonitor.Quiet.Enums;
using ClipboardMonitor.Quiet.EventArguments;
using ClipboardMonitor.Quiet.Interfaces;

namespace ClipboardMonitor.Quiet.ClipboardListenerImp;

public abstract class ClipboardListenerBase : IClipboardListener, IDisposable
{
    protected Task? _clipboardMonitorTask;
    protected CancellationTokenSource? _clipboardMonitorTokenCts;
    protected NotificationType _notificationType;
    protected bool _callbacksSet;

    public bool IsMonitoring { get; protected set; }

    public event EventHandler<ClipboardChangedEventArgs>? ClipboardChanged;

    public virtual void SetNotificationType(NotificationType notificationType)
    {
        if (_notificationType == notificationType) return;

        _notificationType = notificationType;

        RemoveCallbacks();
        SetCallbacks();
    }

    public virtual void Start()
    {
        if (IsMonitoring) return;

        if (!_callbacksSet)
            SetCallbacks();

        _clipboardMonitorTokenCts = new CancellationTokenSource();
        _clipboardMonitorTask = Task.Run(StartListener, _clipboardMonitorTokenCts.Token);

        IsMonitoring = true;
    }

    public virtual void Stop()
    {
        if (!IsMonitoring) return;

        RemoveCallbacks();
        StopListener();
        _clipboardMonitorTokenCts?.Cancel();
        _clipboardMonitorTokenCts = null;
        _clipboardMonitorTask = null;

        IsMonitoring = false;
    }

    public abstract bool ClearClipboardContent();

    protected void SetCallbacks()
    {
        switch (_notificationType)
        {
            case NotificationType.ChangedWithData:
                SetCallbacksWithData();
                break;

            case NotificationType.ChangeNotificationOnly:
                SetCallbacksNoData();
                break;

            case NotificationType.All:
                SetCallbacksNoData();
                SetCallbacksWithData();
                break;
        }

        _callbacksSet = true;
    }

    protected void RemoveCallbacks()
    {
        SetCallbacksNoData(true);
        SetCallbacksWithData(true);

        _callbacksSet = false;
    }

    protected abstract void SetCallbacksNoData(bool unset = false);

    protected abstract void SetCallbacksWithData(bool unset = false);

    protected abstract void StartListener();

    protected abstract void StopListener();

    protected void OnClipboardChanged(ClipboardChangedEventArgs e)
    {
        ClipboardChanged?.Invoke(this, e);
    }

    public void Dispose()
    {
        Stop();
    }
}
