
using System.Windows;
using System.Windows.Media;

namespace SnowyRiver.WPF;
public static class DependencyObjectExtensionMethods
{
    public static TChild? FindVisualChild<TChild>(this DependencyObject obj) where TChild : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
        {
            var child = VisualTreeHelper.GetChild(obj, i);
            if (child is TChild tChild)
            {
                return tChild;
            }

            var childOfChildren = FindVisualChild<TChild>(child);
            if (childOfChildren != null)
            {
                return childOfChildren;
            }
        }

        return null;
    }
}
