using System;
using System.Collections.Generic;
using System.Text;
namespace Autokolcsonzo.Data;

public static class SzamlaGenerator
{
    public static string Uj(AutoKolcsonzoContext context)
    {
        string szam;
        do
        {
            szam = Random.Shared.NextInt64(1_000_000_000, 10_000_000_000).ToString();
        }
        while (context.Berlesek.Any(b => b.Szamla == szam));
        return szam;
    }
}