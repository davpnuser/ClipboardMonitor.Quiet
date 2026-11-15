using ClipboardMonitor.Quiet.Enums;
using ClipboardMonitor.Quiet.EventArguments;
using ClipboardMonitor.Quiet.Interfaces;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace ClipboardMonitor.Quiet.ClipboardListenerImp;

public class WindowsClipboardListener : ClipboardListenerBase, IWindowsClipboardListener
{
    #region Imports

    private const string NativeDllName = "ClipboardMonitor.Windows.dll";
    private const CallingConvention CallConv = CallingConvention.Cdecl;

    private delegate void ClipboardChangedCallback();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ClipboardChangedCallbackWithData([MarshalAs(UnmanagedType.LPStr)] string data, int type);

    [DllImport(NativeDllName, CallingConvention = CallConv)]
    private static extern void StartClipboardListener();

    [DllImport(NativeDllName, CallingConvention = CallConv)]
    private static extern void StopClipboardListener();

    [DllImport(NativeDllName, CallingConvention = CallConv)]
    private static extern bool ClearClipboard();

    [DllImport(NativeDllName, CallingConvention = CallConv)]
    private static extern void SetClipboardChangedCallback(ClipboardChangedCallback? callback);

    [DllImport(NativeDllName, CallingConvention = CallConv)]
    private static extern void SetClipboardChangedCallbackWithData(ClipboardChangedCallbackWithData? callback);

    #endregion

    #region Private Fields

    private ClipboardChangedCallback? _clipboardChangedCallbackNoData;
    private ClipboardChangedCallbackWithData? _clipboardChangedCallbackWithData;
    private string? _lastStringData;
    private string? _lastImageHash;

    #endregion

    #region Properties

    public bool VerifyNewImageData { get; set; } = false;

    #endregion

    #region Constructor

    public WindowsClipboardListener()
    {
        SetNotificationType(NotificationType.ChangedWithData);
    }

    #endregion

    #region Overrides

    public override bool ClearClipboardContent()
    {
        return ClearClipboard();
    }

    protected override void SetCallbacksNoData(bool unset = false)
    {
        if (unset)
        {
            SetClipboardChangedCallback(null);
            _clipboardChangedCallbackNoData = null;
        }
        else
        {
            _clipboardChangedCallbackNoData = OnClipboardChangedNoData;
            SetClipboardChangedCallback(_clipboardChangedCallbackNoData);
        }
    }

    protected override void SetCallbacksWithData(bool unset = false)
    {
        if (unset)
        {
            SetClipboardChangedCallbackWithData(null);
            _clipboardChangedCallbackWithData = null;
        }
        else
        {
            _clipboardChangedCallbackWithData = OnClipboardChangedWithData;
            SetClipboardChangedCallbackWithData(_clipboardChangedCallbackWithData);
        }
    }

    protected override async void StartListener()
    {
        StartClipboardListener();
        await Task.CompletedTask;
    }

    protected override void StopListener()
    {
        StopClipboardListener();
    }

    #endregion

    #region Methods

    private void OnClipboardChangedNoData()
    {
        OnClipboardChanged(new ClipboardChangedEventArgs(ClipboardDataType.NONE));
    }

    private void OnClipboardChangedWithData(string data, int type)
    {
        var dataType = (ClipboardDataType)type;

        switch (dataType)
        {
            case ClipboardDataType.TEXT:
                if (IsNewClipboardData(data))
                {
                    OnClipboardChanged(new ClipboardChangedEventArgs(data, ClipboardDataType.TEXT));
                }
                break;

            case ClipboardDataType.FILES:
                if (IsNewClipboardData(data))
                {
                    OnClipboardChanged(new ClipboardChangedEventArgs(data, ClipboardDataType.FILES));
                }
                break;

            case ClipboardDataType.IMAGE:
                if (GetBitmapFromClipboard() is Bitmap bitmap && IsNewImageData(bitmap))
                {
                    OnClipboardChanged(new ClipboardChangedEventArgs(bitmap, ClipboardDataType.IMAGE));

                    _lastStringData = null;
                }
                break;

            case ClipboardDataType.CLEARED:
                Console.WriteLine("Clipboard cleared");
                OnClipboardChanged(new ClipboardChangedEventArgs(ClipboardDataType.CLEARED));
                _lastStringData = null;
                _lastImageHash = null;
                break;
        }
    }

    private bool IsNewClipboardData(string data)
    {
        if (string.IsNullOrEmpty(data) || data == _lastStringData)
            return false;

        _lastStringData = data;

        if (VerifyNewImageData)
        {
            _lastImageHash = null;
        }

        return true;
    }

    private static Bitmap? GetBitmapFromClipboard()
    {
        if (OperatingSystem.IsWindowsVersionAtLeast(6, 1))
        {
            Bitmap? bitmap = null;

            if (WindowsNativeMethods.IsClipboardFormatAvailable(WindowsNativeMethods.CF_BITMAP))
            {
                if (WindowsNativeMethods.OpenClipboard(IntPtr.Zero))
                {
                    try
                    {
                        var hBitmap = WindowsNativeMethods.GetClipboardData(WindowsNativeMethods.CF_BITMAP);
                        if (hBitmap != IntPtr.Zero)
                        {
                            bitmap = Image.FromHbitmap(hBitmap);
                            WindowsNativeMethods.DeleteObject(hBitmap);
                        }
                    }
                    finally
                    {
                        WindowsNativeMethods.CloseClipboard();
                    }
                }
            }
            return bitmap;
        }

        return null;
    }

    private static string? GetImageHash(Bitmap bmp)
    {
        if (OperatingSystem.IsWindowsVersionAtLeast(6, 1))
        {
            using var ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Png);
            var hash = SHA256.HashData(ms.ToArray());
            return Convert.ToBase64String(hash);
        }

        return null;
    }

    private bool IsNewImageData(Bitmap bmp)
    {
        if (!VerifyNewImageData)
            return true;

        var newHash = GetImageHash(bmp);

        if (_lastImageHash != newHash)
        {
            _lastImageHash = newHash;
            return true;
        }
        else
        {
            return false;
        }
    }

    #endregion
}