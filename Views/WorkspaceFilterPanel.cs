using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Autokolcsonzo.Views;

public sealed class WorkspaceFilterPanel : HeaderedContentControl
{
    public static RoutedCommand ResetCommand { get; } = new("ResetFilters", typeof(WorkspaceFilterPanel));

    private static readonly DependencyPropertyKey ActiveFilterCountPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ActiveFilterCount), typeof(int), typeof(WorkspaceFilterPanel),
            new PropertyMetadata(0));

    public static readonly DependencyProperty ActiveFilterCountProperty = ActiveFilterCountPropertyKey.DependencyProperty;
    public int ActiveFilterCount => (int)GetValue(ActiveFilterCountProperty);

    private static readonly DependencyPropertyKey HasActiveFiltersPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasActiveFilters), typeof(bool), typeof(WorkspaceFilterPanel),
            new PropertyMetadata(false));

    public static readonly DependencyProperty HasActiveFiltersProperty = HasActiveFiltersPropertyKey.DependencyProperty;
    public bool HasActiveFilters => (bool)GetValue(HasActiveFiltersProperty);

    public event RoutedEventHandler? FiltersReset;
    internal bool IsResetting { get; private set; }

    public WorkspaceFilterPanel()
    {
        CommandBindings.Add(new CommandBinding(ResetCommand, (_, _) => Reset()));
        Loaded += (_, _) => RefreshIndicators();
    }

    internal void RefreshIndicators()
    {
        int advancedCount = Content is DependencyObject content ? Editors(content).Count(IsActive) : 0;
        SetValue(ActiveFilterCountPropertyKey, advancedCount);
        SetValue(HasActiveFiltersPropertyKey, advancedCount > 0 || Header is TextBox { Text.Length: > 0 });
    }

    private void Reset()
    {
        IsResetting = true;
        try
        {
            if (Header is TextBox search)
                search.Clear();

            if (Content is DependencyObject content)
            {
                foreach (Control editor in Editors(content))
                {
                    switch (editor)
                    {
                        case TextBox text:
                            text.Clear();
                            break;
                        case DatePicker date:
                            date.SelectedDate = null;
                            date.Text = "";
                            break;
                        case ComboBox combo:
                            combo.SelectedIndex = combo.HasItems ? 0 : -1;
                            break;
                    }
                }
            }
        }
        finally
        {
            IsResetting = false;
        }

        RefreshIndicators();
        FiltersReset?.Invoke(this, new RoutedEventArgs());
        (Header as TextBox)?.Focus();
    }

    private static bool IsActive(Control editor) => editor switch
    {
        TextBox text => text.Text.Length > 0,
        DatePicker date => date.SelectedDate.HasValue,
        ComboBox combo => combo.SelectedIndex > 0,
        _ => false
    };

    private static IEnumerable<Control> Editors(DependencyObject parent)
    {
        if (parent is TextBox or DatePicker or ComboBox)
        {
            yield return (Control)parent;
            yield break;
        }

        foreach (var child in LogicalTreeHelper.GetChildren(parent).OfType<DependencyObject>())
            foreach (var editor in Editors(child))
                yield return editor;
    }
}
