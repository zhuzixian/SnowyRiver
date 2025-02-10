using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SnowyRiver.Accounts.Modules.Manager.Controls
{
    /// <summary>
    /// Login.xaml 的交互逻辑
    /// </summary>
    public partial class Login : UserControl
    {
        public Login()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            nameof(Message), typeof(string), typeof(Login), new PropertyMetadata(default(string)));

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public static readonly DependencyProperty UserNameProperty = DependencyProperty.Register(
            nameof(UserName), typeof(string), typeof(Login), new PropertyMetadata(default(string)));

        public string UserName
        {
            get => (string)GetValue(UserNameProperty);
            set => SetValue(UserNameProperty, value);
        }

        public static readonly DependencyProperty PasswordProperty = DependencyProperty.Register(
            nameof(Password), typeof(string), typeof(Login), new PropertyMetadata(default(string)));

        public string Password
        {
            get => (string)GetValue(PasswordProperty);
            set => SetValue(PasswordProperty, value);
        }

        public static readonly DependencyProperty RememberMeProperty = DependencyProperty.Register(
            nameof(RememberMe), typeof(bool), typeof(Login), new PropertyMetadata(false));

        public bool RememberMe
        {
            get => (bool)GetValue(RememberMeProperty);
            set => SetValue(RememberMeProperty, value);
        }

        public static readonly DependencyProperty ConfirmCommandProperty = DependencyProperty.Register(
            nameof(ConfirmCommand), typeof(ICommand), typeof(Login), new PropertyMetadata(default(ICommand)));

        public ICommand ConfirmCommand
        {
            get => (ICommand)GetValue(ConfirmCommandProperty);
            set => SetValue(ConfirmCommandProperty, value);
        }

        public static readonly DependencyProperty CancelCommandProperty = DependencyProperty.Register(
            nameof(CancelCommand), typeof(ICommand), typeof(Login), new PropertyMetadata(default(ICommand)));

        public ICommand CancelCommand
        {
            get => (ICommand)GetValue(CancelCommandProperty);
            set => SetValue(CancelCommandProperty, value);
        }
    }
}
