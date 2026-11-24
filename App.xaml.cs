using Autokolcsonzo.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Autokolcsonzo
{
    public partial class App : Application
    {
        public static readonly DependencyProperty AccentProperty = DependencyProperty.RegisterAttached(
            "Accent", typeof(SolidColorBrush), typeof(App),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, AccentChanged));

        public static readonly DependencyProperty AccentTextProperty = DependencyProperty.RegisterAttached(
            "AccentText", typeof(Brush), typeof(App),
            new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.Inherits));

        public static SolidColorBrush? GetAccent(DependencyObject element) =>
            (SolidColorBrush?)element.GetValue(AccentProperty);

        public static void SetAccent(DependencyObject element, SolidColorBrush? value) =>
            element.SetValue(AccentProperty, value);

        public static Brush GetAccentText(DependencyObject element) =>
            (Brush)element.GetValue(AccentTextProperty);

        private static void AccentChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
        {
            var accent = (SolidColorBrush?)e.NewValue;
            if (element is Window or TabItem)
                element.SetValue(AccentTextProperty, accent == null ? Brushes.Black : ContrastText(accent.Color));

            if (element is DatePicker datePicker)
            {
                datePicker.Resources["CalendarViewBackground"] = Current.FindResource("SurfaceBrush");
                datePicker.Resources["CalendarViewBorderBrush"] = Current.FindResource("OutlineBrush");
                datePicker.Resources["CalendarViewForeground"] = Current.FindResource("TextBrush");
                datePicker.Resources["CalendarViewTodayBackground"] = accent ?? Brushes.MediumSeaGreen;
                datePicker.Resources["CalendarViewSelectedBorderBrush"] = accent ?? Brushes.MediumSeaGreen;
                datePicker.Resources["CalendarViewTodayForeground"] = accent == null ? Brushes.Black : ContrastText(accent.Color);
            }
        }

        private static Brush ContrastText(Color color)
        {
            static double Linearize(byte channel)
            {
                double value = channel / 255.0;
                return value <= 0.04045 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);
            }

            double luminance = 0.2126 * Linearize(color.R) + 0.7152 * Linearize(color.G) + 0.0722 * Linearize(color.B);
            return luminance > 0.179 ? Brushes.Black : Brushes.White;
        }

        internal static MessageBoxResult Uzenet(Window owner, string uzenet, string cim,
            MessageBoxButton gombok = MessageBoxButton.OK, MessageBoxImage ikon = MessageBoxImage.None)
        {
            var dialog = new Window
            {
                Owner = owner,
                Title = cim,
                Style = (Style)Current.FindResource("DialogWindow"),
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            BindingOperations.SetBinding(dialog, AccentProperty, new Binding
            {
                Source = owner,
                Path = new PropertyPath("(0)", AccentProperty)
            });

            var szoveg = new TextBlock { Text = uzenet, TextWrapping = TextWrapping.Wrap, LineHeight = 21 };
            var uzenetPanel = new DockPanel();
            if (ikon != MessageBoxImage.None)
            {
                var jel = new TextBlock
                {
                    Text = ikon == MessageBoxImage.Question ? "?" : "!",
                    FontSize = 19,
                    FontWeight = FontWeights.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                jel.SetBinding(TextBlock.ForegroundProperty, new Binding
                {
                    Path = new PropertyPath("(0)", AccentProperty),
                    RelativeSource = RelativeSource.Self
                });
                var ikonKeret = new Border
                {
                    Child = jel,
                    Width = 32,
                    Height = 32,
                    CornerRadius = new CornerRadius(8),
                    Margin = new Thickness(0, 0, 12, 0),
                    VerticalAlignment = VerticalAlignment.Top,
                    Background = (Brush)Current.FindResource("RaisedBrush")
                };
                DockPanel.SetDock(ikonKeret, Dock.Left);
                uzenetPanel.Children.Add(ikonKeret);
            }
            uzenetPanel.Children.Add(szoveg);

            bool megerosites = gombok == MessageBoxButton.YesNo;
            var gombSor = new StackPanel { Style = (Style)Current.FindResource("DialogActions") };
            var bezar = new Button { Content = megerosites ? "Nem" : "OK", IsCancel = true, IsDefault = !megerosites };
            Button alapertelmezettGomb = bezar;
            gombSor.Children.Add(bezar);
            var kiemeltStilus = (Style)Current.FindResource("PrimaryButton");
            if (megerosites)
            {
                var igen = new Button { Content = "Igen", Style = kiemeltStilus, IsDefault = true, Margin = new Thickness(8, 0, 0, 0) };
                igen.Click += (_, _) => dialog.DialogResult = true;
                gombSor.Children.Add(igen);
                alapertelmezettGomb = igen;
            }
            else
            {
                bezar.Style = kiemeltStilus;
            }

            dialog.Content = new StackPanel { Children = { uzenetPanel, gombSor } };
            dialog.Loaded += (_, _) => alapertelmezettGomb.Focus();
            bool elfogadta = dialog.ShowDialog() == true;
            return megerosites ? (elfogadta ? MessageBoxResult.Yes : MessageBoxResult.No) : MessageBoxResult.OK;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            using var context = new AutoKolcsonzoContext();
            context.Database.EnsureCreated();   
            DbSeeder.Seed(context);             
        }
    }

}
