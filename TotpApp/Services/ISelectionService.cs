using System.Drawing;

namespace TotpApp.Services;

/// <summary>
/// 画面の特定領域を選択するためのインターフェース。
/// </summary>
public interface ISelectionService
{
    /// <summary>
    /// ユーザーに画面範囲を選択させ、その領域を返します。
    /// キャンセルされた場合は null を返します。
    /// </summary>
    Rectangle? SelectArea();
}
