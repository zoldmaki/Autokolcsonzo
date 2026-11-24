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

public partial class VarosWindow : Window
{    
    public Varos Eredmeny { get; private set; }    
    public VarosWindow(Varos? meglevo = null)
    {
        InitializeComponent();

        if (meglevo == null)
        {
            Title = "Új város";
            Eredmeny = new Varos();
        }
        else
        {
            Title = "Város szerkesztése";            
            Eredmeny = new Varos { VarosId = meglevo.VarosId };
            NevTextBox.Text = meglevo.Nev;
            IranySzamTextBox.Text = meglevo.IranySzam;
            MegyeTextBox.Text = meglevo.Megye;
        }
    }

    private void Mentes_Click(object sender, RoutedEventArgs e)
    {
        string nev = NevTextBox.Text.Trim();
        string iranySzam = IranySzamTextBox.Text.Trim();
        string megye = MegyeTextBox.Text.Trim();

        if (nev == "" || megye == "")
        {
            App.Uzenet(this, "A név és a megye megadása kötelező.", "Hibás adat");
            return;
        }
        if (iranySzam.Length != 4 || !iranySzam.All(char.IsDigit))
        {
            App.Uzenet(this, "Az irányítószám pontosan 4 számjegy kell legyen.", "Hibás adat");
            return;
        }
        
        using var context = new AutoKolcsonzoContext();
        bool foglalt = context.Varosok.Any(v => v.IranySzam == iranySzam && v.VarosId != Eredmeny.VarosId);
        if (foglalt)
        {
            App.Uzenet(this, "Ez az irányítószám már szerepel az adatbázisban.", "Hibás adat");
            return;
        }

        Eredmeny.Nev = nev;
        Eredmeny.IranySzam = iranySzam;
        Eredmeny.Megye = megye;

        DialogResult = true; 
    }
}
