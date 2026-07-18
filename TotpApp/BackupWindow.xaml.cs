using System;
using System.Windows;
using TotpApp.Models;
using TotpApp.Services;

namespace TotpApp
{
    /// <summary>
    /// Interaction logic for BackupWindow.xaml
    /// </summary>
    public partial class BackupWindow : Window
    {
        private readonly Account _account;
        private readonly string _otpAuthUri;

        public BackupWindow(Account account)
        {
            InitializeComponent();
            _account = account ?? throw new ArgumentNullException(nameof(account));
            
            // Format labels
            AccountLabel.Text = string.IsNullOrEmpty(_account.Issuer)
                ? _account.AccountName
                : $"{_account.Issuer} ({_account.AccountName})";

            // Generate otpauth URI
            _otpAuthUri = OtpAuthUriParser.ToUriString(_account);
            UriTextBox.Text = _otpAuthUri;

            // Generate QR Code Image
            try
            {
                var qrImage = QrCodeGenerator.GenerateQrCodeImage(_otpAuthUri);
                QrCodeImage.Source = qrImage;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"QRコードの生成に失敗しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(_otpAuthUri);
                MessageBox.Show("URIをクリップボードにコピーしました。", "コピー完了", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"コピーに失敗しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
