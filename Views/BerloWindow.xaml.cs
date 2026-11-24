using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Autokolcsonzo.Data;
using Autokolcsonzo.Models;
using Microsoft.EntityFrameworkCore;

namespace Autokolcsonzo.Views;

public partial class BerloWindow : Window
{
    public Berlo Eredmeny { get; private set; }
    public Berles? ElsoBerles { get; private set; }
    private readonly bool ujBerlo;

    public BerloWindow(Berlo? meglevo = null)
    {
        InitializeComponent();
        ujBerlo = meglevo == null;
        Eredmeny = new Berlo { BerloId = meglevo?.BerloId ?? 0 };

        using (var context = new AutoKolcsonzoContext())
        {           
            var varosok = context.Varosok.OrderBy(v => v.Nev).ToList();
            varosok.Insert(0, new Varos { VarosId = 0, Nev = "(nincs megadva)" });
            VarosComboBox.ItemsSource = varosok;

            if (ujBerlo)
            {
                ElsoAutoComboBox.ItemsSource = BerlesEllenorzo.SzabadAutok(context)
                    .Include(a => a.Tipus)
                    .OrderBy(a => a.AutoId).ToList();
            }
        }

        SzulDatumPicker.DisplayDateEnd = DateTime.Today;

        if (ujBerlo)
        {
            Title = "Új bérlő";
            VarosComboBox.SelectedIndex = 0;
            ElsoKezdoDatePicker.SelectedDate = DateTime.Today;
        }
        else
        {
            Title = "Bérlő szerkesztése";
            ElsoBerlesGroupBox.Visibility = Visibility.Collapsed;

            NevTextBox.Text = meglevo!.Nev;
            SzulDatumPicker.SelectedDate = meglevo.SzulDatum;
            UtcaTextBox.Text = meglevo.UtcaNev;
            HazSzamTextBox.Text = meglevo.HazSzam;
            VarosComboBox.SelectedItem = ((IEnumerable<Varos>)VarosComboBox.ItemsSource)
                .FirstOrDefault(v => v.VarosId == (meglevo.VarosId ?? 0));
        }
    }

    private void ElsoVegDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
    {
        bool lezart = ElsoVegDatePicker.SelectedDate is DateTime veg && veg.Date <= DateTime.Today;
        ElsoKmTextBox.IsEnabled = lezart;
        if (!lezart) ElsoKmTextBox.Text = "";
    }

    private void Mentes_Click(object sender, RoutedEventArgs e)
    {
        if (SzulDatumPicker.SelectedDate is not DateTime szulDatum)
        {
            MessageBox.Show("A születési dátum megadása kötelező.", "Hibás adat");
            return;
        }        
        string? utca = string.IsNullOrWhiteSpace(UtcaTextBox.Text) ? null : UtcaTextBox.Text.Trim();
        string? hazSzam = string.IsNullOrWhiteSpace(HazSzamTextBox.Text) ? null : HazSzamTextBox.Text.Trim();
        var varos = VarosComboBox.SelectedItem as Varos;
        int? varosId = (varos == null || varos.VarosId == 0) ? null : varos.VarosId;

        Eredmeny.Nev = NevTextBox.Text.Trim();
        Eredmeny.SzulDatum = szulDatum;
        Eredmeny.UtcaNev = utca;
        Eredmeny.HazSzam = hazSzam;
        Eredmeny.VarosId = varosId;
        
        string? hiba = Eredmeny.Ellenorzes();
        if (hiba != null)
        {
            MessageBox.Show(hiba, "Hibás adat");
            return;
        }

        if (hazSzam != null && utca == null)
        {
            MessageBox.Show("Házszám csak utcanévvel együtt adható meg.", "Hibás adat");
            return;
        }
        if (utca != null && varosId == null)
        {
            MessageBox.Show("Utcanév megadásához válasszon várost is.", "Hibás adat");
            return;
        }

        if (ujBerlo)
        {
            if (ElsoAutoComboBox.SelectedItem is not Auto auto)
            {
                MessageBox.Show("Új bérlőhöz meg kell adni az első bérlését is: válasszon autót!", "Hibás adat");
                return;
            }
            if (ElsoKezdoDatePicker.SelectedDate is not DateTime kezdo)
            {
                MessageBox.Show("Adja meg a bérlés kezdő dátumát!", "Hibás adat");
                return;
            }

            if (Berlo.KorEgyNapon(szulDatum, kezdo) < Berlo.MinKor)
            {
                MessageBox.Show($"A bérlőnek a bérlés kezdetén legalább {Berlo.MinKor} évesnek kell lennie.", "Hibás adat");
                return;
            }

            DateTime? vegDatum = ElsoVegDatePicker.SelectedDate;

            int? km = null;
            string kmSzoveg = ElsoKmTextBox.Text.Trim();
            if (kmSzoveg != "")
            {
                if (!int.TryParse(kmSzoveg, out int kmErtek))
                {
                    MessageBox.Show("A kilométer egész szám kell legyen.", "Hibás adat");
                    return;
                }
                km = kmErtek;
            }

            var berles = new Berles { AutoId = auto.AutoId, KezdoDatum = kezdo, VegDatum = vegDatum, Km = km };

            string? berlesHiba = berles.Ellenorzes();
            if (berlesHiba != null)
            {
                MessageBox.Show(berlesHiba, "Hibás adat");
                return;
            }

            using (var context = new AutoKolcsonzoContext())
            {
                string? utkozes = BerlesEllenorzo.UtkozesHiba(context, berles);
                if (utkozes != null)
                {
                    MessageBox.Show(utkozes, "Foglalt autó");
                    return;
                }
            }

            ElsoBerles = berles;
        }

        DialogResult = true;
    }
}
