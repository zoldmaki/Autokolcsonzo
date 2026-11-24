using Autokolcsonzo.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Autokolcsonzo;

public partial class App : Application
{
    public App()
    {
        CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(
            SystemCommands.CloseWindowCommand, (sender, _) => SystemCommands.CloseWindow((Window)sender)));
        EventManager.RegisterClassHandler(typeof(Window), Mouse.MouseDownEvent,
            new MouseButtonEventHandler(ClearEditorFocusOnBackgroundClick));
    }

    private static void ClearEditorFocusOnBackgroundClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left || Keyboard.FocusedElement is not (TextBox or ComboBox)) return;

        // Editors and interactive controls handle their own clicks before this bubbling event reaches the window.
        if (GetWindowSurface((Window)sender) is { Focusable: true } surface)
            surface.Focus();
    }

    private static UIElement? GetWindowSurface(Window window) =>
        window.Template?.FindName("DialogSurface", window) as UIElement ?? window.Content as UIElement;

    internal static void Uzenet(Window owner, string uzenet, string cim)
    {
        var dialog = new Window
        {
            Title = cim,
            Style = (Style)Current.FindResource("DialogWindow"),
            WindowStartupLocation = owner.IsVisible ? WindowStartupLocation.CenterOwner : WindowStartupLocation.CenterScreen
        };
        if (owner.IsVisible) dialog.Owner = owner;

        var gomb = new Button
        {
            Content = "OK",
            IsDefault = true,
            IsCancel = true,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 14, 0, 0),
            Style = (Style)Current.FindResource("PrimaryButton")
        };
        gomb.Click += (_, _) => dialog.DialogResult = true;
        dialog.Content = new StackPanel
        {
            Children = { new TextBlock { Text = uzenet, TextWrapping = TextWrapping.Wrap, LineHeight = 21 }, gomb }
        };
        dialog.Loaded += (_, _) => gomb.Focus();
        ShowModal(dialog);
    }

    internal static bool? ShowModal(Window dialog)
    {
        var owner = dialog.Owner;
        if (owner is not { IsVisible: true }) return dialog.ShowDialog();

        var surface = GetWindowSurface(owner);
        if (surface == null || AdornerLayer.GetAdornerLayer(surface) is not { } layer)
            return dialog.ShowDialog();

        var shade = new ModalShade(surface, (Brush)Current.FindResource("ModalShadeBrush")) { IsHitTestVisible = false };
        layer.Add(shade);
        try
        {
            return dialog.ShowDialog();
        }
        finally
        {
            layer.Remove(shade);
        }
    }

    private void DialogTitle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
            Window.GetWindow((DependencyObject)sender)?.DragMove();
    }

    private sealed class ModalShade(UIElement surface, Brush brush) : Adorner(surface)
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
            var area = new Rect(AdornedElement.RenderSize);
            double radius = AdornedElement is Border border ? border.CornerRadius.TopLeft : 0;
            drawingContext.DrawRoundedRectangle(brush, null, area, radius, radius);
        }
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        using var context = new AutoKolcsonzoContext();
        context.Database.EnsureCreated();
        DbSeeder.Seed(context);
    }
}
