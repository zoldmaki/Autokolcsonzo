using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Autokolcsonzo.Views;

public enum ColumnPreviewState
{
    None,
    Shifted,
    Dragged
}

public static class ColumnDragFeedback
{
    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled", typeof(bool), typeof(ColumnDragFeedback), new PropertyMetadata(false, IsEnabledChanged));

    public static readonly DependencyProperty TranslationProperty = DependencyProperty.RegisterAttached(
        "Translation", typeof(Transform), typeof(ColumnDragFeedback), new PropertyMetadata(Transform.Identity));

    public static readonly DependencyProperty StateProperty = DependencyProperty.RegisterAttached(
        "State", typeof(ColumnPreviewState), typeof(ColumnDragFeedback), new PropertyMetadata(ColumnPreviewState.None));

    public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(DependencyObject element, bool value) => element.SetValue(IsEnabledProperty, value);
    public static Transform GetTranslation(DependencyObject element) => (Transform)element.GetValue(TranslationProperty);
    public static ColumnPreviewState GetState(DependencyObject element) => (ColumnPreviewState)element.GetValue(StateProperty);

    private static void IsEnabledChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
    {
        if (element is not DataGrid grid) return;
        if ((bool)e.NewValue)
            grid.ColumnReordering += PreparePreview;
        else
            grid.ColumnReordering -= PreparePreview;
    }

    private static void PreparePreview(object? sender, DataGridColumnReorderingEventArgs e)
    {
        if (sender is not DataGrid grid || e.Cancel || Mouse.Captured is not DataGridColumnHeader source || source.Column != e.Column ||
            e.DropLocationIndicator is not { } marker) return;

        var columns = grid.Columns.Where(column => column.Visibility == Visibility.Visible && !column.IsFrozen)
            .OrderBy(column => column.DisplayIndex).ToArray();
        int sourceIndex = Array.IndexOf(columns, e.Column);
        if (sourceIndex < 0) return;
        var translations = new TranslateTransform[columns.Length];
        var targets = new double[columns.Length];
        var animations = new DoubleAnimation?[columns.Length];
        for (int index = 0; index < columns.Length; index++)
        {
            translations[index] = new TranslateTransform();
            columns[index].SetValue(TranslationProperty, translations[index]);
        }

        var scroll = grid.Template.FindName("DG_ScrollViewer", grid) as ScrollViewer;
        var layer = AdornerLayer.GetAdornerLayer(grid);
        ScrolledColumnPreview? scrolledPreview = null;

        // WPF still determines and applies the drop. The translations are visual only.
        marker.Opacity = 0;
        if (e.DragIndicator is { } indicator) indicator.Opacity = 0;

        void UpdatePreview(object? _, EventArgs args)
        {
            if (VisualTreeHelper.GetParent(marker) is not DataGridColumnHeadersPresenter presenter ||
                !presenter.IsAncestorOf(source)) return;
            if (!marker.IsVisible)
            {
                ClearPreview();
                return;
            }

            var boundaries = new double[columns.Length + 1];
            for (int index = 0; index < columns.Length; index++)
            {
                var header = presenter.ItemContainerGenerator.ContainerFromIndex(grid.Columns.IndexOf(columns[index])) as DataGridColumnHeader;
                boundaries[index + 1] = boundaries[index] + (header?.ActualWidth ?? columns[index].ActualWidth);
            }

            // Translate the native gap into the coordinates of the unshifted columns.
            double gap = marker.TranslatePoint(new Point(marker.ActualWidth / 2, 0), presenter).X;
            gap -= source.TranslatePoint(new Point(), presenter).X - boundaries[sourceIndex];
            int insertion = 0;
            while (insertion < columns.Length && gap > (boundaries[insertion] + boundaries[insertion + 1]) / 2)
                insertion++;

            int targetIndex = insertion > sourceIndex ? insertion - 1 : insertion;
            double sourceWidth = boundaries[sourceIndex + 1] - boundaries[sourceIndex];
            double sourceOffset = boundaries[insertion] - boundaries[sourceIndex];
            if (insertion > sourceIndex) sourceOffset -= sourceWidth;

            for (int index = 0; index < columns.Length; index++)
            {
                double offset = 0;
                if (sourceIndex < index && index <= targetIndex) offset = -sourceWidth;
                if (targetIndex <= index && index < sourceIndex) offset = sourceWidth;

                SlideColumn(index, index == sourceIndex ? sourceOffset : offset, index == sourceIndex
                    ? ColumnPreviewState.Dragged : offset != 0 ? ColumnPreviewState.Shifted : ColumnPreviewState.None);
            }

            if (scroll is { HorizontalOffset: > 0 } && layer != null)
            {
                if (scrolledPreview == null)
                {
                    scrolledPreview = new ScrolledColumnPreview(grid, presenter, scroll, source.Column);
                    layer.Add(scrolledPreview);
                }
                scrolledPreview.Update();
            }
            else
                RemoveScrolledPreview();
        }

        void SlideColumn(int index, double offset, ColumnPreviewState state)
        {
            var column = columns[index];
            if (targets[index] == offset)
            {
                if (animations[index] == null) column.SetValue(StateProperty, state);
                return;
            }

            var translation = translations[index];
            double start = translation.X;
            targets[index] = offset;
            animations[index] = null;
            translation.BeginAnimation(TranslateTransform.XProperty, null);
            translation.X = offset;
            if (!SystemParameters.ClientAreaAnimation || Math.Abs(start - offset) < 0.5)
            {
                column.SetValue(StateProperty, state);
                return;
            }

            var slide = new DoubleAnimation(start, offset, TimeSpan.FromMilliseconds(140))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
                FillBehavior = FillBehavior.Stop
            };
            animations[index] = slide;
            column.SetValue(StateProperty, state == ColumnPreviewState.None ? ColumnPreviewState.Shifted : state);
            slide.Completed += (_, _) =>
            {
                if (!ReferenceEquals(animations[index], slide)) return;
                animations[index] = null;
                translation.BeginAnimation(TranslateTransform.XProperty, null);
                column.SetValue(StateProperty, state);
            };
            translation.BeginAnimation(TranslateTransform.XProperty, slide);
        }

        void RemoveScrolledPreview()
        {
            if (scrolledPreview == null) return;
            layer?.Remove(scrolledPreview);
            scrolledPreview = null;
        }

        void ClearPreview()
        {
            RemoveScrolledPreview();
            for (int index = 0; index < columns.Length; index++)
            {
                animations[index] = null;
                targets[index] = 0;
                translations[index].BeginAnimation(TranslateTransform.XProperty, null);
                translations[index].X = 0;
                columns[index].ClearValue(StateProperty);
            }
        }

        void StopPreview()
        {
            marker.LayoutUpdated -= UpdatePreview;
            grid.ColumnHeaderDragCompleted -= OnDragCompleted;
            grid.Unloaded -= OnGridUnloaded;
            ClearPreview();
            foreach (var column in columns)
                column.ClearValue(TranslationProperty);
        }

        void OnDragCompleted(object? _, DragCompletedEventArgs args) => StopPreview();
        void OnGridUnloaded(object? _, RoutedEventArgs args) => StopPreview();

        marker.LayoutUpdated += UpdatePreview;
        grid.ColumnHeaderDragCompleted += OnDragCompleted;
        grid.Unloaded += OnGridUnloaded;
    }

}
