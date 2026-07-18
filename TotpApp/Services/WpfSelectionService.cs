using System.Drawing;

namespace TotpApp.Services;

/// <summary>
/// WPFのSelectionWindowを使用して画面領域の選択を行うサービス。
/// </summary>
public sealed class WpfSelectionService : ISelectionService
{
    public Rectangle? SelectArea()
    {
        var selector = new SelectionWindow();
        if (selector.ShowDialog() == true)
        {
            return new Rectangle(selector.X, selector.Y, selector.W, selector.H);
        }
        return null;
    }
}
