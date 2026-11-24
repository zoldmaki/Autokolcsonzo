using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Autokolcsonzo.Views;

public static class SmoothScrolling
{
    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled", typeof(bool), typeof(SmoothScrolling), new PropertyMetadata(false, IsEnabledChanged));

    private static readonly DependencyProperty AnimatedOffsetProperty = DependencyProperty.RegisterAttached(
        "AnimatedOffset", typeof(double), typeof(SmoothScrolling), new PropertyMetadata(0.0, OffsetChanged));

    private static readonly DependencyProperty AnimationProperty = DependencyProperty.RegisterAttached(
        "Animation", typeof(DoubleAnimation), typeof(SmoothScrolling));

    public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(DependencyObject element, bool value) => element.SetValue(IsEnabledProperty, value);

    private static void IsEnabledChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
    {
        if (element is not FrameworkElement control) return;

        if ((bool)e.NewValue)
        {
            control.PreviewMouseWheel += OnWheel;
            control.PreviewMouseLeftButtonDown += OnOtherInput;
            control.PreviewKeyDown += OnOtherInput;
            control.Unloaded += OnOtherInput;
        }
        else
        {
            control.PreviewMouseWheel -= OnWheel;
            control.PreviewMouseLeftButtonDown -= OnOtherInput;
            control.PreviewKeyDown -= OnOtherInput;
            control.Unloaded -= OnOtherInput;
            if (FindScrollViewer(control) is { } viewer) Stop(viewer);
        }
    }

    private static ScrollViewer? FindScrollViewer(object control) => control switch
    {
        ScrollViewer viewer => viewer,
        DataGrid grid => grid.Template?.FindName("DG_ScrollViewer", grid) as ScrollViewer,
        _ => null
    };

    private static void OnWheel(object sender, MouseWheelEventArgs e)
    {
        var viewer = FindScrollViewer(sender);
        if (viewer is not { ScrollableHeight: > 0 } || e.Delta == 0) return;

        e.Handled = true;
        int lines = SystemParameters.WheelScrollLines;
        double itemHeight = sender is DataGrid grid ? grid.RowHeight : ComboItemHeight(viewer);
        double distance = lines < 0 ? viewer.ViewportHeight : lines * itemHeight;
        var previousAnimation = viewer.GetValue(AnimationProperty) as DoubleAnimation;
        double previousTarget = previousAnimation?.To ?? viewer.VerticalOffset;
        double target = Math.Clamp(previousTarget - distance * e.Delta / 120.0, 0, viewer.ScrollableHeight);
        if (target == previousTarget) return;

        if (!SystemParameters.ClientAreaAnimation)
        {
            Stop(viewer);
            viewer.ScrollToVerticalOffset(target);
            return;
        }

        var animation = new DoubleAnimation(viewer.VerticalOffset, target, TimeSpan.FromMilliseconds(125))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        animation.Completed += (_, _) =>
        {
            if (!ReferenceEquals(viewer.GetValue(AnimationProperty), animation)) return;
            Stop(viewer);
            viewer.ScrollToVerticalOffset(target);
        };
        viewer.SetValue(AnimationProperty, animation);
        viewer.BeginAnimation(AnimatedOffsetProperty, animation);
    }

    private static double ComboItemHeight(ScrollViewer viewer) => viewer.TemplatedParent is ComboBox combo &&
        combo.ItemContainerGenerator.ContainerFromIndex(0) is FrameworkElement { ActualHeight: > 0 } item
        ? item.ActualHeight
        : 30;

    private static void OnOtherInput(object sender, RoutedEventArgs e)
    {
        if (FindScrollViewer(sender) is { } viewer) Stop(viewer);
    }

    private static void Stop(ScrollViewer viewer)
    {
        if (viewer.GetValue(AnimationProperty) == null) return;
        viewer.ClearValue(AnimationProperty);
        viewer.BeginAnimation(AnimatedOffsetProperty, null);
    }

    private static void OffsetChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
    {
        var viewer = (ScrollViewer)element;
        if (viewer.GetValue(AnimationProperty) != null)
            viewer.ScrollToVerticalOffset((double)e.NewValue);
    }
}
