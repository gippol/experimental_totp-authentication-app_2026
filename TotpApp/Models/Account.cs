using System;

namespace TotpApp.Models;

/// <summary>
/// 1つのTOTPアカウント（サイト）を表すモデル。
/// Secret以外はUIにそのまま表示される情報。
/// </summary>
public sealed class Account
{
    /// <summary>アプリ内で一意に識別するためのID。</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>サイト名（例: GitHub）。otpauthのissuerに相当。</summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>ユーザー名/メールアドレス。otpauthのaccountNameに相当。</summary>
    public string AccountName { get; set; } = string.Empty;

    /// <summary>Base32エンコードされた秘密鍵。保存時は必ず暗号化した状態で永続化する。</summary>
    public string SecretBase32 { get; set; } = string.Empty;

    /// <summary>コードの桁数。通常は6。</summary>
    public int Digits { get; set; } = 6;

    /// <summary>コードの有効期間（秒）。通常は30。</summary>
    public int PeriodSeconds { get; set; } = 30;

    /// <summary>ハッシュアルゴリズム名（SHA1 / SHA256 / SHA512）。</summary>
    public string Algorithm { get; set; } = "SHA1";
}
