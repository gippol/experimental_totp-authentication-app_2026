using System;
using System.Web;

namespace TotpApp.Models;

/// <summary>
/// "otpauth://totp/Issuer:AccountName?secret=...&amp;issuer=...&amp;algorithm=...&amp;digits=...&amp;period=..."
/// 形式のURIをAccountに変換する。QRコードスキャンの結果はすべてこのクラスを経由する。
/// UIやWinUI3 APIに依存しないため、単体テストしやすい形にしてある。
/// </summary>
public static class OtpAuthUriParser
{
    /// <summary>
    /// URI文字列のパースを試みる。失敗した場合はfalseを返し、accountはnullになる。
    /// 例外は投げない（QRコードの中身は信頼できない外部入力のため）。
    /// </summary>
    public static bool TryParse(string? uriString, out Account? account)
    {
        account = null;

        if (string.IsNullOrWhiteSpace(uriString))
        {
            return false;
        }

        if (!Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
        {
            return false;
        }

        if (!string.Equals(uri.Scheme, "otpauth", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // "totp" 以外(hotpなど)は今回のアプリでは非対応
        if (!string.Equals(uri.Host, "totp", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var query = HttpUtility.ParseQueryString(uri.Query);
        var secret = query["secret"];
        if (string.IsNullOrWhiteSpace(secret))
        {
            // secretが無いotpauth URIは無効
            return false;
        }

        // パス部分 "/Issuer:AccountName" または "/AccountName" をデコード
        var label = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));
        var (labelIssuer, accountName) = SplitLabel(label);

        // issuerクエリパラメータがあればそちらを優先(otpauth仕様上の推奨)
        var issuer = query["issuer"];
        if (string.IsNullOrWhiteSpace(issuer))
        {
            issuer = labelIssuer;
        }

        var digits = TryParsePositiveInt(query["digits"], defaultValue: 6);
        var period = TryParsePositiveInt(query["period"], defaultValue: 30);
        var algorithm = string.IsNullOrWhiteSpace(query["algorithm"])
            ? "SHA1"
            : query["algorithm"]!.ToUpperInvariant();

        account = new Account
        {
            Issuer = issuer ?? string.Empty,
            AccountName = accountName,
            SecretBase32 = secret!.Replace(" ", string.Empty).ToUpperInvariant(),
            Digits = digits,
            PeriodSeconds = period,
            Algorithm = algorithm,
        };

        return true;
    }

    private static (string Issuer, string AccountName) SplitLabel(string label)
    {
        var separatorIndex = label.IndexOf(':');
        if (separatorIndex < 0)
        {
            return (string.Empty, label);
        }

        var issuer = label.Substring(0, separatorIndex).Trim();
        var accountName = label.Substring(separatorIndex + 1, label.Length - (separatorIndex + 1)).Trim();
        return (issuer, accountName);
    }

    private static int TryParsePositiveInt(string? value, int defaultValue)
    {
        if (int.TryParse(value, out var parsed) && parsed > 0)
        {
            return parsed;
        }

        return defaultValue;
    }

    /// <summary>
    /// Accountからotpauth URI文字列を生成する。
    /// </summary>
    public static string ToUriString(Account account)
    {
        if (account == null)
        {
            throw new ArgumentNullException(nameof(account));
        }

        // ラベル部分のエンコード。Issuerがある場合は "Issuer:AccountName"
        var label = string.IsNullOrEmpty(account.Issuer)
            ? Uri.EscapeDataString(account.AccountName)
            : $"{Uri.EscapeDataString(account.Issuer)}:{Uri.EscapeDataString(account.AccountName)}";

        var query = $"?secret={Uri.EscapeDataString(account.SecretBase32)}";

        if (!string.IsNullOrEmpty(account.Issuer))
        {
            query += $"&issuer={Uri.EscapeDataString(account.Issuer)}";
        }

        query += $"&algorithm={Uri.EscapeDataString(account.Algorithm)}";
        query += $"&digits={account.Digits}";
        query += $"&period={account.PeriodSeconds}";

        return $"otpauth://totp/{label}{query}";
    }
}
