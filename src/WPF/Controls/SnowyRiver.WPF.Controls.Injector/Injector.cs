﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace SnowyRiver.WPF.Controls.Injector
{
    /// <summary>
    /// 按照步骤 1a 或 1b 操作，然后执行步骤 2 以在 XAML 文件中使用此自定义控件。
    ///
    /// 步骤 1a) 在当前项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:FE.Modules.Valves.Themes"
    ///
    ///
    /// 步骤 1b) 在其他项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:FE.Modules.Valves.Themes;assembly=FE.Modules.Valves.Themes"
    ///
    /// 您还需要添加一个从 XAML 文件所在的项目到此项目的项目引用，
    /// 并重新生成以避免编译错误:
    ///
    ///     在解决方案资源管理器中右击目标项目，然后依次单击
    ///     “添加引用”->“项目”->[浏览查找并选择此项目]
    ///
    ///
    /// 步骤 2)
    /// 继续操作并在 XAML 文件中使用控件。
    ///
    ///     <MyNamespace:InjectionPump/>
    ///
    /// </summary>
    public class Injector : Slider
    {
        static Injector()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Injector), new FrameworkPropertyMetadata(typeof(Injector)));
        }


        public static readonly DependencyProperty PistonHeightProperty = DependencyProperty.Register(
            nameof(PistonHeight), typeof(double), typeof(Injector), new PropertyMetadata(default(double)));
        public double PistonHeight
        {
            get => (double)GetValue(PistonHeightProperty);
            set => SetValue(PistonHeightProperty, value);
        }

        public static readonly DependencyProperty PistonBrushProperty = DependencyProperty.Register(
            nameof(PistonBrush), typeof(Brush), typeof(Injector), new PropertyMetadata(Brushes.Black));
        public Brush PistonBrush
        {
            get => (Brush)GetValue(PistonBrushProperty);
            set => SetValue(PistonBrushProperty, value);
        }

        public static readonly DependencyProperty PlungerBrushProperty = DependencyProperty.Register(
            nameof(PlungerBrush), typeof(Brush), typeof(Injector), new PropertyMetadata(Brushes.Black));
        public Brush PlungerBrush
        {
            get => (Brush)GetValue(PlungerBrushProperty);
            set => SetValue(PlungerBrushProperty, value);
        }
    }
}
