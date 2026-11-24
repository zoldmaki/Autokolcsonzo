using Autokolcsonzo.Models;

namespace Autokolcsonzo.Data;

public static class DbSeeder
{
    public static void Seed(AutoKolcsonzoContext context)
    {
        if (context.Varosok.Any() || context.Tipusok.Any() || context.Autok.Any()
            || context.Berlok.Any() || context.Berlesek.Any())
            return;

        //Varosok
        var budapest = new Varos { Nev = "Budapest", IranySzam = "1051", Megye = "Budapest" };
        var szeged = new Varos { Nev = "Szeged", IranySzam = "6720", Megye = "Csongrád-Csanád" };
        var nagykoros = new Varos { Nev = "Nagykőrös", IranySzam = "2750", Megye = "Pest" };
        var kecskemet = new Varos { Nev = "Kecskemét", IranySzam = "6000", Megye = "Bács-Kiskun" };
        var debrecen = new Varos { Nev = "Debrecen", IranySzam = "4024", Megye = "Hajdú-Bihar" };
        var pecs = new Varos { Nev = "Pécs", IranySzam = "7621", Megye = "Baranya" };
        var gyor = new Varos { Nev = "Győr", IranySzam = "9021", Megye = "Győr-Moson-Sopron" };
        var miskolc = new Varos { Nev = "Miskolc", IranySzam = "3525", Megye = "Borsod-Abaúj-Zemplén" };
        var nyiregyhaza = new Varos { Nev = "Nyíregyháza", IranySzam = "4400", Megye = "Szabolcs-Szatmár-Bereg" };
        var szekesfehervar = new Varos { Nev = "Székesfehérvár", IranySzam = "8000", Megye = "Fejér" };
        var veszprem = new Varos { Nev = "Veszprém", IranySzam = "8200", Megye = "Veszprém" };
        var eger = new Varos { Nev = "Eger", IranySzam = "3300", Megye = "Heves" };
        var szolnok = new Varos { Nev = "Szolnok", IranySzam = "5000", Megye = "Jász-Nagykun-Szolnok" };
        var szombathely = new Varos { Nev = "Szombathely", IranySzam = "9700", Megye = "Vas" };
        var kaposvar = new Varos { Nev = "Kaposvár", IranySzam = "7400", Megye = "Somogy" };
        var bekescsaba = new Varos { Nev = "Békéscsaba", IranySzam = "5600", Megye = "Békés" };

        //Tipusok
        var yaris = new Tipus { Marka = "Toyota", Modell = "Yaris", Evjarat = 2020, Fogyasztas = 4.5, Uzemanyag = "Benzin" };
        var golf = new Tipus { Marka = "Volkswagen", Modell = "Golf", Evjarat = 2018, Fogyasztas = 5.2, Uzemanyag = "Dízel" };
        var octavia = new Tipus { Marka = "Škoda", Modell = "Octavia", Evjarat = 2019, Fogyasztas = 5.8, Uzemanyag = "Benzin" };
        var twingo = new Tipus { Marka = "Renault", Modell = "Twingo", Evjarat = 2021, Fogyasztas = 4.2, Uzemanyag = "Benzin" };
        var focus = new Tipus { Marka = "Ford", Modell = "Focus", Evjarat = 2017, Fogyasztas = 5.1, Uzemanyag = "Dízel" };
        var swift = new Tipus { Marka = "Suzuki", Modell = "Swift", Evjarat = 2022, Fogyasztas = 4.9, Uzemanyag = "Benzin" };
        var ceed = new Tipus { Marka = "Kia", Modell = "Ceed", Evjarat = 2021, Fogyasztas = 6.1, Uzemanyag = "Benzin" };
        var i30 = new Tipus { Marka = "Hyundai", Modell = "i30", Evjarat = 2020, Fogyasztas = 6.3, Uzemanyag = "Benzin" };
        var jazz = new Tipus { Marka = "Honda", Modell = "Jazz", Evjarat = 2023, Fogyasztas = 4.6, Uzemanyag = "Hibrid" };
        var duster = new Tipus { Marka = "Dacia", Modell = "Duster", Evjarat = 2019, Fogyasztas = 5.7, Uzemanyag = "Dízel" };
        var bmw320d = new Tipus { Marka = "BMW", Modell = "320d", Evjarat = 2018, Fogyasztas = 5.4, Uzemanyag = "Dízel" };
        var astra = new Tipus { Marka = "Opel", Modell = "Astra", Evjarat = 2016, Fogyasztas = 6.7, Uzemanyag = "Benzin" };
        var kona = new Tipus { Marka = "Hyundai", Modell = "Kona Electric", Evjarat = 2022, Fogyasztas = 15.2, Uzemanyag = "Elektromos" };
        var leaf = new Tipus { Marka = "Nissan", Modell = "Leaf", Evjarat = 2020, Fogyasztas = 17.1, Uzemanyag = "Elektromos" };
        var corolla = new Tipus { Marka = "Toyota", Modell = "Corolla", Evjarat = 2024, Fogyasztas = 4.8, Uzemanyag = "Hibrid" };
        var mazda3 = new Tipus { Marka = "Mazda", Modell = "3", Evjarat = 2021, Fogyasztas = 6.5, Uzemanyag = "Benzin" };

        //Autok
        var auto1 = new Auto { Tipus = yaris, AlapKm = 45000, NapiAr = 12000 };
        var auto2 = new Auto { Tipus = yaris, AlapKm = 62000, NapiAr = 11000 };
        var auto3 = new Auto { Tipus = golf, AlapKm = 87000, NapiAr = 14000 };
        var auto4 = new Auto { Tipus = golf, AlapKm = 120000, NapiAr = 12500 };
        var auto5 = new Auto { Tipus = octavia, AlapKm = 33000, NapiAr = 16000 };
        var auto6 = new Auto { Tipus = twingo, AlapKm = 25000, NapiAr = 10000 };
        var auto7 = new Auto { Tipus = focus, AlapKm = 138000, NapiAr = 12500 };
        var auto8 = new Auto { Tipus = swift, AlapKm = 26000, NapiAr = 10500 };
        var auto9 = new Auto { Tipus = ceed, AlapKm = 48000, NapiAr = 14000 };
        var auto10 = new Auto { Tipus = i30, AlapKm = 63000, NapiAr = 13500 };
        var auto11 = new Auto { Tipus = jazz, AlapKm = 19500, NapiAr = 15500 };
        var auto12 = new Auto { Tipus = duster, AlapKm = 92000, NapiAr = 14500 };
        var auto13 = new Auto { Tipus = bmw320d, AlapKm = 154000, NapiAr = 22900 };
        var auto14 = new Auto { Tipus = astra, AlapKm = 168000, NapiAr = 9900 };
        var auto15 = new Auto { Tipus = kona, AlapKm = 34000, NapiAr = 18900 };
        var auto16 = new Auto { Tipus = leaf, AlapKm = 52000, NapiAr = 16900 };
        var auto17 = new Auto { Tipus = corolla, AlapKm = 12500, NapiAr = 18500 };
        var auto18 = new Auto { Tipus = mazda3, AlapKm = 71000, NapiAr = 15800 };

        //Berlok
        var berlo1 = new Berlo { Nev = "Kovács Anna", SzulDatum = new DateTime(1995, 3, 14), Varos = budapest, UtcaNev = "Váci utca", HazSzam = "12" };
        var berlo2 = new Berlo { Nev = "Nagy Péter", SzulDatum = new DateTime(1988, 11, 2), Varos = debrecen, UtcaNev = "Piac utca", HazSzam = "5" };
        var berlo3 = new Berlo { Nev = "Szabó Eszter", SzulDatum = new DateTime(2001, 7, 21) };
        var berlo4 = new Berlo { Nev = "Tóth Gábor", SzulDatum = new DateTime(1979, 1, 30), Varos = budapest, UtcaNev = "Fő utca", HazSzam = "8" };
        var berlo5 = new Berlo { Nev = "Horváth Lilla", SzulDatum = new DateTime(1999, 9, 9), Varos = debrecen };
        var berlo6 = new Berlo { Nev = "Varga Márk", SzulDatum = new DateTime(1992, 5, 17) };
        var berlo7 = new Berlo { Nev = "Kiss Bence", SzulDatum = new DateTime(1997, 4, 6), Varos = szeged, UtcaNev = "Tisza Lajos körút", HazSzam = "18" };
        var berlo8 = new Berlo { Nev = "Molnár Júlia", SzulDatum = new DateTime(1986, 6, 13), Varos = kecskemet, UtcaNev = "Rákóczi út", HazSzam = "7/A" };
        var berlo9 = new Berlo { Nev = "Farkas Ádám", SzulDatum = new DateTime(1994, 12, 8), Varos = nagykoros };
        var berlo10 = new Berlo { Nev = "Németh Dóra", SzulDatum = new DateTime(2003, 8, 27), Varos = pecs, UtcaNev = "Király utca", HazSzam = "23" };
        var berlo11 = new Berlo { Nev = "Balogh Tamás", SzulDatum = new DateTime(1981, 2, 19), Varos = gyor, UtcaNev = "Baross Gábor út", HazSzam = "9" };
        var berlo12 = new Berlo { Nev = "Papp Réka", SzulDatum = new DateTime(1998, 10, 11), Varos = miskolc, UtcaNev = "Széchenyi István út", HazSzam = "32" };
        var berlo13 = new Berlo { Nev = "Lakatos Dávid", SzulDatum = new DateTime(1990, 5, 25), Varos = nyiregyhaza, UtcaNev = "Bessenyei tér" };
        var berlo14 = new Berlo { Nev = "Takács Nóra", SzulDatum = new DateTime(2000, 1, 16), Varos = szekesfehervar, UtcaNev = "Palotai út", HazSzam = "14/B" };
        var berlo15 = new Berlo { Nev = "Juhász Máté", SzulDatum = new DateTime(1987, 7, 3), Varos = veszprem, UtcaNev = "Kossuth Lajos utca", HazSzam = "4" };
        var berlo16 = new Berlo { Nev = "Oláh Zsófia", SzulDatum = new DateTime(1996, 11, 24), Varos = eger };
        var berlo17 = new Berlo { Nev = "Mészáros András", SzulDatum = new DateTime(1984, 9, 5), Varos = szolnok, UtcaNev = "Baross utca", HazSzam = "38" };
        var berlo18 = new Berlo { Nev = "Simon Petra", SzulDatum = new DateTime(2004, 3, 20), Varos = szombathely, UtcaNev = "Széll Kálmán utca", HazSzam = "11" };

        //Berlesek
        var berlesek = new List<Berles>
        {
            new() { Auto = auto1, Berlo = berlo1, KezdoDatum = new DateTime(2026, 1, 10), VegDatum = new DateTime(2026, 1, 15), Km = 620, Szamla = "2026000001" },
            new() { Auto = auto2, Berlo = berlo2, KezdoDatum = new DateTime(2026, 2, 3), VegDatum = new DateTime(2026, 2, 5), Km = 310, Szamla = "2026000002" },
            new() { Auto = auto3, Berlo = berlo3, KezdoDatum = new DateTime(2026, 3, 20), VegDatum = new DateTime(2026, 3, 27), Km = 1150, Szamla = "2026000003" },
            new() { Auto = auto1, Berlo = berlo4, KezdoDatum = new DateTime(2026, 5, 1), VegDatum = new DateTime(2026, 5, 4), Km = 480, Szamla = "2026000004" },
            new() { Auto = auto4, Berlo = berlo5, KezdoDatum = new DateTime(2026, 6, 12), VegDatum = new DateTime(2026, 6, 14), Km = 260, Szamla = "2026000005" },
            new() { Auto = auto3, Berlo = berlo6, KezdoDatum = new DateTime(2026, 8, 8), VegDatum = new DateTime(2026, 8, 22), Km = 2100, Szamla = "2026000006" },
            new() { Auto = auto2, Berlo = berlo1, KezdoDatum = new DateTime(2026, 10, 30), Szamla = "2026000007" },
            new() { Auto = auto4, Berlo = berlo2, KezdoDatum = new DateTime(2026, 11, 15), Szamla = "2026000008" },
            new() { Auto = auto5, Berlo = berlo4, KezdoDatum = new DateTime(2026, 8, 31), VegDatum = new DateTime(2026, 9, 4), Km = 540, Szamla = "2026000009" },
            new() { Auto = auto6, Berlo = berlo5, KezdoDatum = new DateTime(2026, 9, 15), VegDatum = new DateTime(2026, 9, 21), Km = 980, Szamla = "2026000010" },
            new() { Auto = auto5, Berlo = berlo6, KezdoDatum = new DateTime(2026, 11, 22), VegDatum = new DateTime(2026, 11, 27), Szamla = "2026000011" },
            new() { Auto = auto6, Berlo = berlo3, KezdoDatum = new DateTime(2026, 12, 1), VegDatum = new DateTime(2026, 12, 4), Szamla = "2026000012" },
            new() { Auto = auto7, Berlo = berlo7, KezdoDatum = new DateTime(2026, 8, 16), VegDatum = new DateTime(2026, 8, 18), Km = 180, Szamla = "2026000013" },
            new() { Auto = auto8, Berlo = berlo8, KezdoDatum = new DateTime(2026, 8, 23), VegDatum = new DateTime(2026, 8, 26), Km = 275, Szamla = "2026000014" },
            new() { Auto = auto9, Berlo = berlo9, KezdoDatum = new DateTime(2026, 8, 30), VegDatum = new DateTime(2026, 9, 3), Km = 370, Szamla = "2026000015" },
            new() { Auto = auto10, Berlo = berlo10, KezdoDatum = new DateTime(2026, 9, 6), VegDatum = new DateTime(2026, 9, 11), Km = 465, Szamla = "2026000016" },
            new() { Auto = auto11, Berlo = berlo11, KezdoDatum = new DateTime(2026, 9, 13), VegDatum = new DateTime(2026, 9, 19), Km = 560, Szamla = "2026000017" },
            new() { Auto = auto12, Berlo = berlo12, KezdoDatum = new DateTime(2026, 9, 20), VegDatum = new DateTime(2026, 9, 27), Km = 655, Szamla = "2026000018" },
            new() { Auto = auto13, Berlo = berlo13, KezdoDatum = new DateTime(2026, 9, 27), VegDatum = new DateTime(2026, 9, 29), Km = 750, Szamla = "2026000019" },
            new() { Auto = auto14, Berlo = berlo14, KezdoDatum = new DateTime(2026, 10, 4), VegDatum = new DateTime(2026, 10, 7), Km = 845, Szamla = "2026000020" },
            new() { Auto = auto15, Berlo = berlo15, KezdoDatum = new DateTime(2026, 10, 11), VegDatum = new DateTime(2026, 10, 15), Km = 940, Szamla = "2026000021" },
            new() { Auto = auto1, Berlo = berlo16, KezdoDatum = new DateTime(2026, 10, 18), VegDatum = new DateTime(2026, 10, 23), Km = 1035, Szamla = "2026000022" },
            new() { Auto = auto5, Berlo = berlo17, KezdoDatum = new DateTime(2026, 10, 25), VegDatum = new DateTime(2026, 10, 31), Km = 1130, Szamla = "2026000023" },
            new() { Auto = auto6, Berlo = berlo18, KezdoDatum = new DateTime(2026, 11, 1), VegDatum = new DateTime(2026, 11, 8), Km = 1225, Szamla = "2026000024" },
        };

        context.Varosok.AddRange(budapest, debrecen, szeged, nagykoros, kecskemet, pecs, gyor,
            miskolc, nyiregyhaza, szekesfehervar, veszprem, eger, szolnok, szombathely, kaposvar, bekescsaba);
        context.Tipusok.AddRange(yaris, golf, octavia, twingo, focus, swift, ceed, i30,
            jazz, duster, bmw320d, astra, kona, leaf, corolla, mazda3);
        context.Autok.AddRange(auto1, auto2, auto3, auto4, auto5, auto6, auto7, auto8, auto9,
            auto10, auto11, auto12, auto13, auto14, auto15, auto16, auto17, auto18);
        context.Berlok.AddRange(berlo1, berlo2, berlo3, berlo4, berlo5, berlo6, berlo7, berlo8, berlo9,
            berlo10, berlo11, berlo12, berlo13, berlo14, berlo15, berlo16, berlo17, berlo18);
        context.Berlesek.AddRange(berlesek);

        context.SaveChanges();
    }
}
