using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using MahApps.Metro.Controls;

namespace SnowyRiver.WPF.MaterialDesignInPrism.Windows;
public class MaterialDesignMetroWindow : MetroWindow
{
    private WindowState _lastNotMinimizedWindowState;

    public MaterialDesignMetroWindow()
    {
        StateChanged += MainWindow_OnStateChanged;
        Style = FindResource("MaterialDesignWindow") as Style;
        TitleForeground = FindResource("MaterialDesign.Brush.Primary.Dark.Foreground") as Brush;
        GlowBrush = FindResource("MahApps.Brushes.Accent") as Brush;
        FontFamily = new FontFamily("Microsoft YaHei");
        WindowTransitionsEnabled = false;
        SetBinding(TitleProperty, "Title");
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        var source = PresentationSource.FromVisual(this) as HwndSource;
        source?.AddHook(WndProc);
    }

    protected virtual IntPtr WndProc(IntPtr hwnd, int msg,
        IntPtr wparam, IntPtr lparam, ref bool handled)
    {
        if (msg == NativeMethods.WM_SHOWME && EnableActivateOnShowMeWindowMessage)
        {
            if (WindowState == WindowState.Minimized)
            {
                WindowState = _lastNotMinimizedWindowState;
            }
            Activate();
        }

        return IntPtr.Zero;
    }

    private void MainWindow_OnStateChanged(object? sender, EventArgs e)
    {
        if (WindowState != WindowState.Minimized)
        {
            _lastNotMinimizedWindowState = WindowState;
        }
    }

    protected virtual bool EnableActivateOnShowMeWindowMessage => false;
}
