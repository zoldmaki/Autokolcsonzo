using System;
using System.Collections.Generic;
using System.Text;
using Autokolcsonzo.Models;

namespace Autokolcsonzo.Data;

public static class DbSeeder
{
    public static void Seed(AutoKolcsonzoContext context)
    {
        if (context.Varosok.Any()) return;
        
        //Varosok
        var budapest = new Varos { Nev = "Budapest", IranySzam = "1051", Megye = "Pest" };
        var szeged = new Varos { Nev = "Szeged", IranySzam = "6720", Megye = "Csongrád" };
        var nagykőrös = new Varos { Nev = "Nagykőrös", IranySzam = "2750", Megye = "Pest" };
        var kecskemét = new Varos { Nev = "Kecskemét", IranySzam = "6000", Megye = "Bács-Kiskun" };
        var debrecen = new Varos { Nev = "Debrecen", IranySzam = "4024", Megye = "Hajdú-Bihar" };

        //Tipusok
        var yaris = new Tipus { Marka = "Toyota", Modell = "Yaris", Evjarat = 2020, Fogyasztas = 4.5, Uzemanyag = "Benzin" };
        var golf = new Tipus { Marka = "Volkswagen", Modell = "Golf", Evjarat = 2018, Fogyasztas = 5.2, Uzemanyag = "Dízel" };
        var octavia = new Tipus { Marka = "Skoda", Modell = "Octavia", Evjarat = 2019, Fogyasztas = 5.8, Uzemanyag = "Benzin" };
        var twingo = new Tipus { Marka = "Renault", Modell = "Twingo", Evjarat = 2021, Fogyasztas = 4.2, Uzemanyag = "Benzin" };

        //Autok
        var auto1 = new Auto { Tipus = yaris, AlapKm = 45000, NapiAr = 12000 };
        var auto2 = new Auto { Tipus = yaris, AlapKm = 62000, NapiAr = 11000 };
        var auto3 = new Auto { Tipus = golf, AlapKm = 87000, NapiAr = 14000 };
        var auto4 = new Auto { Tipus = golf, AlapKm = 120000, NapiAr = 12500 };
        var auto5 = new Auto { Tipus = octavia, AlapKm = 33000, NapiAr = 16000 };
        var auto6 = new Auto { Tipus = twingo, AlapKm = 25000, NapiAr = 10000 };

        //Berlok
        var berlo1 = new Berlo { Nev = "Kovács Anna", SzulDatum = new DateTime(1995, 3, 14), Varos = budapest, UtcaNev = "Váci utca", HazSzam = "12" };
        var berlo2 = new Berlo { Nev = "Nagy Péter", SzulDatum = new DateTime(1988, 11, 2), Varos = debrecen, UtcaNev = "Piac utca", HazSzam = "5" };
        var berlo3 = new Berlo { Nev = "Szabó Eszter", SzulDatum = new DateTime(2001, 7, 21) };
        var berlo4 = new Berlo { Nev = "Tóth Gábor", SzulDatum = new DateTime(1979, 1, 30), Varos = budapest, UtcaNev = "Fő utca", HazSzam = "8" };
        var berlo5 = new Berlo { Nev = "Horváth Lilla", SzulDatum = new DateTime(1999, 9, 9), Varos = debrecen };
        var berlo6 = new Berlo { Nev = "Varga Márk", SzulDatum = new DateTime(1992, 5, 17) };

        //Berlesek
        var berlesek = new List<Berles>
        {
            new() { Auto = auto1, Berlo = berlo1, KezdoDatum = new DateTime(2026, 1, 10),  VegDatum = new DateTime(2026, 1, 15),  Km = 620,  Szamla = "2026000001" },
            new() { Auto = auto2, Berlo = berlo2, KezdoDatum = new DateTime(2026, 2, 3),   VegDatum = new DateTime(2026, 2, 5),   Km = 310,  Szamla = "2026000002" },
            new() { Auto = auto3, Berlo = berlo3, KezdoDatum = new DateTime(2026, 3, 20),  VegDatum = new DateTime(2026, 3, 27),  Km = 1150, Szamla = "2026000003" },
            new() { Auto = auto1, Berlo = berlo4, KezdoDatum = new DateTime(2026, 5, 1),   VegDatum = new DateTime(2026, 5, 4),   Km = 480,  Szamla = "2026000004" },
            new() { Auto = auto4, Berlo = berlo5, KezdoDatum = new DateTime(2026, 6, 12),  VegDatum = new DateTime(2026, 6, 14),  Km = 260,  Szamla = "2026000005" },
            new() { Auto = auto3, Berlo = berlo6, KezdoDatum = new DateTime(2026, 8, 8),   VegDatum = new DateTime(2026, 8, 22),  Km = 2100, Szamla = "2026000006" },
            new() { Auto = auto2, Berlo = berlo1, KezdoDatum = new DateTime(2026, 10, 30), Szamla = "2026000007" },
            new() { Auto = auto4, Berlo = berlo2, KezdoDatum = new DateTime(2026, 11, 15), Szamla = "2026000008" },
        };

        context.Varosok.AddRange(budapest, debrecen, szeged, nagykőrös, kecskemét);
        context.Tipusok.AddRange(yaris, golf, octavia, twingo);
        context.Autok.AddRange(auto1, auto2, auto3, auto4, auto5, auto6);
        context.Berlok.AddRange(berlo1, berlo2, berlo3, berlo4, berlo5, berlo6);
        context.Berlesek.AddRange(berlesek);

        context.SaveChanges();
    }
}
