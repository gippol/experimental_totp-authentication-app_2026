using System.Windows;
using TotpApp.Services;

namespace TotpApp;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 今回はDIコンテナを使わず、依存を素朴にnewして手渡す構成にしてある。
        // (将来的にMicrosoft.Extensions.DependencyInjectionへ差し替えても、
        //  各クラスはインターフェース越しにしか依存していないため影響範囲は小さい)
        var totpService = new TotpService();
        var selectionService = new WpfSelectionService();
        var qrScanService = new ScreenCaptureQrService(selectionService);
        var storageService = new SecureStorageService();

        var window = new MainWindow(totpService, qrScanService, storageService);
        window.Show();
    }
}
