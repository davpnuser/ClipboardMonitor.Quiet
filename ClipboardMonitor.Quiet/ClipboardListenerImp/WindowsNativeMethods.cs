using System.Runtime.InteropServices;

namespace ClipboardMonitor.Quiet.ClipboardListenerImp;

public static class WindowsNativeMethods
{
    #region Imports

    [DllImport("user32.dll")]
    public static extern bool OpenClipboard(IntPtr hWndNewOwner);

    [DllImport("user32.dll")]
    public static extern bool CloseClipboard();

    [DllImport("user32.dll")]
    public static extern IntPtr GetClipboardData(uint uFormat);

    [DllImport("user32.dll")]
    public static extern bool IsClipboardFormatAvailable(uint format);

    [DllImport("gdi32.dll")]
    public static extern bool DeleteObject(IntPtr hObject);

    [DllImport("user32.dll")]
    public static extern uint GetClipboardSequenceNumber();

    #endregion

    public const uint CF_BITMAP = 2;
}
