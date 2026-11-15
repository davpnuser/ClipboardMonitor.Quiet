using ClipboardMonitor.Quiet.ClipboardObjects;
using ClipboardMonitor.Quiet.Enums;
using ClipboardMonitor.Quiet.EventArguments;
using ClipboardMonitor.Quiet.Interfaces;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace ClipboardMonitor.Quiet.ClipboardListenerImp;

public class LinuxClipboardListener : ClipboardListenerBase, ILinuxClipboardListener
{
    #region Imports

    private const string NativeDllName = "libClipboardMonitor.Linux.so";
    private const CallingConvention CallConv = CallingConvention.Cdecl;

    private delegate void ClipboardChangedCallback();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ClipboardChangedCallbackWithData(IntPtr data, int dataSize, int type);

    [DllImport(NativeDllName, CallingConvention = CallConv)]
    private static extern void StartClipboardListener();

    [DllImport(NativeDllName, CallingConvention = CallConv)]
    private static extern void StopClipboardListener();

    [DllImport(NativeDllName, CallingConvention = CallConv)]
    private static extern void SetClipboardChangedCallback(ClipboardChangedCallback? callback);

    [DllImport(NativeDllName, CallingConvention = CallConv)]
    private static extern void SetClipboardChangedCallbackWithData(ClipboardChangedCallbackWithData? callback);

    #endregion

    #region Private Fields

    private ClipboardChangedCallback? _clipboardChangedCallbackNoData;
    private ClipboardChangedCallbackWithData? _clipboardChangedCallbackWithData;

    #endregion

    #region Constructor

    public LinuxClipboardListener()
    {
        SetNotificationType(NotificationType.ChangedWithData);
    }

    #endregion

    #region Overrides

    public override bool ClearClipboardContent()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "xclip",
                Arguments = "-selection clipboard",
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISPLAY")))
                psi.Environment["DISPLAY"] = ":0"; // why does the code base need to be so stupidly complicated

            using var process = Process.Start(psi);
            process!.StandardInput.Write("");
            process.StandardInput.Flush();
            process.StandardInput.Close();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                OnClipboardChanged(new ClipboardChangedEventArgs(ClipboardDataType.CLEARED));
                return true;
            }
            else
            {
                return false;
            }
        }
        catch { return false; /* do not crash */ }
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

    #region Private Helpers

    private void OnClipboardChangedNoData()
    {
        OnClipboardChanged(new ClipboardChangedEventArgs(ClipboardDataType.NONE));
    }

    private void OnClipboardChangedWithData(IntPtr data, int size, int type)
    {
        var dataType = (ClipboardDataType)type;

        switch (dataType)
        {
            case ClipboardDataType.TEXT:
            case ClipboardDataType.FILES:
                {
                    var buffer = new byte[size];
                    Marshal.Copy(data, buffer, 0, size);
                    var text = Encoding.UTF8.GetString(buffer);

                    if (dataType == ClipboardDataType.TEXT)
                    {
                        OnClipboardChanged(new ClipboardChangedEventArgs(text, ClipboardDataType.TEXT));
                    }
                    else
                    {
                        OnClipboardChanged(new ClipboardChangedEventArgs(text, ClipboardDataType.FILES));
                    }
                    break;
                }

            case ClipboardDataType.IMAGE:
                {
                    var buffer = new byte[size];
                    Marshal.Copy(data, buffer, 0, size);

                    var image = ClipboardImage.FromClipboardData(buffer, size);
                    OnClipboardChanged(new ClipboardChangedEventArgs(image, ClipboardDataType.IMAGE));
                    break;
                }
        }
    }

    #endregion
}
