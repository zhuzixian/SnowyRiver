using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SnowyRiver.WPF.Modules.Splash.Views
{
    /// <summary>
    /// SplashWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoadingWindow : Window
    {
        public LoadingWindow()
        {
            InitializeComponent();

            var logoFileInfo = new FileInfo("./Resources/logo.png");
            if (logoFileInfo.Exists)
            {
                Logo.Source = new BitmapImage(new Uri(logoFileInfo.FullName));
            }
        }
    }
}
