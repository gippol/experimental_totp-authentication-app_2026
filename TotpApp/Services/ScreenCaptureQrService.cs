using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using ZXing;

namespace TotpApp.Services;

/// <summary>
/// GDIのBitBltで仮想スクリーン全体(マルチディスプレイ分すべて)をキャプチャし、
/// ZXing.NetでQRコード(otpauth:// URI)を検出する。
/// </summary>
public sealed class ScreenCaptureQrService : IQrScanService
{
    private readonly ISelectionService _selectionService;

    public ScreenCaptureQrService(ISelectionService selectionService)
    {
        _selectionService = selectionService;
    }

    private const int SM_XVIRTUALSCREEN = 76;
    private const int SM_YVIRTUALSCREEN = 77;
    private const int SM_CXVIRTUALSCREEN = 78;
    private const int SM_CYVIRTUALSCREEN = 79;

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    public string? ScanForOtpAuthUri()
    {
        var area = _selectionService.SelectArea();
        if (area.HasValue)
        {
            var rect = area.Value;
            var screenshot = CaptureArea(rect.X, rect.Y, rect.Width, rect.Height);

            try
            {
                var reader = new BarcodeReader();
                var result = reader.Decode(screenshot);

                // デバッグ表示
                //this.DebugImage.Source = BitmapUtil.ConvertBitmap(bmp);
                screenshot.Save("debug.png", System.Drawing.Imaging.ImageFormat.Png);

                // QR解析
                return result?.Text;
            }
            finally
            {
                screenshot.Dispose();
            }
        }

        return null;
    }

    private Bitmap CaptureArea(int x, int y, int width, int height)
    {
        var bmp = new Bitmap(width, height);

        using (var g = Graphics.FromImage(bmp))
        {
            g.CopyFromScreen(x, y, 0, 0, new System.Drawing.Size(width, height));
        }

        return bmp;
    }

    private static Bitmap CaptureVirtualScreen()
    {
        var x = GetSystemMetrics(SM_XVIRTUALSCREEN);
        var y = GetSystemMetrics(SM_YVIRTUALSCREEN);
        var width = GetSystemMetrics(SM_CXVIRTUALSCREEN);
        var height = GetSystemMetrics(SM_CYVIRTUALSCREEN);

        var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.CopyFromScreen(x, y, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);

        return bitmap;
    }
}
