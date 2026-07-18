using System;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace TotpApp
{
    /// <summary>
    /// Interaction logic for SelectionWindow.xaml
    /// </summary>
    public partial class SelectionWindow : Window
    {
        private System.Windows.Point Start;
        private System.Windows.Shapes.Rectangle RangeRect;

        public int X { get; private set; }
        public int Y { get; private set; }
        public int W { get; private set; }
        public int H { get; private set; }

        public SelectionWindow()
        {
            InitializeComponent();
            RangeRect = SelectionRect;

            MouseDown += OnMouseDown;
            MouseMove += OnMouseMove;
            MouseUp += OnMouseUp;
            KeyDown += (_, e) => { if (e.Key == Key.Escape) Close(); };
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Start = e.GetPosition(this);
            Canvas.SetLeft(RangeRect, Start.X);
            Canvas.SetTop(RangeRect, Start.Y);
            RangeRect.Width = 0;
            RangeRect.Height = 0;
            RangeRect.Visibility = Visibility.Visible;
        }

        private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var pos = e.GetPosition(this);

            var startScreen = this.PointToScreen(Start);
            var posScreen = this.PointToScreen(pos);

            // 座標表示（常に更新）
            CoordText.Text = $"X:{(int)posScreen.X}  Y:{(int)posScreen.Y}";
            Canvas.SetLeft(CoordText, pos.X);
            Canvas.SetTop(CoordText, pos.Y);
            CoordText.Visibility = Visibility.Visible;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                double x = Math.Min(pos.X, Start.X);
                double y = Math.Min(pos.Y, Start.Y);
                double w = Math.Abs(pos.X - Start.X);
                double h = Math.Abs(pos.Y - Start.Y);

                Canvas.SetLeft(RangeRect, x);
                Canvas.SetTop(RangeRect, y);
                RangeRect.Width = w;
                RangeRect.Height = h;

                // 範囲サイズも表示
                CoordText.Text += $"  W:{(int)w} H:{(int)h}";
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            var end = e.GetPosition(this);

            var startScreen = this.PointToScreen(Start);
            var endScreen = this.PointToScreen(end);

            double x = Math.Min(startScreen.X, endScreen.X);
            double y = Math.Min(startScreen.Y, endScreen.Y);
            double w = Math.Abs(startScreen.X - endScreen.X);
            double h = Math.Abs(startScreen.Y - endScreen.Y);

            if (w < 1 || h < 1) { Close(); return; }

            this.X = (int)x;
            this.Y = (int)y;
            this.W = (int)w;
            this.H = (int)h;

            DialogResult = true;
            Close();
        }
    }
}

