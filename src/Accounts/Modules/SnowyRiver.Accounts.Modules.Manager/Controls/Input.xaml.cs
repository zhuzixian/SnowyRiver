using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace SnowyRiver.Accounts.Modules.Manager.Controls
{
    /// <summary>
    /// InputTextBox.xaml 的交互逻辑
    /// </summary>
    public partial class Input: UserControl
    {
        public Input()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TextWidthProperty = DependencyProperty.Register(
            nameof(TextWidth), typeof(GridLength), typeof(Input), new PropertyMetadata(default(GridLength)));

        public GridLength TextWidth
        {
            get => (GridLength)GetValue(TextWidthProperty);
            set => SetValue(TextWidthProperty, value);
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text), typeof(string), typeof(Input), new PropertyMetadata(default(string)));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value), typeof(string), typeof(Input), new PropertyMetadata(default(string)));

        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }


        public static readonly DependencyProperty OptionsProperty = DependencyProperty.Register(
            nameof(Options), typeof(IEnumerable), typeof(Input), new PropertyMetadata(default(IEnumerable?)));

        public IEnumerable? Options
        {
            get => (IEnumerable?)GetValue(OptionsProperty);
            set => SetValue(OptionsProperty, value);
        }
    }
}
