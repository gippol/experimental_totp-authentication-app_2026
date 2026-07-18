using System.Security.Cryptography;
using System.Text;

namespace TotpApp.Services;

/// <summary>
/// Windows DPAPIを使った暗号化実装。CurrentUserスコープで保護するため、
/// 復号は同一Windowsユーザーアカウントでのみ可能(=他ユーザーや他PCへファイルをコピーしても読めない)。
/// </summary>
public sealed class DpapiDataProtector : IDataProtector
{
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("TotpApp.SecureStorage.v1");

    public byte[] Protect(byte[] plainBytes) =>
        ProtectedData.Protect(plainBytes, Entropy, DataProtectionScope.CurrentUser);

    public byte[] Unprotect(byte[] protectedBytes) =>
        ProtectedData.Unprotect(protectedBytes, Entropy, DataProtectionScope.CurrentUser);
}
