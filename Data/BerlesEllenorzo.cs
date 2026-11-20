using System;
using System.Collections.Generic;
using System.Text;
using Autokolcsonzo.Models;
using Microsoft.EntityFrameworkCore;

namespace Autokolcsonzo.Data;

public static class BerlesEllenorzo
{    
    public static string? UtkozesHiba(AutoKolcsonzoContext context, int autoId,
                                      DateTime kezdo, DateTime? veg, Berles? kiveve = null)
    {
        DateTime ujKezdo = kezdo.Date;
        DateTime ujVeg = veg?.Date ?? DateTime.MaxValue;

        var masok = context.Berlesek.Include(b => b.Berlo).Where(b => b.AutoId == autoId).ToList();

        foreach (var b in masok)
        {
            if (kiveve != null && b.KezdoDatum == kiveve.KezdoDatum && b.BerloId == kiveve.BerloId)
                continue;

            DateTime bVeg = b.VegDatum?.Date ?? DateTime.MaxValue;
           
            if (ujKezdo <= bVeg && b.KezdoDatum.Date <= ujVeg)
            {
                string idoszak = b.VegDatum.HasValue
                    ? $"{b.KezdoDatum:yyyy.MM.dd} – {b.VegDatum:yyyy.MM.dd}"
                    : $"{b.KezdoDatum:yyyy.MM.dd} – (folyamatban)";
                return $"Az autó ebben az időszakban már foglalt: {b.Berlo.Nev}, {idoszak}.";
            }
        }
        return null;
    }
}
