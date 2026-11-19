using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Autokolcsonzo.Data;
using Autokolcsonzo.Models;
using Autokolcsonzo.Views;
using Microsoft.EntityFrameworkCore;

namespace Autokolcsonzo;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        TipusUzemanyagFilterComboBox.ItemsSource = new[] { "Összes" }.Concat(Tipus.Uzemanyagok).ToList();
        TipusUzemanyagFilterComboBox.SelectedIndex = 0;

        VarosokBetoltese();
        TipusokBetoltese();
        AutokBetoltese();
        BerlokBetoltese();
    }

    private void FoTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.OriginalSource != sender || !IsLoaded) return;

        VarosokBetoltese();
        TipusokBetoltese();
        AutokBetoltese();
        BerlokBetoltese();
    }

    private bool MentesBiztonsagosan(AutoKolcsonzoContext context, string mit)
    {
        try
        {
            context.SaveChanges();
            return true;
        }
        catch (Exception ex)
        {
            string uzenet = $"Hiba történt a(z) {mit} mentésekor:\n{ex.Message}";
            if (ex.InnerException != null)
                uzenet += $"\n\nRészletek: {ex.InnerException.Message}";   // one box instead of two
            MessageBox.Show(uzenet, "Mentési hiba");
            return false;
        }
    }

    private static bool SorraKattintottak(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not DependencyObject forras) return false;
        return ItemsControl.ContainerFromElement((DataGrid)sender, forras) is DataGridRow;
    }

    // ---------- VÁROSOK ----------

    private void VarosokBetoltese()
    {
        using var context = new AutoKolcsonzoContext();
        string szuroVaros = VarosNevFilterTextBox.Text.Trim();
        string szuroIranySzam = VarosIranySzamFilterTextBox.Text.Trim();
        string szuroMegye = VarosMegyeFilterTextBox.Text.Trim();

        IQueryable<Varos> lekerdezes = context.Varosok.Include(v => v.Berlok);

        lekerdezes = lekerdezes.Where(v => (szuroVaros != "" ? v.Nev.Contains(szuroVaros) : true)
                                        && (szuroIranySzam != "" ? v.IranySzam.Contains(szuroIranySzam) : true)
                                        && (szuroMegye != "" ? v.Megye.Contains(szuroMegye) : true));

        VarosDataGrid.ItemsSource = lekerdezes.OrderBy(v => v.VarosId).ToList();
    }

    private void VarosFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        VarosokBetoltese();
    }

    private void UjVaros_Click(object sender, RoutedEventArgs e)
    {
        var ablak = new VarosWindow() { Owner = this };
        if (ablak.ShowDialog() == true)
        {
            using var context = new AutoKolcsonzoContext();
            context.Varosok.Add(ablak.Eredmeny);
            if (!MentesBiztonsagosan(context, "város")) return;
            VarosokBetoltese();
        }
    }

    private void SzerkesztVaros_Click(object sender, RoutedEventArgs e)
    {
        VarosSzerkesztese();
    }

    private void VarosDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (!SorraKattintottak(sender, e)) return;
        VarosSzerkesztese();
    }

    private void VarosSzerkesztese()
    {
        if (VarosDataGrid.SelectedItem is not Varos kivalasztott)
        {
            MessageBox.Show("Válasszon ki egy várost!", "Nincs kijelölés");
            return;
        }

        var ablak = new VarosWindow(kivalasztott) { Owner = this };
        if (ablak.ShowDialog() == true)
        {
            using var context = new AutoKolcsonzoContext();
            context.Varosok.Add(ablak.Eredmeny);
            if (!MentesBiztonsagosan(context, "város")) return;
            VarosokBetoltese();
        }
    }

    private void TorolVaros_Click(object sender, RoutedEventArgs e)
    {
        if (VarosDataGrid.SelectedItem is not Varos kivalasztott)
        {
            MessageBox.Show("Válasszon ki egy várost!", "Nincs kijelölés");
            return;
        }

        string uzenet = $"Biztosan törli a(z) {kivalasztott.Nev} várost?";
        if (kivalasztott.Berlok.Count > 0)
            uzenet += $"\n\n{kivalasztott.Berlok.Count} bérlő lakcíme elveszíti a városát (a bérlők megmaradnak).";

        if (MessageBox.Show(uzenet, "Törlés megerősítése",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        using var context = new AutoKolcsonzoContext();
        var torlendo = context.Varosok.Find(kivalasztott.VarosId);
        if (torlendo != null)
        {
            context.Varosok.Remove(torlendo);
            if (!MentesBiztonsagosan(context, "város")) return;
        }
        VarosokBetoltese();
    }






    // ---------- TÍPUSOK ----------

    private void TipusokBetoltese()
    {
        using var context = new AutoKolcsonzoContext();

        string marka = TipusMarkaFilterTextBox.Text.Trim();
        string modell = TipusModellFilterTextBox.Text.Trim();
        string? uzemanyag = TipusUzemanyagFilterComboBox.SelectedItem as string;

        IQueryable<Tipus> lekerdezes = context.Tipusok.Include(t => t.Autok);

        if (marka != "")
            lekerdezes = lekerdezes.Where(t => t.Marka.Contains(marka));
        if (modell != "")
            lekerdezes = lekerdezes.Where(t => t.Modell.Contains(modell));
        if (uzemanyag != null && uzemanyag != "Összes")
            lekerdezes = lekerdezes.Where(t => t.Uzemanyag == uzemanyag);

        TipusDataGrid.ItemsSource = lekerdezes.OrderBy(t => t.TipusId).ToList();
    }

    private void TipusFilter_Changed(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded) return;
        TipusokBetoltese();
    }

    private void UjTipus_Click(object sender, RoutedEventArgs e)
    {
        var ablak = new TipusWindow() { Owner = this };
        if (ablak.ShowDialog() == true)
        {
            using var context = new AutoKolcsonzoContext();
            context.Tipusok.Add(ablak.Eredmeny);
            if (!MentesBiztonsagosan(context, "típus")) return;
            TipusokBetoltese();
        }
    }

    private void SzerkesztTipus_Click(object sender, RoutedEventArgs e)
    {
        TipusSzerkesztese();
    }

    private void TipusDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (!SorraKattintottak(sender, e)) return;
        TipusSzerkesztese();
    }

    private void TipusSzerkesztese()
    {
        if (TipusDataGrid.SelectedItem is not Tipus kivalasztott)
        {
            MessageBox.Show("Válasszon ki egy típust!", "Nincs kijelölés");
            return;
        }

        var ablak = new TipusWindow(kivalasztott) { Owner = this };
        if (ablak.ShowDialog() == true)
        {
            using var context = new AutoKolcsonzoContext();
            context.Tipusok.Add(ablak.Eredmeny);
            if (!MentesBiztonsagosan(context, "típus")) return;
            TipusokBetoltese();
        }
    }

    private void TorolTipus_Click(object sender, RoutedEventArgs e)
    {
        if (TipusDataGrid.SelectedItem is not Tipus kivalasztott)
        {
            MessageBox.Show("Válasszon ki egy típust!", "Nincs kijelölés");
            return;
        }

        if (kivalasztott.Autok.Count > 0)
        {
            MessageBox.Show($"A(z) {kivalasztott.Marka} {kivalasztott.Modell} típushoz még {kivalasztott.Autok.Count} autó tartozik, ezért nem törölhető.\n\nElőbb törölje vagy módosítsa a hozzá tartozó autókat.",
                            "A törlés nem lehetséges");
            return;
        }

        if (MessageBox.Show($"Biztosan törli a(z) {kivalasztott.Marka} {kivalasztott.Modell} típust?",
                "Törlés megerősítése", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        using var context = new AutoKolcsonzoContext();
        var torlendo = context.Tipusok.Find(kivalasztott.TipusId);
        if (torlendo != null)
        {
            context.Tipusok.Remove(torlendo);
            if (!MentesBiztonsagosan(context, "típus")) return;
        }
        TipusokBetoltese();
    }



    // ---------- AUTÓK ----------

    private void AutokBetoltese()
    {
        using var context = new AutoKolcsonzoContext();

        string marka = AutoMarkaFilterTextBox.Text.Trim();
        string modell = AutoModellFilterTextBox.Text.Trim();
        string maxArSzoveg = AutoMaxArFilterTextBox.Text.Trim();

        IQueryable<Auto> lekerdezes = context.Autok
            .Include(a => a.Tipus)
            .Include(a => a.Berlesek);

        if (marka != "")
            lekerdezes = lekerdezes.Where(a => a.Tipus.Marka.Contains(marka));
        if (modell != "")
            lekerdezes = lekerdezes.Where(a => a.Tipus.Modell.Contains(modell));
        if (int.TryParse(maxArSzoveg, out int maxAr))
            lekerdezes = lekerdezes.Where(a => a.NapiAr <= maxAr);

        AutoDataGrid.ItemsSource = lekerdezes.OrderBy(a => a.AutoId).ToList();
    }

    private void AutoFilter_Changed(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded) return;
        AutokBetoltese();
    }

    private void UjAuto_Click(object sender, RoutedEventArgs e)
    {
        var ablak = new AutoWindow() { Owner = this };
        if (ablak.ShowDialog() == true)
        {
            using var context = new AutoKolcsonzoContext();
            context.Autok.Add(ablak.Eredmeny);
            if (!MentesBiztonsagosan(context, "autó")) return;
            AutokBetoltese();
        }
    }

    private void SzerkesztAuto_Click(object sender, RoutedEventArgs e)
    {
        AutoSzerkesztese();
    }

    private void AutoDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (!SorraKattintottak(sender, e)) return;
        AutoSzerkesztese();
    }

    private void AutoSzerkesztese()
    {
        if (AutoDataGrid.SelectedItem is not Auto kivalasztott)
        {
            MessageBox.Show("Válasszon ki egy autót!", "Nincs kijelölés");
            return;
        }

        var ablak = new AutoWindow(kivalasztott) { Owner = this };
        if (ablak.ShowDialog() == true)
        {
            using var context = new AutoKolcsonzoContext();
            context.Autok.Update(ablak.Eredmeny);
            if (!MentesBiztonsagosan(context, "autó")) return;
            AutokBetoltese();
        }
    }

    private void TorolAuto_Click(object sender, RoutedEventArgs e)
    {
        if (AutoDataGrid.SelectedItem is not Auto kivalasztott)
        {
            MessageBox.Show("Válasszon ki egy autót!", "Nincs kijelölés");
            return;
        }

        if (kivalasztott.Berlesek.Count > 0)
        {
            MessageBox.Show($"Az autóhoz {kivalasztott.Berlesek.Count} bérlés tartozik, ezért nem törölhető.\n\nElőbb törölje a hozzá tartozó bérléseket.",
                            "A törlés nem lehetséges");
            return;
        }

        if (MessageBox.Show($"Biztosan törli a(z) {kivalasztott.Tipus.Marka} {kivalasztott.Tipus.Modell} autót (azonosító: {kivalasztott.AutoId})?",
                "Törlés megerősítése", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        using var context = new AutoKolcsonzoContext();
        var torlendo = context.Autok.Find(kivalasztott.AutoId);
        if (torlendo != null)
        {
            context.Autok.Remove(torlendo);
            if (!MentesBiztonsagosan(context, "autó")) return;
        }
        AutokBetoltese();
    }




    // ---------- BÉRLŐK ----------

    private void BerlokBetoltese()
    {
        using var context = new AutoKolcsonzoContext();

        string nev = BerloNevFilterTextBox.Text.Trim();
        string varos = BerloVarosFilterTextBox.Text.Trim();

        IQueryable<Berlo> lekerdezes = context.Berlok
            .Include(b => b.Varos)
            .Include(b => b.Berlesek);

        if (nev != "")
            lekerdezes = lekerdezes.Where(b => b.Nev.Contains(nev));
        if (varos != "")
            lekerdezes = lekerdezes.Where(b => b.Varos != null && b.Varos.Nev.Contains(varos));

        BerloDataGrid.ItemsSource = lekerdezes.OrderBy(b => b.BerloId).ToList();
    }

    private void BerloFilter_Changed(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded) return;
        BerlokBetoltese();
    }

    private void UjBerlo_Click(object sender, RoutedEventArgs e)
    {
        var ablak = new BerloWindow() { Owner = this };
        if (ablak.ShowDialog() == true)
        {
            using var context = new AutoKolcsonzoContext();

            var elsoBerles = ablak.ElsoBerles!;                    // always filled in "new" mode
            elsoBerles.Szamla = SzamlaGenerator.Uj(context);
            ablak.Eredmeny.Berlesek.Add(elsoBerles);               // link the rental to the new renter
            context.Berlok.Add(ablak.Eredmeny);                    // adds the renter AND the rental

            if (!MentesBiztonsagosan(context, "bérlő")) return;    // one SaveChanges = one transaction
            BerlokBetoltese();
        }
    }

    private void SzerkesztBerlo_Click(object sender, RoutedEventArgs e)
    {
        BerloSzerkesztese();
    }

    private void BerloDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (!SorraKattintottak(sender, e)) return;
        BerloSzerkesztese();
    }

    private void BerloSzerkesztese()
    {
        if (BerloDataGrid.SelectedItem is not Berlo kivalasztott)
        {
            MessageBox.Show("Válasszon ki egy bérlőt!", "Nincs kijelölés");
            return;
        }

        var ablak = new BerloWindow(kivalasztott) { Owner = this };
        if (ablak.ShowDialog() == true)
        {
            using var context = new AutoKolcsonzoContext();
            context.Berlok.Update(ablak.Eredmeny);
            if (!MentesBiztonsagosan(context, "bérlő")) return;
            BerlokBetoltese();
        }
    }

    private void TorolBerlo_Click(object sender, RoutedEventArgs e)
    {
        if (BerloDataGrid.SelectedItem is not Berlo kivalasztott)
        {
            MessageBox.Show("Válasszon ki egy bérlőt!", "Nincs kijelölés");
            return;
        }

        string uzenet = $"Biztosan törli a(z) {kivalasztott.Nev} nevű bérlőt?";
        if (kivalasztott.Berlesek.Count > 0)
            uzenet += $"\n\nA bérlő {kivalasztott.Berlesek.Count} bérlése is törlődik, és ezek kilométerei is kikerülnek az autók megtett km-éből.";

        if (MessageBox.Show(uzenet, "Törlés megerősítése",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            return;

        using var context = new AutoKolcsonzoContext();
        var torlendo = context.Berlok.Include(b => b.Berlesek).FirstOrDefault(b => b.BerloId == kivalasztott.BerloId);
        if (torlendo != null)
        {
            context.Berlesek.RemoveRange(torlendo.Berlesek);   // the rentals first...
            context.Berlok.Remove(torlendo);                   // ...then the renter, all in one SaveChanges
            if (!MentesBiztonsagosan(context, "bérlő")) return;
        }
        BerlokBetoltese();
    }
}