using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace ReceiptSystem.Desktop.Helpers;

/// <summary>
/// Lightweight toast notification — a colored bar that slides in at the top of a Panel,
/// auto-hides after 3 seconds. Three types: Success (green), Info (blue), Error (red).
/// </summary>
public static class ToastHelper
{
    public static void ShowSuccess(Panel parent, string message) => Show(parent, message, "Success");
    public static void ShowInfo(Panel parent, string message) => Show(parent, message, "Info");
    public static void ShowError(Panel parent, string message) => Show(parent, message, "Error");

    private static void Show(Panel parent, string message, string type)
    {
        var bg = type switch
        {
            "Success" => new SolidColorBrush(Color.FromRgb(0x22, 0xC5, 0x5E)),
            "Error" => new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44)),
            _ => new SolidColorBrush(Color.FromRgb(0x66, 0x7E, 0xEA))
        };

        var icon = type switch
        {
            "Success" => "\u2714",
            "Error" => "\u2716",
            _ => "\u2139"
        };

        var border = new Border
        {
            Background = bg,
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(16, 10, 16, 10),
            Margin = new Thickness(20, 10, 20, 0),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Opacity = 0,
            RenderTransform = new TranslateTransform(0, -20),
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = Colors.Black, BlurRadius = 12, Opacity = 0.3, ShadowDepth = 4
            }
        };

        var sp = new StackPanel { Orientation = Orientation.Horizontal, FlowDirection = FlowDirection.RightToLeft };
        sp.Children.Add(new TextBlock
        {
            Text = icon, FontSize = 14, Foreground = Brushes.White, Margin = new Thickness(0, 0, 8, 0),
            VerticalAlignment = VerticalAlignment.Center
        });
        sp.Children.Add(new TextBlock
        {
            Text = message, FontSize = 13, Foreground = Brushes.White, FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center, FontFamily = new FontFamily("Segoe UI")
        });
        border.Child = sp;

        // Insert at top of parent
        if (parent is Grid grid)
        {
            Grid.SetColumnSpan(border, 10);
            Panel.SetZIndex(border, 9999);
            grid.Children.Add(border);
        }
        else
        {
            parent.Children.Insert(0, border);
        }

        // Animate in
        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250));
        var slideIn = new DoubleAnimation(-20, 0, TimeSpan.FromMilliseconds(250));
        border.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        ((TranslateTransform)border.RenderTransform).BeginAnimation(TranslateTransform.YProperty, slideIn);

        // Auto-dismiss after 3 seconds
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        timer.Tick += (s, e) =>
        {
            timer.Stop();
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
            fadeOut.Completed += (_, _) =>
            {
                if (parent is Grid g)
                    g.Children.Remove(border);
                else
                    parent.Children.Remove(border);
            };
            border.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        };
        timer.Start();
    }
}
