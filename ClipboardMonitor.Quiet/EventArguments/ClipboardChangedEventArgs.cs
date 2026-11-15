using ClipboardMonitor.Quiet.Enums;
using ClipboardMonitor.Quiet.Interfaces;
using System.Drawing;

namespace ClipboardMonitor.Quiet.EventArguments;

public class ClipboardChangedEventArgs(ClipboardDataType type) : EventArgs
{
    public string? ClipboardText { get; }
    public bool IsClipboardTextFilesPath => DataType == ClipboardDataType.FILES;
    public bool IsNotificationOnly => DataType == ClipboardDataType.NONE;
    public ClipboardDataType DataType { get; } = type;
    public IEnumerable<string>? ClipboardFiles => DataType == ClipboardDataType.FILES && !string.IsNullOrWhiteSpace(ClipboardText)
                ? (ClipboardText?.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries))
                : (IEnumerable<string>?)null;

    public Bitmap? BmpClipboardImage { get; }
    public IClipboardImage? ClipboardImage { get; }

    public ClipboardChangedEventArgs(object? clipboardImage, ClipboardDataType type) : this(type)
    {
        if (clipboardImage is Bitmap bitmap)
            BmpClipboardImage = bitmap;
        else if (clipboardImage is IClipboardImage clipImg)
            ClipboardImage = clipImg;
    }

    public ClipboardChangedEventArgs(string clipboardText, ClipboardDataType type) : this(type)
    {
        ClipboardText = clipboardText;
    }
}
