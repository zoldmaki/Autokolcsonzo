using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Autokolcsonzo.Views;

public static class ColumnHeaderSizing
{
    public static readonly DependencyProperty FitCaptionProperty = DependencyProperty.RegisterAttached(
        "FitCaption", typeof(bool), typeof(ColumnHeaderSizing), new PropertyMetadata(false, FitCaptionChanged));

    public static bool GetFitCaption(DependencyObject element) => (bool)element.GetValue(FitCaptionProperty);
    public static void SetFitCaption(DependencyObject element, bool value) => element.SetValue(FitCaptionProperty, value);

    private static void FitCaptionChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
    {
        if (element is not DataGridColumnHeader header) return;
        if ((bool)e.NewValue)
            header.Loaded += Fit;
        else
            header.Loaded -= Fit;
    }

    private static void Fit(object sender, RoutedEventArgs e)
    {
        var header = (DataGridColumnHeader)sender;
        if (header.Column is not { } column || header.Content is not string { Length: > 0 } caption) return;

        header.ApplyTemplate();
        var text = new FormattedText(caption, header.Language.GetEquivalentCulture(), header.FlowDirection,
            new Typeface(header.FontFamily, header.FontStyle, header.FontWeight, header.FontStretch), header.FontSize,
            header.Foreground, null, TextOptions.GetTextFormattingMode(header), VisualTreeHelper.GetDpi(header).PixelsPerDip);
        double width = text.WidthIncludingTrailingWhitespace + header.Padding.Left + header.Padding.Right
            + header.BorderThickness.Left + header.BorderThickness.Right;
        if (header.CanUserSort && header.Template.FindName("SortSlot", header) is FrameworkElement slot)
            width += slot.Width + slot.Margin.Left + slot.Margin.Right;

        column.MinWidth = Math.Min(column.MaxWidth, Math.Max(column.MinWidth, Math.Ceiling(width) + 1));
    }
}
