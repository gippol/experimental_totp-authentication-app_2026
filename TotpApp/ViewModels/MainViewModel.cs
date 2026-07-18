using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TotpApp.Models;
using TotpApp.Services;

namespace TotpApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ITotpService _totpService;
    private readonly IQrScanService _qrScanService;
    private readonly ISecureStorageService _storageService;
    private readonly DispatcherTimer _timer;

    public MainViewModel(
        ITotpService totpService,
        IQrScanService qrScanService,
        ISecureStorageService storageService)
    {
        _totpService = totpService;
        _qrScanService = qrScanService;
        _storageService = storageService;

        // 200ms間隔でTick: 1秒間隔より滑らかにプログレスバーを動かすため。
        // コードの再計算自体はTotpServiceが期限切れかどうかを見て自動的に切り替わる。
        _timer = new DispatcherTimer(DispatcherPriority.Normal)
        {
            Interval = TimeSpan.FromMilliseconds(200),
        };
        _timer.Tick += (_, _) => OnTimerTick();
    }

    public ObservableCollection<AccountViewModel> Accounts { get; } = new();

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private bool _isScanning;

    public async Task InitializeAsync()
    {
        var savedAccounts = await _storageService.LoadAsync();
        foreach (var account in savedAccounts)
        {
            Accounts.Add(new AccountViewModel(account, _totpService));
        }

        _timer.Start();
    }

    [RelayCommand]
    private async Task ScanAsync()
    {
        if (IsScanning)
        {
            return;
        }

        IsScanning = true;
        StatusMessage = null;

        try
        {
            var otpAuthUri = _qrScanService.ScanForOtpAuthUri();

            if (!OtpAuthUriParser.TryParse(otpAuthUri, out var account) || account is null)
            {
                StatusMessage = "画面上にQRコードが見つかりませんでした。";
                return;
            }

            if (Accounts.Any(a => a.Account.SecretBase32 == account.SecretBase32))
            {
                StatusMessage = $"「{account.Issuer}」は既に登録済みです。";
                return;
            }

            Accounts.Add(new AccountViewModel(account, _totpService));
            await _storageService.SaveAsync(Accounts.Select(a => a.Account).ToList());
            StatusMessage = $"「{account.Issuer}」を追加しました。";
        }
        finally
        {
            IsScanning = false;
        }
    }

    [RelayCommand]
    private async Task DeleteAccountAsync(AccountViewModel accountViewModel)
    {
        if (accountViewModel == null)
        {
            return;
        }

        var result = MessageBox.Show(
            $"「{accountViewModel.Issuer} ({accountViewModel.AccountName})」を削除しますか？\nこの操作は取り消せません。",
            "アカウントの削除",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            Accounts.Remove(accountViewModel);
            await _storageService.SaveAsync(Accounts.Select(a => a.Account).ToList());
            StatusMessage = $"「{accountViewModel.Issuer}」を削除しました。";
        }
    }

    [RelayCommand]
    private async Task ExportBackupAsync()
    {
        if (Accounts.Count == 0)
        {
            MessageBox.Show("エクスポートするアカウントがありません。", "情報", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            FileName = "totp_backup",
            DefaultExt = ".txt",
            Filter = "Text Documents (.txt)|*.txt|All Files (*.*)|*.*",
            Title = "バックアップファイルの保存"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                var lines = Accounts.Select(a => OtpAuthUriParser.ToUriString(a.Account)).ToArray();
                System.IO.File.WriteAllLines(dialog.FileName, lines, System.Text.Encoding.UTF8);
                StatusMessage = "バックアップを保存しました。";
                MessageBox.Show("バックアップのエクスポートが完了しました。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"バックアップの書き出しに失敗しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private async Task ImportBackupAsync()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            DefaultExt = ".txt",
            Filter = "Text Documents (.txt)|*.txt|All Files (*.*)|*.*",
            Title = "バックアップファイルの読み込み"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                var lines = System.IO.File.ReadAllLines(dialog.FileName, System.Text.Encoding.UTF8);
                int importedCount = 0;
                int skippedCount = 0;

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    if (OtpAuthUriParser.TryParse(line, out var account) && account != null)
                    {
                        if (Accounts.Any(a => a.Account.SecretBase32 == account.SecretBase32))
                        {
                            skippedCount++;
                            continue;
                        }

                        Accounts.Add(new AccountViewModel(account, _totpService));
                        importedCount++;
                    }
                }

                if (importedCount > 0)
                {
                    await _storageService.SaveAsync(Accounts.Select(a => a.Account).ToList());
                }

                StatusMessage = $"{importedCount} 件のアカウントを復元しました。(重複スキップ: {skippedCount} 件)";
                MessageBox.Show($"バックアップの読み込みが完了しました。\n復元: {importedCount} 件\nスキップ: {skippedCount} 件", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"バックアップの読み込みに失敗しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void OnTimerTick()
    {
        var now = DateTime.UtcNow;
        foreach (var account in Accounts)
        {
            account.Refresh(now);
        }
    }
}
