using System;
using System.Windows;
using MahApps.Metro.Controls;

namespace SnowyRiver.WPF.Modules.Splash.Views
{
    /// <summary>
    /// Interaction logic for SplashWindow.xaml
    /// </summary>
    public partial class SplashView
    {
        private const double StartupWindowWidth = 600;
        private const double StartupWindowHeight = 371;

        public SplashView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // 启动阶段以“无标题栏、居中、600x371”小窗呈现，与改造前独立对话框观感一致。
            // 窗口态的恢复（进入主界面后最大化）由宿主在完成回调中负责，这里不做还原，避免竞态。
            var window = Window.GetWindow(this);
            if (window is null)
            {
                return;
            }

            ApplyStartupWindowState(window);
        }

        private static void ApplyStartupWindowState(Window window)
        {
            if (window is MetroWindow metroWindow)
            {
                metroWindow.ShowTitleBar = false;
                metroWindow.ShowMinButton = false;
                metroWindow.ShowMaxRestoreButton = false;
                metroWindow.ShowCloseButton = false;
            }

            window.WindowStyle = WindowStyle.None;
            window.ResizeMode = ResizeMode.NoResize;
            window.WindowState = WindowState.Normal;
            window.Width = StartupWindowWidth;
            window.Height = StartupWindowHeight;
            CenterOnScreen(window);
        }

        private static void CenterOnScreen(Window window)
        {
            var screen = SystemParameters.WorkArea;
            window.Left = screen.Left + (screen.Width - window.Width) / 2;
            window.Top = screen.Top + (screen.Height - window.Height) / 2;
        }
    }
}
