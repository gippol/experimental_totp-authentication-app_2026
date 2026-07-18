using System;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TotpApp.Models;
using TotpApp.Services;

namespace TotpApp.ViewModels;

/// <summary>
/// アカウント一覧の各カードに対応するViewModel。
/// MainViewModelのタイマーからRefresh()が呼ばれ、現在のコードとプログレスバーの状態を更新する。
/// </summary>
public partial class AccountViewModel : ObservableObject
{
    private readonly ITotpService _totpService;

    public AccountViewModel(Account account, ITotpService totpService)
    {
        Account = account;
        _totpService = totpService;
        Refresh(DateTime.UtcNow);
    }

    /// <summary>元データ。SecretBase32等はUIにバインドしない。</summary>
    public Account Account { get; }

    public string Issuer => Account.Issuer;

    public string AccountName => Account.AccountName;

    [ObservableProperty]
    private string _currentCode = string.Empty;

    /// <summary>0.0〜1.0。プログレスバーへ直接バインドする残り時間の割合。</summary>
    [ObservableProperty]
    private double _progressFraction;

    /// <summary>残り5秒以下になったらtrue。プログレスバーを赤くする等の表示切り替えに使う。</summary>
    [ObservableProperty]
    private bool _isExpiringSoon;

    [ObservableProperty]
    private bool _wasRecentlyCopied;

    /// <summary>現在時刻を基準にコードと残り時間表示を更新する。</summary>
    public void Refresh(DateTime utcNow)
    {
        CurrentCode = _totpService.ComputeCode(Account, utcNow);

        var secondsRemaining = _totpService.GetSecondsRemaining(Account, utcNow);
        double fraction = secondsRemaining / Account.PeriodSeconds;
        ProgressFraction = Math.Max(0.0, Math.Min(fraction, 1.0));
        IsExpiringSoon = secondsRemaining <= 5;
    }

    [RelayCommand]
    private async Task Copy()
    {
        Clipboard.SetText(CurrentCode);

        WasRecentlyCopied = true;
        await Task.Delay(TimeSpan.FromSeconds(2));
        WasRecentlyCopied = false;
    }

    [RelayCommand]
    private void ShowBackup()
    {
        var window = new BackupWindow(Account)
        {
            Owner = Application.Current.MainWindow
        };
        window.ShowDialog();
    }
}
