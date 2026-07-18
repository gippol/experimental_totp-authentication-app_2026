using System.Windows;
using TotpApp.Services;
using TotpApp.ViewModels;

namespace TotpApp;

public partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; }

    public MainWindow(
        ITotpService totpService,
        IQrScanService qrScanService,
        ISecureStorageService storageService)
    {
        InitializeComponent();

        ViewModel = new MainViewModel(totpService, qrScanService, storageService);
        DataContext = ViewModel;

        // 保存済みアカウントの読み込みとタイマー開始は非同期のためfire-and-forgetする。
        // (Window生成時点ではawaitできるコンストラクタが無いため)
        _ = ViewModel.InitializeAsync();
    }
}
