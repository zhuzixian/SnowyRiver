using System.Windows;
using System.Windows.Media;
using MahApps.Metro.Controls;

namespace SnowyRiver.WPF.MaterialDesignInPrism.Windows;
public class MaterialDesignMetroWindow : MetroWindow
{
    public MaterialDesignMetroWindow()
    {
        Style = FindResource("MaterialDesignWindow") as Style;
        TitleForeground = FindResource("MaterialDesign.Brush.Primary.Dark.Foreground") as Brush;
        GlowBrush = FindResource("MahApps.Brushes.Accent") as Brush;
        FontFamily = new FontFamily("Microsoft YaHei");
        WindowTransitionsEnabled = false;
        SetBinding(TitleProperty, "Title");
    }
}
