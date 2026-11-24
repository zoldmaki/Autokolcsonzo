using System.Windows;
using System.Windows.Controls;

namespace Autokolcsonzo.Views;

public sealed class FillWrapPanel : Panel
{
    private const double ColumnGap = 9;

    protected override Size MeasureOverride(Size availableSize)
    {
        double width = 0;
        double height = 0;
        foreach (var row in Rows(availableSize.Width))
        {
            double growth = Growth(availableSize.Width, row.Width, row.End - row.Start);
            double rowHeight = 0;
            for (int i = row.Start; i < row.End; i++)
            {
                UIElement child = InternalChildren[i];
                child.Measure(new Size(ItemMinWidth(child) + growth, availableSize.Height));
                rowHeight = Math.Max(rowHeight, child.DesiredSize.Height);
            }
            width = Math.Max(width, row.Width + growth * (row.End - row.Start));
            height += rowHeight;
        }

        return new Size(width, height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double y = 0;
        foreach (var row in Rows(finalSize.Width))
        {
            double growth = Growth(finalSize.Width, row.Width, row.End - row.Start);
            double rowHeight = 0;
            for (int i = row.Start; i < row.End; i++)
                rowHeight = Math.Max(rowHeight, InternalChildren[i].DesiredSize.Height);

            double x = 0;
            for (int i = row.Start; i < row.End; i++)
            {
                UIElement child = InternalChildren[i];
                double width = ItemMinWidth(child) + growth;
                child.Arrange(new Rect(x, y, width, rowHeight));
                x += width + ColumnGap;
            }
            y += rowHeight;
        }

        return finalSize;
    }

    private IEnumerable<(int Start, int End, double Width)> Rows(double availableWidth)
    {
        int start = 0;
        double width = 0;
        for (int i = 0; i < InternalChildren.Count; i++)
        {
            double itemWidth = ItemMinWidth(InternalChildren[i]);
            if (i > start && width + ColumnGap + itemWidth > availableWidth)
            {
                yield return (start, i, width);
                start = i;
                width = 0;
            }
            width += i == start ? itemWidth : ColumnGap + itemWidth;
        }
        if (start < InternalChildren.Count)
            yield return (start, InternalChildren.Count, width);
    }

    private static double ItemMinWidth(UIElement child) => child is FrameworkElement element ? element.MinWidth : 0;

    private static double Growth(double availableWidth, double rowWidth, int columns) =>
        double.IsFinite(availableWidth) ? Math.Max(0, availableWidth - rowWidth) / columns : 0;
}
