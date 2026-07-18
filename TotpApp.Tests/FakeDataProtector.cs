using TotpApp.Services;

namespace TotpApp.Tests;

/// <summary>
/// 実際のDPAPI呼び出しを避けるためのテスト用ダミー実装。
/// 暗号化はせずバイト列をそのまま返すだけなので、
/// SecureStorageServiceのJSON変換ロジックだけを切り出してテストできる。
/// </summary>
internal sealed class FakeDataProtector : IDataProtector
{
    public byte[] Protect(byte[] plainBytes) => plainBytes;

    public byte[] Unprotect(byte[] protectedBytes) => protectedBytes;
}
