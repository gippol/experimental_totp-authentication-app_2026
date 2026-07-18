using System;
using OtpNet;
using TotpApp.Models;

namespace TotpApp.Services;

/// <summary>
/// Otp.NETをラップし、Accountモデルから直接コードを計算できるようにするサービス。
/// UTC時刻を明示的に引数で受け取れるようにしてあるため、時刻依存のロジックをテストしやすい。
/// </summary>
public sealed class TotpService : ITotpService
{
    public string ComputeCode(Account account, DateTime? utcNow = null)
    {
        if (account == null) throw new ArgumentNullException();

        var totp = CreateTotp(account);
        var timestamp = utcNow ?? DateTime.UtcNow;
        return totp.ComputeTotp(timestamp);
    }

    public double GetSecondsRemaining(Account account, DateTime? utcNow = null)
    {
        if (account == null) throw new ArgumentNullException();

        var totp = CreateTotp(account);
        var timestamp = utcNow ?? DateTime.UtcNow;
        return totp.RemainingSeconds(timestamp);
    }

    private static Totp CreateTotp(Account account)
    {
        var secretBytes = Base32Encoding.ToBytes(account.SecretBase32);
        var algorithm = ParseAlgorithm(account.Algorithm);

        return new Totp(
            secretBytes,
            step: account.PeriodSeconds,
            mode: algorithm,
            totpSize: account.Digits);
    }

    private static OtpHashMode ParseAlgorithm(string algorithm) => algorithm.ToUpperInvariant() switch
    {
        "SHA256" => OtpHashMode.Sha256,
        "SHA512" => OtpHashMode.Sha512,
        _ => OtpHashMode.Sha1,
    };
}
