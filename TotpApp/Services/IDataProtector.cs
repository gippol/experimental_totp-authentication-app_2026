namespace TotpApp.Services;

/// <summary>
/// バイト列の暗号化/復号を抽象化するインターフェース。
/// 本番実装はDPAPI(Windows専用API)を使うが、単体テストではダミー実装に差し替えられるようにする。
/// </summary>
public interface IDataProtector
{
    byte[] Protect(byte[] plainBytes);
    byte[] Unprotect(byte[] protectedBytes);
}
