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
        VarosokBetoltese();
    }

    // ---------- VÁROSOK ----------

    private void VarosokBetoltese()
    {
        using var context = new AutoKolcsonzoContext();
        string szuroVaros = VarosNevFilterTextBox.Text.Trim();
        string szuroIranySzam = VarosIranySzamFilterTextBox.Text.Trim();
        string szuroMegye = VarosMegyeFilterTextBox.Text.Trim();

        IQueryable<Varos> lekerdezes = context.Varosok.Include(v => v.Berlok);

        lekerdezes = lekerdezes.Where(v => (szuroVaros!=""? v.Nev.Contains(szuroVaros) : true)
                                        && (szuroIranySzam !=""? v.IranySzam.Contains(szuroIranySzam) : true)
                                        && (szuroMegye!=""? v.Megye.Contains(szuroMegye) : true));        

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
            context.SaveChanges();
            VarosokBetoltese();
        }
    }

    private void SzerkesztVaros_Click(object sender, RoutedEventArgs e)
    {
        VarosSzerkesztese();
    }

    private void VarosDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        var dataGrid = (DataGrid)sender;
        if (dataGrid.SelectedIndex == -1)
            return;
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
            context.Varosok.Update(ablak.Eredmeny);
            context.SaveChanges();
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
            context.SaveChanges();
        }
        VarosokBetoltese();
    }
}