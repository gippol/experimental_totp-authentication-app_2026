using System;
using TotpApp.Models;
using TotpApp.Services;

namespace TotpApp.Tests;

/// <summary>
/// 固定のコード/残り秒数を返すフェイク実装。
/// AccountViewModelのRefresh()ロジック(表示更新)だけを、実際のTOTP計算から切り離してテストするために使う。
/// </summary>
internal sealed class FakeTotpService : ITotpService
{
    public string CodeToReturn { get; set; } = "000000";
    public double SecondsRemainingToReturn { get; set; } = 30;

    public string ComputeCode(Account account, DateTime? utcNow = null) => CodeToReturn;

    public double GetSecondsRemaining(Account account, DateTime? utcNow = null) => SecondsRemainingToReturn;
}
