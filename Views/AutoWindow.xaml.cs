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

namespace Autokolcsonzo.Views;

public partial class AutoWindow : Window
{
    public Auto Eredmeny { get; private set; }

    public AutoWindow(Auto? meglevo = null)
    {
        InitializeComponent();
        
        using (var context = new AutoKolcsonzoContext())
        {
            TipusComboBox.ItemsSource = context.Tipusok
                .OrderBy(t => t.Marka).ThenBy(t => t.Modell).ToList();
        }

        if (meglevo == null)
        {
            Title = "Új autó";
            Eredmeny = new Auto();
            AlapKmTextBox.Text = "0";
        }
        else
        {
            Title = "Autó szerkesztése";
            Eredmeny = new Auto { AutoId = meglevo.AutoId };
            AlapKmTextBox.Text = meglevo.AlapKm.ToString();
            NapiArTextBox.Text = meglevo.NapiAr.ToString();
            MegtettKmTextBox.Text = $"{meglevo.MegtettKm:N0} km";
            
            TipusComboBox.SelectedItem = ((IEnumerable<Tipus>)TipusComboBox.ItemsSource)
                .FirstOrDefault(t => t.TipusId == meglevo.TipusId);
        }
    }

    private void Mentes_Click(object sender, RoutedEventArgs e)
    {
        if (TipusComboBox.SelectedItem is not Tipus tipus)
        {
            App.Uzenet(this, "Válasszon típust! Ha még nincs típus, előbb vegyen fel egyet a Típusok fülön.", "Hibás adat");
            return;
        }

        if (!int.TryParse(AlapKmTextBox.Text.Trim(), out int alapKm) || alapKm < 0)
        {
            App.Uzenet(this, "Az alap km nulla vagy pozitív egész szám lehet.", "Hibás adat");
            return;
        }

        if (!int.TryParse(NapiArTextBox.Text.Trim(), out int napiAr) || napiAr <= 0 || napiAr > 1_000_000)
        {
            App.Uzenet(this, "A napi ár 1 és 1 000 000 közötti egész szám kell legyen.", "Hibás adat");
            return;
        }

        Eredmeny.TipusId = tipus.TipusId;
        Eredmeny.AlapKm = alapKm;
        Eredmeny.NapiAr = napiAr;

        DialogResult = true;
    }
}
