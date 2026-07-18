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

    private void OnTimerTick()
    {
        var now = DateTime.UtcNow;
        foreach (var account in Accounts)
        {
            account.Refresh(now);
        }
    }
}
