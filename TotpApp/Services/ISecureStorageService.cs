using System.Collections.Generic;
using System.Threading.Tasks;
using TotpApp.Models;

namespace TotpApp.Services;

public interface ISecureStorageService
{
    /// <summary>DPAPIで復号したアカウント一覧を読み込む。ファイルが無ければ空リストを返す。</summary>
    Task<List<Account>> LoadAsync();

    /// <summary>アカウント一覧をJSONシリアライズしDPAPIで暗号化して保存する。</summary>
    Task SaveAsync(IReadOnlyList<Account> accounts);
}
