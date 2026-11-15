using ClipboardMonitor.Quiet.Helpers;
using ClipboardMonitor.Quiet.Interfaces;

namespace ClipboardMonitor.Quiet.ClipboardObjects;

public class ClipboardImage(byte[] data, string format) : IClipboardImage
{
    #region Properties

    public byte[] Data { get; } = data;
    public string Format { get; } = format;

    #endregion

    #region Methods

    public virtual void Save(string path)
    {
        if (!Path.GetExtension(path).Equals($".{Format.ToLower()}", StringComparison.CurrentCultureIgnoreCase))
            Path.ChangeExtension(path, Format);

        File.WriteAllBytes(path, Data);
    }

    public static ClipboardImage FromClipboardData(byte[] data, int size)
    {
        try
        {
            if (data == null || size <= 0 || size > data.Length)
                throw new ArgumentException("Invalid data or size.");

            var imageData = new byte[size];
            Array.Copy(data, 0, imageData, 0, size);

            var format = ImageHelper.DetectImageFormat(imageData);

            return new ClipboardImage(imageData, format);
        }
        catch { /* do not crash */ }

        return null!;
    }

    #endregion
}
