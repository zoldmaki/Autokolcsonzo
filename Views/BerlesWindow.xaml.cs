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

        if (meglevo == null)
        {
            using var context = new AutoKolcsonzoContext();
            BerloComboBox.ItemsSource = BerlesEllenorzo.SzabadBerlok(context).OrderBy(b => b.Nev).ToList();
            AutoComboBox.ItemsSource = BerlesEllenorzo.SzabadAutok(context).Include(a => a.Tipus)
                .OrderBy(a => a.AutoId).ToList();

            Title = "Új bérlés";
            KezdoDatePicker.SelectedDate = DateTime.Today;
        }
        else
        {
            Title = "Bérlés szerkesztése";

            BerloComboBox.ItemsSource = new[] { meglevo.Berlo };
            BerloComboBox.SelectedIndex = 0;
            AutoComboBox.ItemsSource = new[] { meglevo.Auto };
            AutoComboBox.SelectedIndex = 0;
            KezdoDatePicker.SelectedDate = meglevo.KezdoDatum;

            BerloComboBox.IsEnabled = false;
            AutoComboBox.IsEnabled = false;
            KezdoDatePicker.IsEnabled = false;
            ZarolasTextBlock.Visibility = Visibility.Visible;
            SzamlaTextBox.Text = meglevo.Szamla;

            VegDatePicker.SelectedDate = meglevo.VegDatum;
            if (meglevo.Km.HasValue) KmTextBox.Text = meglevo.Km.Value.ToString();
        }
    }

    private void Mentes_Click(object sender, RoutedEventArgs e)
    {
        if (BerloComboBox.SelectedItem is not Berlo berlo)
        {
            App.Uzenet(this, "Válasszon bérlőt!", "Hibás adat");
            return;
        }
        if (AutoComboBox.SelectedItem is not Auto auto)
        {
            App.Uzenet(this, "Válasszon autót!", "Hibás adat");
            return;
        }
        if (KezdoDatePicker.SelectedDate is not DateTime kezdo)
        {
            App.Uzenet(this, "A kezdő dátum megadása kötelező.", "Hibás adat");
            return;
        }

        DateTime? veg = VegDatePicker.SelectedDate;

        int? km = null;
        string kmSzoveg = KmTextBox.Text.Trim();
        if (kmSzoveg != "")
        {
            if (!int.TryParse(kmSzoveg, out int kmErtek))
            {
                App.Uzenet(this, "A kilométer egész szám kell legyen.", "Hibás adat");
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
            App.Uzenet(this, hiba, "Hibás adat");
            return;
        }

        if (meglevo == null && Berlo.KorEgyNapon(berlo.SzulDatum, kezdo) < Berlo.MinKor)
        {
            App.Uzenet(this, $"A bérlőnek a bérlés kezdetén legalább {Berlo.MinKor} évesnek kell lennie.", "Hibás adat");
            return;
        }

        using (var context = new AutoKolcsonzoContext())
        {
            string? utkozes = BerlesEllenorzo.UtkozesHiba(context, berles, meglevo != null);
            if (utkozes != null)
            {
                App.Uzenet(this, utkozes, "Ütköző bérlés");
                return;
            }
        }

        Eredmeny = berles;
        DialogResult = true;
    }
}
