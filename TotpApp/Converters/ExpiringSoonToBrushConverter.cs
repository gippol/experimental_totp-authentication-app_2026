using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TotpApp.Converters;

/// <summary>IsExpiringSoon(bool)をプログレスバーの色(赤 or 通常色)に変換する。</summary>
public sealed class ExpiringSoonToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var isExpiringSoon = value is bool b && b;
        return isExpiringSoon
            ? new SolidColorBrush(Colors.Red)
            : new SolidColorBrush(Colors.SeaGreen);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
