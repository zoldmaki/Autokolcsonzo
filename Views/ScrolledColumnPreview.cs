using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;

namespace Autokolcsonzo.Views;

internal sealed class ScrolledColumnPreview : Adorner
{
    private readonly DataGrid grid;
    private readonly DataGridColumnHeadersPresenter headers;
    private readonly ScrollViewer scroll;
    private readonly DataGridColumn draggedColumn;
    private readonly DrawingVisual picture = new();
    private (double X, double Y, Size Size)? previousView;

    internal ScrolledColumnPreview(DataGrid grid, DataGridColumnHeadersPresenter headers, ScrollViewer scroll,
        DataGridColumn draggedColumn) : base(grid)
    {
        this.grid = grid;
        this.headers = headers;
        this.scroll = scroll;
        this.draggedColumn = draggedColumn;
        IsHitTestVisible = false;
        AddVisualChild(picture);
    }

    protected override int VisualChildrenCount => 1;

    protected override Visual GetVisualChild(int index) => index == 0 ? picture : throw new ArgumentOutOfRangeException(nameof(index));

    internal void Update()
    {
        var view = (scroll.HorizontalOffset, scroll.VerticalOffset, grid.RenderSize);
        if (previousView == view || scroll.Template.FindName("PART_ScrollContentPresenter", scroll) is not FrameworkElement body)
            return;
        previousView = view;

        // WPF clips the original header and cells at the frozen column. Draw the preview
        // from their inner visuals, then clip at the frozen boundary in the projected order.
        var headerArea = new Rect(headers.TranslatePoint(new Point(), grid), headers.RenderSize);
        var bodyArea = new Rect(body.TranslatePoint(new Point(), grid), body.RenderSize);
        var columnHeaders = Children<DataGridColumnHeader>(headers).Where(header => header.Column != null).ToArray();
        double pinnedWidth = columnHeaders.Where(header => header.IsFrozen).Sum(header => header.ActualWidth);
        var canvas = (Brush)grid.FindResource("CanvasBrush");
        using var drawing = picture.RenderOpen();
        drawing.DrawRectangle(canvas, null, headerArea);
        DrawColumns(drawing, headerArea, pinnedWidth, canvas, columnHeaders.Select(header =>
            ((Control)header, header.Column, header.Template.FindName("HeaderFrame", header) as FrameworkElement)));

        drawing.PushClip(new RectangleGeometry(bodyArea));
        drawing.DrawRectangle(canvas, null, bodyArea);
        var rowLine = new Pen(grid.HorizontalGridLinesBrush, 1);
        foreach (var row in Children<DataGridRow>(body))
        {
            var rowBounds = new Rect(row.TranslatePoint(new Point(), grid), new Size(bodyArea.Width, row.ActualHeight));
            rowBounds.X = bodyArea.Left;
            if (!rowBounds.IntersectsWith(bodyArea)) continue;

            Brush background = row.Background ?? canvas;
            drawing.DrawRectangle(background, null, rowBounds);
            DrawColumns(drawing, rowBounds, pinnedWidth, background, Children<DataGridCell>(row).Select(cell =>
                ((Control)cell, cell.Column, cell.Template.FindName("CellVisual", cell) as FrameworkElement)));
            double y = rowBounds.Bottom - 0.5;
            drawing.DrawLine(rowLine, new Point(rowBounds.Left, y), new Point(rowBounds.Right, y));
        }
        drawing.Pop();
    }

    private void DrawColumns(DrawingContext drawing, Rect viewport, double pinnedWidth, Brush background,
        IEnumerable<(Control Container, DataGridColumn Column, FrameworkElement? Frame)> columns)
    {
        foreach (var (container, column, frame) in columns.OrderBy(column => column.Column == draggedColumn))
        {
            if (frame == null) continue;
            var area = viewport;
            if (column.IsFrozen) area.Width = Math.Min(viewport.Width, pinnedWidth);
            else
            {
                area.X += pinnedWidth;
                area.Width = Math.Max(0, viewport.Width - pinnedWidth);
            }
            Vector offset = VisualTreeHelper.GetOffset(frame);
            var position = container.TranslatePoint((Point)offset, grid);
            var destination = new Rect(position, frame.RenderSize);

            var brush = new VisualBrush(frame)
            {
                AutoLayoutContent = false,
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect((Point)offset, frame.RenderSize)
            };
            drawing.PushClip(new RectangleGeometry(area));
            drawing.PushTransform(ColumnDragFeedback.GetTranslation(column));
            drawing.DrawRectangle(background, null, destination);
            drawing.DrawRectangle(brush, null, destination);
            drawing.Pop();
            drawing.Pop();
        }
    }

    private static IEnumerable<T> Children<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            var child = VisualTreeHelper.GetChild(parent, index);
            if (child is T match) yield return match;
            else
                foreach (var matchInChild in Children<T>(child))
                    yield return matchInChild;
        }
    }
}
