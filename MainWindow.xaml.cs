using System.Data;
using System.Globalization;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        BerlesAllapotFilterComboBox.ItemsSource = new[] { "Összes", "Tervezett", "Folyamatban", "Lezárt" };
        BerlesAllapotFilterComboBox.SelectedIndex = 0;

        VarosokBetoltese();
        TipusokBetoltese();
        AutokBetoltese();
        BerlokBetoltese();
        BerlesekBetoltese();
    }

    private void FoTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.OriginalSource != sender || !IsLoaded) return;

        VarosokBetoltese();
        TipusokBetoltese();
        AutokBetoltese();
        BerlokBetoltese();
        BerlesekBetoltese();
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
            MentesiHibaMegjelenitese(mit, ex);
            return false;
        }
    }

    private void MentesiHibaMegjelenitese(string mit, Exception ex)
    {
        string uzenet = $"Hiba történt a(z) {mit} mentésekor:\n{ex.Message}";
        if (ex.InnerException != null)
            uzenet += $"\n\nRészletek: {ex.InnerException.Message}";
        App.Uzenet(this, uzenet, "Mentési hiba");
    }

    private static bool SorraKattintottak(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not DependencyObject forras) return false;
        return ItemsControl.ContainerFromElement((DataGrid)sender, forras) is DataGridRow;
    }

    private static Predicate<string?> SzovegSzuro(TextBox mezo)
    {
        string szuro = mezo.Text.Trim();
        return ertek => szuro.Length == 0 || ertek?.Contains(szuro, StringComparison.CurrentCultureIgnoreCase) == true;
    }

    private static bool SzuroSzam<T>(TextBox mezo, out T? ertek) where T : struct, INumber<T>
    {
        string szoveg = mezo.Text.Trim();
        ertek = null;
        if (szoveg.Length == 0) return true;

        if (!T.TryParse(szoveg.Replace(',', '.'), CultureInfo.InvariantCulture, out T szam) || !T.IsFinite(szam))
            return false;

        ertek = szam;
        return true;
    }

    private static Predicate<T?> SzamSzuro<T>(TextBox minimum, TextBox maximum) where T : struct, INumber<T>
    {
        if (!SzuroSzam<T>(minimum, out var min) || !SzuroSzam<T>(maximum, out var max))
            return _ => false;

        return TartomanySzuro(min, max);
    }

    private static Predicate<T?> TartomanySzuro<T>(T? minimum, T? maximum) where T : struct, IComparable<T>
    {
        if (!minimum.HasValue && !maximum.HasValue) return _ => true;

        return ertek => ertek is T adat
            && (!minimum.HasValue || adat.CompareTo(minimum.Value) >= 0)
            && (!maximum.HasValue || adat.CompareTo(maximum.Value) <= 0);
    }

    private static void SzuroAlkalmazasa<T>(DataGrid tabla, Predicate<T> szuro)
    {
        tabla.Items.Filter = sor => sor is T adat && szuro(adat);
    }

    // ---------- VÁROSOK ----------

    private void VarosokBetoltese()
    {
        using var context = new AutoKolcsonzoContext();
        VarosDataGrid.ItemsSource = context.Varosok
            .Include(v => v.Berlok).OrderBy(v => v.VarosId).ToList();
        VarosSzures();
    }

    private void VarosSzures()
    {
        var nev = SzovegSzuro(VarosNevFilterTextBox);
        var iranySzam = SzovegSzuro(VarosIranySzamFilterTextBox);
        var megye = SzovegSzuro(VarosMegyeFilterTextBox);
        var berlok = SzamSzuro<int>(VarosBerlokMin, VarosBerlokMax);

        SzuroAlkalmazasa<Varos>(VarosDataGrid, v =>
            nev(v.Nev) && iranySzam(v.IranySzam) && megye(v.Megye) && berlok(v.Berlok.Count));
    }

    private void VarosFilter_Changed(object sender, RoutedEventArgs e)
    {
        if (IsLoaded) VarosSzures();
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
            App.Uzenet(this, "Válasszon ki egy várost!", "Nincs kijelölés");
            return;
        }

        var ablak = new VarosWindow(kivalasztott) { Owner = this };
        if (ablak.ShowDialog() == true)
        {
            using var context = new AutoKolcsonzoContext();
            context.Varosok.Update(ablak.Eredmeny);
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

        if (App.Uzenet(this, uzenet, "Törlés megerősítése",
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
        TipusDataGrid.ItemsSource = context.Tipusok
            .Include(t => t.Autok).OrderBy(t => t.TipusId).ToList();
        TipusSzures();
    }

    private void TipusSzures()
    {
        var marka = SzovegSzuro(TipusMarkaFilterTextBox);
        var modell = SzovegSzuro(TipusModellFilterTextBox);
        var evjarat = SzamSzuro<int>(TipusEvjaratMin, TipusEvjaratMax);
        var fogyasztas = SzamSzuro<double>(TipusFogyasztasMin, TipusFogyasztasMax);
        var autok = SzamSzuro<int>(TipusAutokMin, TipusAutokMax);
        string? uzemanyag = TipusUzemanyagFilterComboBox.SelectedItem as string;

        SzuroAlkalmazasa<Tipus>(TipusDataGrid, t => marka(t.Marka) && modell(t.Modell)
            && evjarat(t.Evjarat) && fogyasztas(t.Fogyasztas) && autok(t.Autok.Count)
            && (uzemanyag is null or "Összes" || t.Uzemanyag == uzemanyag));
    }

    private void TipusFilter_Changed(object sender, RoutedEventArgs e)
    {
        if (IsLoaded) TipusSzures();
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
            App.Uzenet(this, "Válasszon ki egy típust!", "Nincs kijelölés");
            return;
        }

        var ablak = new TipusWindow(kivalasztott) { Owner = this };
        if (ablak.ShowDialog() == true)
        {
            using var context = new AutoKolcsonzoContext();
            context.Tipusok.Update(ablak.Eredmeny);
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
            App.Uzenet(this, $"A(z) {kivalasztott.Marka} {kivalasztott.Modell} típushoz még {kivalasztott.Autok.Count} autó tartozik, ezért nem törölhető.\n\nElőbb törölje vagy módosítsa a hozzá tartozó autókat.",
                            "A törlés nem lehetséges");
            return;
        }

        if (App.Uzenet(this, $"Biztosan törli a(z) {kivalasztott.Marka} {kivalasztott.Modell} típust?",
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
        AutoDataGrid.ItemsSource = context.Autok
            .Include(a => a.Tipus)
            .Include(a => a.Berlesek).OrderBy(a => a.AutoId).ToList();
        AutoSzures();
    }

    private void AutoSzures()
    {
        var marka = SzovegSzuro(AutoMarkaFilterTextBox);
        var modell = SzovegSzuro(AutoModellFilterTextBox);
        var evjarat = SzamSzuro<int>(AutoEvjaratMin, AutoEvjaratMax);
        var alapKm = SzamSzuro<int>(AutoAlapKmMin, AutoAlapKmMax);
        var megtettKm = SzamSzuro<long>(AutoMegtettKmMin, AutoMegtettKmMax);
        var ar = SzamSzuro<int>(AutoArMin, AutoArMax);
        var berlesek = SzamSzuro<int>(AutoBerlesekMin, AutoBerlesekMax);

        SzuroAlkalmazasa<Auto>(AutoDataGrid, a => marka(a.Tipus.Marka) && modell(a.Tipus.Modell)
            && evjarat(a.Tipus.Evjarat) && alapKm(a.AlapKm) && megtettKm(a.MegtettKm)
            && ar(a.NapiAr) && berlesek(a.Berlesek.Count));
    }

    private void AutoFilter_Changed(object sender, RoutedEventArgs e)
    {
        if (IsLoaded) AutoSzures();
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
            App.Uzenet(this, "Válasszon ki egy autót!", "Nincs kijelölés");
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
            App.Uzenet(this, $"Az autóhoz {kivalasztott.Berlesek.Count} bérlés tartozik, ezért nem törölhető.\n\nElőbb törölje a hozzá tartozó bérléseket.",
                            "A törlés nem lehetséges");
            return;
        }

        if (App.Uzenet(this, $"Biztosan törli a(z) {kivalasztott.Tipus.Marka} {kivalasztott.Tipus.Modell} autót (azonosító: {kivalasztott.AutoId})?",
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
        BerloDataGrid.ItemsSource = context.Berlok
            .Include(b => b.Varos)
            .Include(b => b.Berlesek).OrderBy(b => b.BerloId).ToList();
        BerloSzures();
    }

    private void BerloSzures()
    {
        var nev = SzovegSzuro(BerloNevFilterTextBox);
        var szulDatum = TartomanySzuro(BerloSzulDatumMin.SelectedDate, BerloSzulDatumMax.SelectedDate);
        var eletkor = SzamSzuro<int>(BerloEletkorMin, BerloEletkorMax);
        var varos = SzovegSzuro(BerloVarosFilterTextBox);
        var utca = SzovegSzuro(BerloUtcaFilterTextBox);
        var hazSzam = SzovegSzuro(BerloHazSzamFilterTextBox);
        var berlesek = SzamSzuro<int>(BerloBerlesekMin, BerloBerlesekMax);

        SzuroAlkalmazasa<Berlo>(BerloDataGrid, b => nev(b.Nev) && szulDatum(b.SzulDatum.Date)
            && eletkor(b.Eletkor) && varos(b.Varos?.Nev) && utca(b.UtcaNev)
            && hazSzam(b.HazSzam) && berlesek(b.Berlesek.Count));
    }

    private void BerloFilter_Changed(object sender, RoutedEventArgs e)
    {
        if (IsLoaded) BerloSzures();
    }

    private void UjBerlo_Click(object sender, RoutedEventArgs e)
    {
        var ablak = new BerloWindow() { Owner = this };
        if (ablak.ShowDialog() == true)
        {
            if (!BerlesMentese(ablak.ElsoBerles!, ujBerlo: ablak.Eredmeny)) return;
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
            App.Uzenet(this, "Válasszon ki egy bérlőt!", "Nincs kijelölés");
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



    // ---------- BÉRLÉSEK ----------

    private void BerlesekBetoltese()
    {
        using var context = new AutoKolcsonzoContext();
        BerlesDataGrid.ItemsSource = context.Berlesek
            .Include(b => b.Berlo)
            .Include(b => b.Auto).ThenInclude(a => a.Tipus)
            .OrderByDescending(b => b.KezdoDatum).ToList();
        BerlesSzures();
    }

    private void BerlesSzures()
    {
        var szamla = SzovegSzuro(BerlesSzamlaFilterTextBox);
        var berlo = SzovegSzuro(BerlesBerloFilterTextBox);
        var ar = SzamSzuro<long>(BerlesArMin, BerlesArMax);
        var marka = SzovegSzuro(BerlesMarkaFilterTextBox);
        var modell = SzovegSzuro(BerlesModellFilterTextBox);
        var kezdoDatum = TartomanySzuro(BerlesKezdoDatumMin.SelectedDate, BerlesKezdoDatumMax.SelectedDate);
        var vegDatum = TartomanySzuro(BerlesVegDatumMin.SelectedDate, BerlesVegDatumMax.SelectedDate);
        var km = SzamSzuro<int>(BerlesKmMin, BerlesKmMax);
        var hossz = SzamSzuro<int>(BerlesHosszMin, BerlesHosszMax);
        string? allapot = BerlesAllapotFilterComboBox.SelectedItem as string;

        SzuroAlkalmazasa<Berles>(BerlesDataGrid, b => szamla(b.Szamla) && berlo(b.Berlo.Nev) && ar(b.Ar)
            && marka(b.Auto.Tipus.Marka) && modell(b.Auto.Tipus.Modell)
            && kezdoDatum(b.KezdoDatum.Date) && vegDatum(b.VegDatum?.Date) && km(b.Km) && hossz(b.Hossz)
            && (allapot is null or "Összes" || b.Allapot == allapot));
    }

    private void BerlesFilter_Changed(object sender, RoutedEventArgs e)
    {
        if (IsLoaded) BerlesSzures();
    }

    private bool BerlesMentese(Berles berles, bool szerkesztes = false, Berlo? ujBerlo = null)
    {
        string? utkozes;
        try
        {
            using var context = new AutoKolcsonzoContext();
            using var tranzakcio = context.Database.BeginTransaction(IsolationLevel.Serializable);
            utkozes = BerlesEllenorzo.UtkozesHiba(context, berles, szerkesztes);
            if (utkozes == null)
            {
                if (szerkesztes)
                {
                    context.Berlesek.Update(berles);
                }
                else
                {
                    berles.Szamla = SzamlaGenerator.Uj(context);
                    if (ujBerlo == null)
                    {
                        context.Berlesek.Add(berles);
                    }
                    else
                    {
                        ujBerlo.Berlesek.Add(berles);
                        context.Berlok.Add(ujBerlo);
                    }
                }

                context.SaveChanges();
                tranzakcio.Commit();
                return true;
            }
        }
        catch (Exception ex)
        {
            MentesiHibaMegjelenitese(ujBerlo != null ? "bérlő" : "bérlés", ex);
            return false;
        }

        App.Uzenet(this, utkozes, "Ütköző bérlés");
        return false;
    }

    private void UjBerles_Click(object sender, RoutedEventArgs e)
    {
        var ablak = new BerlesWindow() { Owner = this };
        if (ablak.ShowDialog() == true)
        {
            if (!BerlesMentese(ablak.Eredmeny)) return;
            BerlesekBetoltese();
        }
    }

    private void SzerkesztBerles_Click(object sender, RoutedEventArgs e)
    {
        BerlesSzerkesztese();
    }

    private void BerlesDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (!SorraKattintottak(sender, e)) return;
        BerlesSzerkesztese();
    }

    private void BerlesSzerkesztese()
    {
        if (BerlesDataGrid.SelectedItem is not Berles kivalasztott)
        {
            App.Uzenet(this, "Válasszon ki egy bérlést!", "Nincs kijelölés");
            return;
        }

        var ablak = new BerlesWindow(kivalasztott) { Owner = this };
        if (ablak.ShowDialog() == true)
        {
            if (!BerlesMentese(ablak.Eredmeny, szerkesztes: true)) return;
            BerlesekBetoltese();
        }
    }

    private void TorolBerles_Click(object sender, RoutedEventArgs e)
    {
        if (BerlesDataGrid.SelectedItem is not Berles kivalasztott)
        {
            MessageBox.Show("Válasszon ki egy bérlést!", "Nincs kijelölés");
            return;
        }

        using var context = new AutoKolcsonzoContext();

        int darab = context.Berlesek.Count(b => b.BerloId == kivalasztott.BerloId);
        if (darab <= 1)
        {
            App.Uzenet(this, $"Ez {kivalasztott.Berlo.Nev} egyetlen bérlése, ezért nem törölhető.\n\nHa a bérlőt is el szeretné távolítani, törölje a Bérlők fülön (ott a bérlései is törlődnek).",
                            "A törlés nem lehetséges");
            return;
        }

        string uzenet = $"Biztosan törli a(z) {kivalasztott.Szamla} számlájú bérlést?";
        if (kivalasztott.Km.HasValue)
            uzenet += $"\n\nA bérlés {kivalasztott.Km:N0} km-e kikerül az autó megtett kilométereiből.";

        if (MessageBox.Show(uzenet, "Törlés megerősítése",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        var torlendo = context.Berlesek.Find(kivalasztott.KezdoDatum, kivalasztott.AutoId, kivalasztott.BerloId);
        if (torlendo != null)
        {
            context.Berlesek.Remove(torlendo);
            if (!MentesBiztonsagosan(context, "bérlés")) return;
        }
        BerlesekBetoltese();
    }
}
