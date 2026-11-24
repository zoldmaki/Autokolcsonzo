using System.Windows;
using System.Windows.Media;

namespace Autokolcsonzo.Views;

public partial class TorlesWindow : Window
{
    internal sealed record Sor(string Nev, string Reszlet, IReadOnlyList<Sor>? KapcsolodoSorok = null)
    {
        public int KapcsolodoSorokSzama => KapcsolodoSorok?.Count ?? 0;
    }
    internal sealed record KapcsolatTipus(string EgyesSzam, string TobbesSzam, string Ikon);
    private sealed record ListaKinezet(Geometry Ikon, string? KapcsolatokEgyesSzama, string? KapcsolatokTobbesSzama,
        Geometry? KapcsolatokIkonja);

    internal TorlesWindow(Window owner, string cim, string kerdes, IEnumerable<Sor> adatok, string ikon,
        string? kovetkezmeny = null, KapcsolatTipus? kapcsolat = null, bool blokkolt = false)
    {
        InitializeComponent();
        Owner = owner;
        Title = cim;
        Kerdes.Text = kerdes;
        Adatok.Tag = new ListaKinezet((Geometry)FindResource(ikon), kapcsolat?.EgyesSzam, kapcsolat?.TobbesSzam,
            kapcsolat == null ? null : (Geometry)FindResource(kapcsolat.Ikon));
        Adatok.ItemsSource = adatok.ToList();
        Kovetkezmeny.Text = kovetkezmeny;
        Kovetkezmeny.Visibility = string.IsNullOrEmpty(kovetkezmeny) ? Visibility.Collapsed : Visibility.Visible;
        if (blokkolt)
        {
            MegseGomb.Content = "OK";
            MegseGomb.IsDefault = true;
            MegseGomb.HorizontalAlignment = HorizontalAlignment.Right;
            MegseGomb.Style = (Style)FindResource("PrimaryButton");
            TorlesGomb.Visibility = Visibility.Collapsed;
        }
    }

    private void Torles_Click(object sender, RoutedEventArgs e) => DialogResult = true;
}
