using System.Windows.Media.Animation;
using System.Windows;

namespace SnowyRiver.WPF.Animation;
public class BlinkAnimation : Animatable
{
    /// <summary>
    /// 单例，保持所有闪烁的动画同步
    /// </summary>
    public static readonly BlinkAnimation Instance = new();

    public double BlinkOpacity
    {
        get => (double)GetValue(BlinkOpacityProperty);
        set => SetValue(BlinkOpacityProperty, value);
    }

    // Using a DependencyProperty as the backing store for BlinkOpacity.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty BlinkOpacityProperty =
        DependencyProperty.Register(nameof(BlinkOpacity), typeof(double), typeof(BlinkAnimation), new PropertyMetadata(0.0));



    /// <summary>
    /// 闪烁动画，用于动画同步
    /// </summary>
    public BlinkAnimation()
    {
        try
        {
            var doubleAnimation = new DoubleAnimationUsingKeyFrames
            {
                RepeatBehavior = RepeatBehavior.Forever,
                Duration = TimeSpan.FromSeconds(1.5)
            };

            var frame1 = new DiscreteDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0)));
            var frame2 = new DiscreteDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.75)));

            doubleAnimation.KeyFrames.Add(frame1);
            doubleAnimation.KeyFrames.Add(frame1);

            BeginAnimation(BlinkOpacityProperty, doubleAnimation);
        }
        catch (Exception)
        {
            // ignored
        }
    }

    protected override Freezable CreateInstanceCore()
    {
        return Instance;
    }
}
