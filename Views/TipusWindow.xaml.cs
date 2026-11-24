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
using System.Globalization;
using Autokolcsonzo.Models;

namespace Autokolcsonzo.Views;

public partial class TipusWindow : Window
{
    public Tipus Eredmeny { get; private set; }

    public TipusWindow(Tipus? meglevo = null)
    {
        InitializeComponent();

        UzemanyagComboBox.ItemsSource = Tipus.Uzemanyagok;

        if (meglevo == null)
        {
            Title = "Új típus";
            Eredmeny = new Tipus();
            UzemanyagComboBox.SelectedIndex = 0;
        }
        else
        {
            Title = "Típus szerkesztése";
            Eredmeny = new Tipus { TipusId = meglevo.TipusId };
            MarkaTextBox.Text = meglevo.Marka;
            ModellTextBox.Text = meglevo.Modell;
            EvjaratTextBox.Text = meglevo.Evjarat.ToString();
            FogyasztasTextBox.Text = meglevo.Fogyasztas.ToString(CultureInfo.CurrentCulture);
            UzemanyagComboBox.SelectedItem = meglevo.Uzemanyag;
        }
    }

    private void Mentes_Click(object sender, RoutedEventArgs e)
    {
        string marka = MarkaTextBox.Text.Trim();
        string modell = ModellTextBox.Text.Trim();

        if (marka == "" || modell == "")
        {
            App.Uzenet(this, "A márka és a modell megadása kötelező.", "Hibás adat");
            return;
        }

        if (!int.TryParse(EvjaratTextBox.Text.Trim(), out int evjarat)
            || evjarat < 1950 || evjarat > DateTime.Today.Year + 1)
        {
            App.Uzenet(this, $"Az évjárat 1950 és {DateTime.Today.Year + 1} közötti egész szám kell legyen.", "Hibás adat");
            return;
        }

        string szoveg = FogyasztasTextBox.Text.Trim().Replace(',', '.');
        if (!double.TryParse(szoveg, NumberStyles.Float, CultureInfo.InvariantCulture, out double fogyasztas)
            || fogyasztas <= 0 || fogyasztas > 50)
        {
            App.Uzenet(this, "A fogyasztás 0 és 50 közötti szám kell legyen.", "Hibás adat");
            return;
        }

        if (UzemanyagComboBox.SelectedItem is not string uzemanyag)
        {
            App.Uzenet(this, "Válasszon üzemanyagot!", "Hibás adat");
            return;
        }

        Eredmeny.Marka = marka;
        Eredmeny.Modell = modell;
        Eredmeny.Evjarat = evjarat;
        Eredmeny.Fogyasztas = fogyasztas;
        Eredmeny.Uzemanyag = uzemanyag;

        DialogResult = true;
    }
}
