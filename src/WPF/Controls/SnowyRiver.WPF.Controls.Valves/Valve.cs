using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SnowyRiver.WPF.Controls.Valves
{
    public class Valve : Control
    {
        static Valve()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Valve), new FrameworkPropertyMetadata(typeof(Valve)));
        }

        public static readonly DependencyProperty StateProperty = DependencyProperty.Register(
            "State", typeof(ValveState), typeof(Valve), new PropertyMetadata(default(ValveState)));

        public ValveState State
        {
            get => (ValveState)GetValue(StateProperty);
            set => SetValue(StateProperty, value);
        }

        public static readonly DependencyProperty BlockBrushProperty = DependencyProperty.Register(
            nameof(BlockBrush), typeof(Brush), typeof(Valve), new PropertyMetadata(Brushes.Red));
        public Brush BlockBrush
        {
            get => (Brush)GetValue(BlockBrushProperty);
            set => SetValue(BlockBrushProperty, value);
        }

        public static readonly DependencyProperty UnblockBrushProperty = DependencyProperty.Register(
            nameof(UnblockBrush), typeof(Brush), typeof(Valve), new PropertyMetadata(Brushes.Green));
        public Brush UnblockBrush
        {
            get => (Brush)GetValue(UnblockBrushProperty);
            set => SetValue(UnblockBrushProperty, value);
        }
    }
}
