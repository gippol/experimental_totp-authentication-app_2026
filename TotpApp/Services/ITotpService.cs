using System;
using TotpApp.Models;

namespace TotpApp.Services;

public interface ITotpService
{
    /// <summary>指定時刻(省略時は現在のUTC時刻)における6桁等のコードを計算する。</summary>
    string ComputeCode(Account account, DateTime? utcNow = null);

    /// <summary>現在のPeriod枠が終わるまでの残り秒数(小数)を返す。プログレスバー同期用。</summary>
    double GetSecondsRemaining(Account account, DateTime? utcNow = null);
}
