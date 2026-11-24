using Autokolcsonzo.Models;
using Microsoft.EntityFrameworkCore;

namespace Autokolcsonzo.Data;

public static class BerlesEllenorzo
{
    private static IQueryable<Berles> AktualisBerlesek(AutoKolcsonzoContext context)
    {
        DateTime ma = DateTime.Today;
        return context.Berlesek.Where(b => b.KezdoDatum <= ma && (b.VegDatum == null || b.VegDatum >= ma));
    }

    public static IQueryable<Auto> SzabadAutok(AutoKolcsonzoContext context)
    {
        var aktualis = AktualisBerlesek(context);
        return context.Autok.Where(a => !aktualis.Any(b => b.AutoId == a.AutoId));
    }

    public static IQueryable<Berlo> SzabadBerlok(AutoKolcsonzoContext context)
    {
        var aktualis = AktualisBerlesek(context);
        return context.Berlok.Where(r => !aktualis.Any(b => b.BerloId == r.BerloId));
    }

    public static string? UtkozesHiba(AutoKolcsonzoContext context, Berles berles, bool szerkesztes = false)
    {
        int autoId = berles.AutoId;
        int berloId = berles.BerloId;
        DateTime kezdo = berles.KezdoDatum.Date;
        DateTime veg = berles.VegDatum?.Date ?? DateTime.MaxValue.Date;

        if (!szerkesztes)
        {
            var aktualis = AktualisBerlesek(context)
                .Where(b => b.AutoId == autoId || b.BerloId == berloId)
                .OrderByDescending(b => b.AutoId == autoId)
                .FirstOrDefault();
            if (aktualis != null)
                return aktualis.AutoId == autoId
                    ? "A kiválasztott autó jelenleg bérlés alatt van."
                    : "A kiválasztott bérlőnek jelenleg folyamatban van egy bérlése.";
        }

        var masok = context.Berlesek.Where(b => b.AutoId == autoId || b.BerloId == berloId);
        if (szerkesztes)
            masok = masok.Where(b => b.KezdoDatum != kezdo || b.AutoId != autoId || b.BerloId != berloId);

        var utkozes = masok
            .Include(b => b.Berlo)
            .Where(b => b.KezdoDatum <= veg && (b.VegDatum == null || b.VegDatum >= kezdo))
            .OrderByDescending(b => b.AutoId == autoId)
            .ThenBy(b => b.KezdoDatum)
            .FirstOrDefault();
        if (utkozes == null) return null;

        string idoszak = utkozes.VegDatum.HasValue
            ? $"{utkozes.KezdoDatum:yyyy.MM.dd} – {utkozes.VegDatum:yyyy.MM.dd}"
            : $"{utkozes.KezdoDatum:yyyy.MM.dd}-tól";
        return utkozes.AutoId == autoId
            ? $"Az autó ebben az időszakban már foglalt: {utkozes.Berlo.Nev}, {idoszak}."
            : $"A bérlőnek ebben az időszakban már van bérlése: {idoszak}.";
    }
}
