using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using TotpApp.Models;

namespace TotpApp.Services;

/// <summary>
/// アカウント一覧をJSONシリアライズし、IDataProtectorで暗号化した状態でファイルに保存する。
/// ファイルI/OとJSON変換ロジックを分離しているため、シリアライズ部分は単体テストしやすい。
/// </summary>
public sealed class SecureStorageService : ISecureStorageService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
    };

    private readonly string _filePath;
    private readonly IDataProtector _protector;

    public SecureStorageService(string filePath, IDataProtector protector)
    {
        _filePath = filePath;
        _protector = protector;
    }

    /// <summary>
    /// %LocalAppData%\TotpApp\accounts.dat を使う既定コンストラクタ。
    /// </summary>
    public SecureStorageService()
        : this(
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TotpApp",
                "accounts.dat"),
            new DpapiDataProtector())
    {
    }

    public async Task<List<Account>> LoadAsync()
    {
        if (!File.Exists(_filePath))
        {
            return new List<Account>();
        }

        var protectedBytes = await ReadAllBytesAsync(_filePath).ConfigureAwait(false);
        return DeserializeAccounts(protectedBytes, _protector);
    }

    public async Task SaveAsync(IReadOnlyList<Account> accounts)
    {
        if (accounts is null)
        {
            throw new ArgumentNullException(nameof(accounts));
        }

        var protectedBytes = SerializeAccounts(accounts, _protector);

        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await WriteAllBytesAsync(_filePath, protectedBytes).ConfigureAwait(false);
    }

    /// <summary>
    /// アカウント一覧 -> JSON -> 暗号化バイト列。
    /// ファイルI/Oを介さないため単体テストで直接検証できる。
    /// </summary>
    internal static byte[] SerializeAccounts(IReadOnlyList<Account> accounts, IDataProtector protector)
    {
        var plainBytes = JsonSerializer.SerializeToUtf8Bytes(accounts, JsonOptions);
        return protector.Protect(plainBytes);
    }

    /// <summary>
    /// 暗号化バイト列 -> 復号 -> JSON -> アカウント一覧。
    /// ファイルI/Oを介さないため単体テストで直接検証できる。
    /// </summary>
    internal static List<Account> DeserializeAccounts(byte[] protectedBytes, IDataProtector protector)
    {
        var plainBytes = protector.Unprotect(protectedBytes);
        return JsonSerializer.Deserialize<List<Account>>(plainBytes, JsonOptions) ?? new List<Account>();
    }

    // .NET Framework の File クラスには ReadAllBytesAsync/WriteAllBytesAsync が無いため自前で用意する
    private static Task<byte[]> ReadAllBytesAsync(string path) =>
        Task.Run(() => File.ReadAllBytes(path));

    private static Task WriteAllBytesAsync(string path, byte[] bytes) =>
        Task.Run(() => File.WriteAllBytes(path, bytes));
}
