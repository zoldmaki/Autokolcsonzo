using System.Data;
using System.Globalization;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

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

        StateChanged += (_, _) => FrissitMunkaterulet();
        LocationChanged += (_, _) => FrissitMunkaterulet();
        WorkspaceSurface.SizeChanged += (_, _) => FrissitMunkaterulet();
        SystemParameters.StaticPropertyChanged += MunkateruletMegvaltozott;
        Closed += (_, _) => SystemParameters.StaticPropertyChanged -= MunkateruletMegvaltozott;

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

    private void MunkateruletMegvaltozott(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SystemParameters.WorkArea))
            FrissitMunkaterulet();
    }

    private void FrissitMunkaterulet()
    {
        if (WindowState != WindowState.Maximized)
        {
            WorkspaceSurface.Padding = default;
            return;
        }

        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () =>
        {
            if (WindowState != WindowState.Maximized || PresentationSource.FromVisual(WorkspaceSurface) == null) return;
            if (MonitorWorkArea.Get(new WindowInteropHelper(this).Handle) is not Rect workArea)
            {
                WorkspaceSurface.Padding = SystemParameters.WindowResizeBorderThickness;
                return;
            }

            Point first = WorkspaceSurface.PointFromScreen(workArea.TopLeft);
            Point last = WorkspaceSurface.PointFromScreen(workArea.BottomRight);
            WorkspaceSurface.Padding = new Thickness(Math.Max(0, first.X), Math.Max(0, first.Y),
                Math.Max(0, WorkspaceSurface.ActualWidth - last.X), Math.Max(0, WorkspaceSurface.ActualHeight - last.Y));
        });
    }

    private void Minimize_Click(object sender, RoutedEventArgs e) => SystemCommands.MinimizeWindow(this);

    private void MaximizeRestore_Click(object sender, RoutedEventArgs e)
    {
        if (WindowState == WindowState.Maximized)
            SystemCommands.RestoreWindow(this);
        else
            SystemCommands.MaximizeWindow(this);
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
        if (e.ChangedButton != MouseButton.Left || Keyboard.Modifiers != ModifierKeys.None) return false;
        if (e.OriginalSource is not DependencyObject forras) return false;
        return ItemsControl.ContainerFromElement((DataGrid)sender, forras) is DataGridRow;
    }

    private static Predicate<string?> SzovegSzuro(TextBox mezo)
    {
        string szuro = mezo.Text.Trim();
        var osszehasonlitas = CultureInfo.InvariantCulture.CompareInfo;
        const CompareOptions opciok = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;
        return ertek => szuro.Length == 0 || (ertek != null && osszehasonlitas.IndexOf(ertek, szuro, opciok) >= 0);
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

    private static void SzuroAlkalmazasa<T>(DataGrid tabla, Predicate<T> szuro) where T : class
    {
        var kijeloltek = tabla.SelectedItems.OfType<T>().ToArray();
        tabla.Items.Filter = sor => sor is T adat && szuro(adat);
        foreach (var sor in kijeloltek)
        {
            if (!tabla.Items.Contains(sor))
                tabla.SelectedItems.Remove(sor);
            else if (!tabla.SelectedItems.Contains(sor))
                tabla.SelectedItems.Add(sor);
        }
    }

    private void SzuresValtozott(WorkspaceFilterPanel panel, Action szures)
    {
        if (!IsLoaded || panel.IsResetting) return;

        szures();
        panel.RefreshIndicators();
    }

    private static T[] KijeloltSorok<T>(DataGrid tabla) where T : class =>
        tabla.SelectedItems.OfType<T>().Where(sor => tabla.Items.Contains(sor)).ToArray();

    private bool TorlesMegerositve(string egyes, string tobbes, string ikon, IEnumerable<TorlesWindow.Sor> sorok,
        string? kovetkezmeny = null, TorlesWindow.KapcsolatTipus? kapcsolat = null)
    {
        var adatok = sorok.ToArray();
        string cim = (adatok.Length == 1 ? egyes : tobbes) + " törlése";
        string kerdes = adatok.Length == 1 ? "Törli a kijelölt elemet?" : $"Törli a kijelölt elemeket ({adatok.Length} db)?";
        return App.ShowModal(new TorlesWindow(this, cim, kerdes, adatok, ikon, kovetkezmeny, kapcsolat)) == true;
    }

    private static string BerlesIdoszakEsKm(Berles berles)
    {
        string idoszak = berles.VegDatum is DateTime vege
            ? $"{berles.KezdoDatum:yyyy.MM.dd.} → {vege:yyyy.MM.dd.}"
            : $"{berles.KezdoDatum:yyyy.MM.dd.} · nyitott";
        return berles.Km is int km ? $"{idoszak} · {km:N0} km" : idoszak;
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
        var keres = SzovegSzuro(VarosSearch);
        var nev = SzovegSzuro(VarosNevFilterTextBox);
        var iranySzam = SzovegSzuro(VarosIranySzamFilterTextBox);
        var megye = SzovegSzuro(VarosMegyeFilterTextBox);
        var berlok = SzamSzuro<int>(VarosBerlokMin, VarosBerlokMax);

        SzuroAlkalmazasa<Varos>(VarosDataGrid, v =>
            (keres(v.Nev) || keres(v.IranySzam) || keres(v.Megye))
            && nev(v.Nev) && iranySzam(v.IranySzam) && megye(v.Megye) && berlok(v.Berlok.Count));
    }

    private void VarosFilter_Changed(object sender, RoutedEventArgs e) => SzuresValtozott(VarosFilters, VarosSzures);

    private void UjVaros_Click(object sender, RoutedEventArgs e)
    {
        var ablak = new VarosWindow() { Owner = this };
        if (App.ShowModal(ablak) == true)
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
        if (VarosDataGrid.SelectedItems.Count != 1 || VarosDataGrid.SelectedItem is not Varos kivalasztott)
        {
            App.Uzenet(this, "Válasszon ki egy várost!", "Nincs kijelölés");
            return;
        }

        var ablak = new VarosWindow(kivalasztott) { Owner = this };
        if (App.ShowModal(ablak) == true)
        {
            using var context = new AutoKolcsonzoContext();
            context.Varosok.Update(ablak.Eredmeny);
            if (!MentesBiztonsagosan(context, "város")) return;
            VarosokBetoltese();
        }
    }

    private void TorolVaros_Click(object sender, RoutedEventArgs e)
    {
        var kijeloltek = KijeloltSorok<Varos>(VarosDataGrid);
        if (kijeloltek.Length == 0) return;

        var jovahagyottBerlok = kijeloltek.SelectMany(v => v.Berlok).Select(b => b.BerloId).ToHashSet();
        string? kovetkezmeny = jovahagyottBerlok.Count > 0
            ? $"{jovahagyottBerlok.Count} bérlő címéből törlődik a város, az utca és a házszám." : null;
        var sorok = kijeloltek.Select(v => new TorlesWindow.Sor(v.Nev, $"{v.IranySzam} · {v.Megye}",
            v.Berlok.OrderBy(b => b.Nev).Select(b => new TorlesWindow.Sor(b.Nev, $"#{b.BerloId} · {b.SzulDatum:yyyy.MM.dd.}")).ToArray()));
        if (!TorlesMegerositve("Város", "Városok", "CityIcon", sorok, kovetkezmeny,
                new("Érintett bérlő", "Érintett bérlők", "RenterIcon"))) return;

        int[] azonosito = kijeloltek.Select(v => v.VarosId).ToArray();
        using var context = new AutoKolcsonzoContext();
        bool megvaltozott;
        try
        {
            using var tranzakcio = context.Database.BeginTransaction(IsolationLevel.Serializable);
            var erintettBerlok = context.Berlok.Where(b => b.VarosId.HasValue && azonosito.Contains(b.VarosId.Value)).ToList();
            megvaltozott = !jovahagyottBerlok.SetEquals(erintettBerlok.Select(b => b.BerloId));
            if (!megvaltozott)
            {
                foreach (var berlo in erintettBerlok)
                {
                    berlo.VarosId = null;
                    berlo.UtcaNev = null;
                    berlo.HazSzam = null;
                }
                context.Varosok.RemoveRange(context.Varosok.Where(v => azonosito.Contains(v.VarosId)).ToList());
                context.SaveChanges();
                tranzakcio.Commit();
            }
        }
        catch (Exception ex)
        {
            MentesiHibaMegjelenitese("város törlése", ex);
            return;
        }
        VarosokBetoltese();
        if (megvaltozott)
            App.Uzenet(this, "Az érintett bérlők időközben megváltoztak. Ellenőrizze a frissített listát.", "A törlés megszakadt");
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
        var keres = SzovegSzuro(TipusSearch);
        var marka = SzovegSzuro(TipusMarkaFilterTextBox);
        var modell = SzovegSzuro(TipusModellFilterTextBox);
        var evjarat = SzamSzuro<int>(TipusEvjaratMin, TipusEvjaratMax);
        var fogyasztas = SzamSzuro<double>(TipusFogyasztasMin, TipusFogyasztasMax);
        var autok = SzamSzuro<int>(TipusAutokMin, TipusAutokMax);
        string? uzemanyag = TipusUzemanyagFilterComboBox.SelectedItem as string;

        SzuroAlkalmazasa<Tipus>(TipusDataGrid, t =>
            (keres(t.Megnevezes) || keres(t.Uzemanyag)) && marka(t.Marka) && modell(t.Modell)
            && evjarat(t.Evjarat) && fogyasztas(t.Fogyasztas) && autok(t.Autok.Count)
            && (uzemanyag is null or "Összes" || t.Uzemanyag == uzemanyag));
    }

    private void TipusFilter_Changed(object sender, RoutedEventArgs e) => SzuresValtozott(TipusFilters, TipusSzures);

    private void UzemanyagFilter_Changed(object sender, SelectionChangedEventArgs e) => TipusFilter_Changed(sender, e);

    private void UjTipus_Click(object sender, RoutedEventArgs e)
    {
        var ablak = new TipusWindow() { Owner = this };
        if (App.ShowModal(ablak) == true)
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
        if (TipusDataGrid.SelectedItems.Count != 1 || TipusDataGrid.SelectedItem is not Tipus kivalasztott)
        {
            App.Uzenet(this, "Válasszon ki egy típust!", "Nincs kijelölés");
            return;
        }

        var ablak = new TipusWindow(kivalasztott) { Owner = this };
        if (App.ShowModal(ablak) == true)
        {
            using var context = new AutoKolcsonzoContext();
            context.Tipusok.Update(ablak.Eredmeny);
            if (!MentesBiztonsagosan(context, "típus")) return;
            TipusokBetoltese();
        }
    }

    private void TorolTipus_Click(object sender, RoutedEventArgs e)
    {
        var kijeloltek = KijeloltSorok<Tipus>(TipusDataGrid);
        if (kijeloltek.Length == 0) return;

        var foglaltak = kijeloltek.Where(t => t.Autok.Count > 0).ToArray();
        if (foglaltak.Length > 0)
        {
            int autoszam = foglaltak.Sum(t => t.Autok.Count);
            string uzenet = foglaltak.Length == 1
                ? $"Ehhez az autótípushoz még {autoszam} autó van rendelve."
                : $"A felsorolt {foglaltak.Length} autótípushoz összesen {autoszam} autó van rendelve.";
            const string kovetkezmeny = "Az autótípus akkor törölhető, ha már egy autó sincs hozzá rendelve. " +
                "A hibás hozzárendelést az Autók fülön, a Szerkesztés ablak Típus mezőjében javíthatja.";
            var sorok = foglaltak.Select(t => new TorlesWindow.Sor($"{t.Marka} {t.Modell}", $"{t.Evjarat} · {t.Uzemanyag}",
                t.Autok.OrderBy(a => a.AutoId).Select(a => new TorlesWindow.Sor($"Autó #{a.AutoId}", $"{a.NapiAr:N0} Ft/nap")).ToArray()));
            App.ShowModal(new TorlesWindow(this, "A törlés nem lehetséges", uzenet,
                sorok, "TypeIcon", kovetkezmeny, new("Kapcsolódó autó", "Kapcsolódó autók", "CarIcon"), blokkolt: true));
            return;
        }

        if (!TorlesMegerositve("Típus", "Típusok", "TypeIcon",
                kijeloltek.Select(t => new TorlesWindow.Sor($"{t.Marka} {t.Modell}", $"{t.Evjarat} · {t.Uzemanyag}")))) return;

        int[] azonosito = kijeloltek.Select(t => t.TipusId).ToArray();
        using var context = new AutoKolcsonzoContext();
        context.Tipusok.RemoveRange(context.Tipusok.Where(t => azonosito.Contains(t.TipusId)).ToList());
        if (MentesBiztonsagosan(context, "típus törlése")) TipusokBetoltese();
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
        var keres = SzovegSzuro(AutoSearch);
        var marka = SzovegSzuro(AutoMarkaFilterTextBox);
        var modell = SzovegSzuro(AutoModellFilterTextBox);
        var evjarat = SzamSzuro<int>(AutoEvjaratMin, AutoEvjaratMax);
        var alapKm = SzamSzuro<int>(AutoAlapKmMin, AutoAlapKmMax);
        var megtettKm = SzamSzuro<long>(AutoMegtettKmMin, AutoMegtettKmMax);
        var ar = SzamSzuro<int>(AutoArMin, AutoArMax);
        var berlesek = SzamSzuro<int>(AutoBerlesekMin, AutoBerlesekMax);

        SzuroAlkalmazasa<Auto>(AutoDataGrid, a =>
            (keres(a.AutoId.ToString(CultureInfo.InvariantCulture)) || keres(a.Tipus.Megnevezes))
            && marka(a.Tipus.Marka) && modell(a.Tipus.Modell)
            && evjarat(a.Tipus.Evjarat) && alapKm(a.AlapKm) && megtettKm(a.MegtettKm)
            && ar(a.NapiAr) && berlesek(a.Berlesek.Count));
    }

    private void AutoFilter_Changed(object sender, RoutedEventArgs e) => SzuresValtozott(AutoFilters, AutoSzures);

    private void UjAuto_Click(object sender, RoutedEventArgs e)
    {
        var ablak = new AutoWindow() { Owner = this };
        if (App.ShowModal(ablak) == true)
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
        if (AutoDataGrid.SelectedItems.Count != 1 || AutoDataGrid.SelectedItem is not Auto kivalasztott)
        {
            App.Uzenet(this, "Válasszon ki egy autót!", "Nincs kijelölés");
            return;
        }

        var ablak = new AutoWindow(kivalasztott) { Owner = this };
        if (App.ShowModal(ablak) == true)
        {
            using var context = new AutoKolcsonzoContext();
            context.Autok.Update(ablak.Eredmeny);
            if (!MentesBiztonsagosan(context, "autó")) return;
            AutokBetoltese();
        }
    }

    private void TorolAuto_Click(object sender, RoutedEventArgs e)
    {
        var kijeloltek = KijeloltSorok<Auto>(AutoDataGrid);
        if (kijeloltek.Length == 0) return;

        int[] azonosito = kijeloltek.Select(a => a.AutoId).ToArray();
        using var context = new AutoKolcsonzoContext();
        List<Auto> autok;
        try
        {
            autok = context.Autok.Where(a => azonosito.Contains(a.AutoId)).Include(a => a.Tipus)
                .Include(a => a.Berlesek).ThenInclude(b => b.Berlo).ToList();
        }
        catch (Exception ex)
        {
            MentesiHibaMegjelenitese("autó törlésének előkészítése", ex);
            return;
        }
        if (autok.Count == 0)
        {
            AutokBetoltese();
            return;
        }

        var foglaltak = autok.Where(a => a.Berlesek.Count > 0).ToArray();
        if (foglaltak.Length > 0)
        {
            int berlesszam = foglaltak.Sum(a => a.Berlesek.Count);
            string uzenet = foglaltak.Length == 1
                ? $"Ehhez az autóhoz még {berlesszam} bérlés tartozik."
                : $"A felsorolt {foglaltak.Length} autóhoz összesen {berlesszam} bérlés tartozik.";
            var sorok = foglaltak.Select(a => new TorlesWindow.Sor($"{a.Tipus.Marka} {a.Tipus.Modell} · #{a.AutoId}",
                $"{a.Tipus.Evjarat} · {a.Tipus.Uzemanyag}", a.Berlesek.OrderByDescending(b => b.KezdoDatum)
                    .Select(b => new TorlesWindow.Sor($"{b.Szamla} · {b.Berlo.Nev}", BerlesIdoszakEsKm(b))).ToArray()));
            App.ShowModal(new TorlesWindow(this, "A törlés nem lehetséges", uzenet,
                sorok, "CarIcon", kapcsolat: new("Kapcsolódó bérlés", "Kapcsolódó bérlések", "RentalIcon"), blokkolt: true));
            return;
        }

        if (!TorlesMegerositve("Autó", "Autók", "CarIcon",
                autok.Select(a => new TorlesWindow.Sor(a.Tipus.Megnevezes, $"#{a.AutoId} · {a.NapiAr:N0} Ft/nap")))) return;

        context.Autok.RemoveRange(autok);
        if (MentesBiztonsagosan(context, "autó törlése")) AutokBetoltese();
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
        var keres = SzovegSzuro(BerloSearch);
        var nev = SzovegSzuro(BerloNevFilterTextBox);
        var szulDatum = TartomanySzuro(BerloSzulDatumMin.SelectedDate, BerloSzulDatumMax.SelectedDate);
        var eletkor = SzamSzuro<int>(BerloEletkorMin, BerloEletkorMax);
        var varos = SzovegSzuro(BerloVarosFilterTextBox);
        var utca = SzovegSzuro(BerloUtcaFilterTextBox);
        var hazSzam = SzovegSzuro(BerloHazSzamFilterTextBox);
        var berlesek = SzamSzuro<int>(BerloBerlesekMin, BerloBerlesekMax);

        SzuroAlkalmazasa<Berlo>(BerloDataGrid, b =>
            (keres(b.Nev) || keres(b.Varos?.Nev) || keres(b.UtcaNev)) && nev(b.Nev) && szulDatum(b.SzulDatum.Date)
            && eletkor(b.Eletkor) && varos(b.Varos?.Nev) && utca(b.UtcaNev)
            && hazSzam(b.HazSzam) && berlesek(b.Berlesek.Count));
    }

    private void BerloFilter_Changed(object sender, RoutedEventArgs e) => SzuresValtozott(BerloFilters, BerloSzures);

    private void UjBerlo_Click(object sender, RoutedEventArgs e)
    {
        var ablak = new BerloWindow() { Owner = this };
        if (App.ShowModal(ablak) == true)
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
        if (BerloDataGrid.SelectedItems.Count != 1 || BerloDataGrid.SelectedItem is not Berlo kivalasztott)
        {
            App.Uzenet(this, "Válasszon ki egy bérlőt!", "Nincs kijelölés");
            return;
        }

        var ablak = new BerloWindow(kivalasztott) { Owner = this };
        if (App.ShowModal(ablak) == true)
        {
            using var context = new AutoKolcsonzoContext();
            context.Berlok.Update(ablak.Eredmeny);
            if (!MentesBiztonsagosan(context, "bérlő")) return;
            BerlokBetoltese();
        }
    }

    private void TorolBerlo_Click(object sender, RoutedEventArgs e)
    {
        var kijeloltek = KijeloltSorok<Berlo>(BerloDataGrid);
        if (kijeloltek.Length == 0) return;

        int[] azonosito = kijeloltek.Select(b => b.BerloId).ToArray();
        using var context = new AutoKolcsonzoContext();
        var erintettek = context.Berlok.Where(b => azonosito.Contains(b.BerloId)).Include(b => b.Berlesek);
        List<Berlo> elonezet;
        try
        {
            elonezet = erintettek.ThenInclude(b => b.Auto).ThenInclude(a => a.Tipus).AsNoTracking().ToList();
        }
        catch (Exception ex)
        {
            MentesiHibaMegjelenitese("bérlő törlésének előkészítése", ex);
            return;
        }
        if (elonezet.Count == 0)
        {
            BerlokBetoltese();
            return;
        }

        var berlesek = elonezet.SelectMany(b => b.Berlesek).ToArray();
        string? kovetkezmeny = berlesek.Length > 0 ? $"{berlesek.Length} felsorolt bérlés is törlődik." : null;
        if (KiszamoltKmValtozasa(berlesek.Sum(b => (long)(b.Km ?? 0))) is string kmSzoveg)
            kovetkezmeny += "\n" + kmSzoveg;

        var sorok = elonezet.Select(b => new TorlesWindow.Sor(b.Nev, $"#{b.BerloId} · {b.SzulDatum:yyyy.MM.dd.}",
            b.Berlesek.OrderByDescending(r => r.KezdoDatum).Select(r =>
                new TorlesWindow.Sor($"{r.Auto.Tipus.Marka} {r.Auto.Tipus.Modell} · #{r.AutoId}",
                    $"{r.Szamla} · {BerlesIdoszakEsKm(r)}")).ToArray()));
        if (!TorlesMegerositve("Bérlő", "Bérlők", "RenterIcon", sorok, kovetkezmeny,
                new("Kapcsolódó bérlés", "Kapcsolódó bérlések", "CarIcon"))) return;

        static (int BerloId, int AutoId, DateTime Kezdo, DateTime? Veg, string Szamla, int? Km) BerlesAdat(Berles b) =>
            (b.BerloId, b.AutoId, b.KezdoDatum, b.VegDatum, b.Szamla, b.Km);
        var jovahagyottBerlesek = berlesek.Select(BerlesAdat).ToHashSet();
        bool megvaltozott;
        try
        {
            using var tranzakcio = context.Database.BeginTransaction(IsolationLevel.Serializable);
            var torlendo = erintettek.ToList();
            var torlendoBerlesek = torlendo.SelectMany(b => b.Berlesek).ToArray();
            megvaltozott = torlendo.Count != elonezet.Count || !jovahagyottBerlesek.SetEquals(torlendoBerlesek.Select(BerlesAdat));
            if (!megvaltozott)
            {
                context.Berlesek.RemoveRange(torlendoBerlesek);
                context.Berlok.RemoveRange(torlendo);
                context.SaveChanges();
                tranzakcio.Commit();
            }
        }
        catch (Exception ex)
        {
            MentesiHibaMegjelenitese("bérlő törlése", ex);
            return;
        }

        BerlokBetoltese();
        if (megvaltozott)
            App.Uzenet(this, "A bérlők vagy a kapcsolódó bérlések időközben megváltoztak. Ellenőrizze a frissített listát.",
                "A törlés megszakadt");
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
        var keres = SzovegSzuro(BerlesSearch);
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

        SzuroAlkalmazasa<Berles>(BerlesDataGrid, b =>
            (keres(b.Szamla) || keres(b.Berlo.Nev) || keres(b.Auto.Tipus.Megnevezes))
            && szamla(b.Szamla) && berlo(b.Berlo.Nev) && ar(b.Ar)
            && marka(b.Auto.Tipus.Marka) && modell(b.Auto.Tipus.Modell)
            && kezdoDatum(b.KezdoDatum.Date) && vegDatum(b.VegDatum?.Date) && km(b.Km) && hossz(b.Hossz)
            && (allapot is null or "Összes" || b.Allapot == allapot));
    }

    private void BerlesFilter_Changed(object sender, RoutedEventArgs e) => SzuresValtozott(BerlesFilters, BerlesSzures);

    private void AllapotFilter_Changed(object sender, SelectionChangedEventArgs e) => BerlesFilter_Changed(sender, e);

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
        if (App.ShowModal(ablak) == true)
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
        if (BerlesDataGrid.SelectedItems.Count != 1 || BerlesDataGrid.SelectedItem is not Berles kivalasztott)
        {
            App.Uzenet(this, "Válasszon ki egy bérlést!", "Nincs kijelölés");
            return;
        }

        var ablak = new BerlesWindow(kivalasztott) { Owner = this };
        if (App.ShowModal(ablak) == true)
        {
            if (!BerlesMentese(ablak.Eredmeny, szerkesztes: true)) return;
            BerlesekBetoltese();
        }
    }

    private void TorolBerles_Click(object sender, RoutedEventArgs e)
    {
        var kijeloltek = KijeloltSorok<Berles>(BerlesDataGrid);
        if (kijeloltek.Length == 0) return;
        int[] berloIds = kijeloltek.Select(b => b.BerloId).Distinct().ToArray();
        using var context = new AutoKolcsonzoContext();

        var berloBerlesei = context.Berlesek.Where(b => berloIds.Contains(b.BerloId));
        var osszes = berloBerlesei.GroupBy(b => b.BerloId).Select(g => new { Id = g.Key, Darab = g.Count() })
            .ToDictionary(g => g.Id, g => g.Darab);
        var blokkolok = BerlesNelkulMaradoBerlok(kijeloltek, osszes);
        if (blokkolok.Length > 0)
        {
            BerlesTorlesBlokkolva(blokkolok);
            return;
        }

        long km = kijeloltek.Sum(b => (long)(b.Km ?? 0));
        string? kovetkezmeny = KiszamoltKmValtozasa(km);
        if (!TorlesMegerositve("Bérlés", "Bérlések", "RentalIcon",
                kijeloltek.Select(b => new TorlesWindow.Sor(b.Szamla, $"{b.Berlo.Nev} · {b.Auto.Tipus.Marka} {b.Auto.Tipus.Modell}")),
                kovetkezmeny)) return;

        var kulcsok = kijeloltek.Select(b => (b.KezdoDatum, b.AutoId, b.BerloId)).ToHashSet();
        try
        {
            using var tranzakcio = context.Database.BeginTransaction(IsolationLevel.Serializable);
            var aktualis = berloBerlesei.Include(b => b.Berlo).ToList();
            var torlendo = aktualis.Where(b => kulcsok.Contains((b.KezdoDatum, b.AutoId, b.BerloId))).ToArray();
            blokkolok = BerlesNelkulMaradoBerlok(torlendo, aktualis.GroupBy(b => b.BerloId).ToDictionary(g => g.Key, g => g.Count()));
            if (blokkolok.Length == 0)
            {
                context.Berlesek.RemoveRange(torlendo);
                context.SaveChanges();
                tranzakcio.Commit();
            }
        }
        catch (Exception ex)
        {
            MentesiHibaMegjelenitese("bérlés törlése", ex);
            return;
        }
        if (blokkolok.Length > 0)
        {
            BerlesTorlesBlokkolva(blokkolok);
            return;
        }
        BerlesekBetoltese();
    }

    private static IGrouping<int, Berles>[] BerlesNelkulMaradoBerlok(IEnumerable<Berles> kijeloltek, IReadOnlyDictionary<int, int> osszes) =>
        kijeloltek.GroupBy(b => b.BerloId)
            .Where(g => g.Count() >= osszes.GetValueOrDefault(g.Key)).ToArray();

    private static string? KiszamoltKmValtozasa(long km) => km > 0
        ? $"Az Autók nézetben a nyilvántartott bérlések alapján számolt futásteljesítmény összesen {km:N0} km-rel csökken."
        : null;

    private void BerlesTorlesBlokkolva(IReadOnlyCollection<IGrouping<int, Berles>> blokkolok)
    {
        var sorok = blokkolok.OrderBy(g => g.First().Berlo.Nev).ThenBy(g => g.Key)
            .Select(g => $"• {g.First().Berlo.Nev} (#{g.Key}) · {string.Join(", ", g.Select(b => b.Szamla).Order())}");
        string bevezeto = blokkolok.Count == 1
            ? "A kijelölés az alábbi bérlő összes bérlését tartalmazza. Legalább egy bérlését meg kell tartani."
            : "A kijelölés az alábbi bérlők összes bérlését tartalmazza. Mindegyiküknél legalább egy bérlést meg kell tartani.";
        string uzenet = bevezeto + "\n\n" + string.Join("\n", sorok)
            + "\n\nA bérlő és az összes bérlése együtt a Bérlők nézetből törölhető.";
        App.Uzenet(this, uzenet, "A törlés nem lehetséges");
    }
}
