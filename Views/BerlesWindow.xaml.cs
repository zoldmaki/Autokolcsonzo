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

public partial class BerlesWindow : Window
{
    public Berles Eredmeny { get; private set; } = new Berles();
    private readonly Berles? meglevo;

    public BerlesWindow(Berles? meglevo = null)
    {
        InitializeComponent();
        this.meglevo = meglevo;

        using (var context = new AutoKolcsonzoContext())
        {
            BerloComboBox.ItemsSource = context.Berlok.OrderBy(b => b.Nev).ToList();
            AutoComboBox.ItemsSource = context.Autok.Include(a => a.Tipus).OrderBy(a => a.AutoId).ToList();
        }

        if (meglevo == null)
        {
            Title = "Új bérlés";
            KezdoDatePicker.SelectedDate = DateTime.Today;
        }
        else
        {
            Title = "Bérlés szerkesztése";

            BerloComboBox.SelectedItem = ((IEnumerable<Berlo>)BerloComboBox.ItemsSource)
                .FirstOrDefault(b => b.BerloId == meglevo.BerloId);
            AutoComboBox.SelectedItem = ((IEnumerable<Auto>)AutoComboBox.ItemsSource)
                .FirstOrDefault(a => a.AutoId == meglevo.AutoId);
            KezdoDatePicker.SelectedDate = meglevo.KezdoDatum;

            BerloComboBox.IsEnabled = false;
            AutoComboBox.IsEnabled = false;
            KezdoDatePicker.IsEnabled = false;
            ZarolasTextBlock.Visibility = Visibility.Visible;
            SzamlaTextBlock.Text = meglevo.Szamla;

            VegDatePicker.SelectedDate = meglevo.VegDatum;
            if (meglevo.Km.HasValue) KmTextBox.Text = meglevo.Km.Value.ToString();
        }
    }

    private void VegDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
    {
        bool lezart = VegDatePicker.SelectedDate is DateTime veg && veg.Date <= DateTime.Today;
        KmTextBox.IsEnabled = lezart;
        if (!lezart) KmTextBox.Text = "";
    }

    private void Mentes_Click(object sender, RoutedEventArgs e)
    {
        if (BerloComboBox.SelectedItem is not Berlo berlo)
        {
            MessageBox.Show("Válasszon bérlőt!", "Hibás adat");
            return;
        }
        if (AutoComboBox.SelectedItem is not Auto auto)
        {
            MessageBox.Show("Válasszon autót!", "Hibás adat");
            return;
        }
        if (KezdoDatePicker.SelectedDate is not DateTime kezdo)
        {
            MessageBox.Show("A kezdő dátum megadása kötelező.", "Hibás adat");
            return;
        }

        DateTime? veg = VegDatePicker.SelectedDate;

        int? km = null;
        string kmSzoveg = KmTextBox.Text.Trim();
        if (kmSzoveg != "")
        {
            if (!int.TryParse(kmSzoveg, out int kmErtek))
            {
                MessageBox.Show("A kilométer egész szám kell legyen.", "Hibás adat");
                return;
            }
            km = kmErtek;
        }

        var berles = new Berles
        {
            KezdoDatum = kezdo,
            AutoId = auto.AutoId,
            BerloId = berlo.BerloId,
            VegDatum = veg,
            Km = km,
            Szamla = meglevo?.Szamla ?? ""
        };

        string? hiba = berles.Ellenorzes();       

        if (hiba != null)
        {
            MessageBox.Show(hiba, "Hibás adat");
            return;
        }

        if (meglevo == null && Berlo.KorEgyNapon(berlo.SzulDatum, kezdo) < Berlo.MinKor)
        {
            MessageBox.Show($"A bérlőnek a bérlés kezdetén legalább {Berlo.MinKor} évesnek kell lennie.", "Hibás adat");
            return;
        }

        using (var context = new AutoKolcsonzoContext())
        {
            string? utkozes = BerlesEllenorzo.UtkozesHiba(context, auto.AutoId, kezdo, veg, meglevo);
            if (utkozes != null)
            {
                MessageBox.Show(utkozes, "Foglalt autó");
                return;
            }
        }

        Eredmeny = berles;
        DialogResult = true;
    }
}