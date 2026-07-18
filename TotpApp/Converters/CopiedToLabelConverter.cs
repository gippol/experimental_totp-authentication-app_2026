using System;
using System.Globalization;
using System.Windows.Data;

namespace TotpApp.Converters;

/// <summary>WasRecentlyCopied(bool)を「コピー」/「✓ コピー済み」ラベルに変換する。</summary>
public sealed class CopiedToLabelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is bool b && b ? "✓ コピー済み" : "コピー";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
