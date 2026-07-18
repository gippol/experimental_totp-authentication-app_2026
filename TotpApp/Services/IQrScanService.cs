using System.Threading.Tasks;

namespace TotpApp.Services;

public interface IQrScanService
{
    /// <summary>
    /// スクリーンをキャプチャし、QRコードを検出してotpauth:// URI文字列を返す。
    /// 見つからない/デコード不可の場合はnullを返す(例外は投げない)。
    /// </summary>
    string? ScanForOtpAuthUri();
}
