using System.Data;
using Autokolcsonzo.Models;
using Microsoft.EntityFrameworkCore;

namespace Autokolcsonzo.Data;

internal sealed record OsszesAdatSor(string Forras, Berles? Berles, Berlo? Berlo, Varos? Varos, Auto? Auto, Tipus? Tipus);

internal static class OsszesAdatok
{
    internal static IReadOnlyList<OsszesAdatSor> Betolt(AutoKolcsonzoContext context)
    {
        using var tranzakcio = context.Database.BeginTransaction(IsolationLevel.Serializable);
        var varosok = context.Varosok.OrderBy(v => v.VarosId).ToList();
        var tipusok = context.Tipusok.OrderBy(t => t.TipusId).ToList();
        var autok = context.Autok.OrderBy(a => a.AutoId).ToList();
        var berlok = context.Berlok.OrderBy(b => b.BerloId).ToList();
        var berlesek = context.Berlesek.OrderByDescending(b => b.KezdoDatum).ThenBy(b => b.Szamla).ToList();
        tranzakcio.Commit();

        // The tracked entities share their relationships, including the collections used for computed totals.
        var sorok = berlesek.Select(b => new OsszesAdatSor("Bérlés", b, b.Berlo, b.Berlo.Varos, b.Auto, b.Auto.Tipus)).ToList();
        sorok.AddRange(berlok.Where(b => b.Berlesek.Count == 0)
            .Select(b => new OsszesAdatSor("Bérlő", null, b, b.Varos, null, null)));
        sorok.AddRange(autok.Where(a => a.Berlesek.Count == 0)
            .Select(a => new OsszesAdatSor("Autó", null, null, null, a, a.Tipus)));
        sorok.AddRange(tipusok.Where(t => t.Autok.Count == 0)
            .Select(t => new OsszesAdatSor("Típus", null, null, null, null, t)));
        sorok.AddRange(varosok.Where(v => v.Berlok.Count == 0)
            .Select(v => new OsszesAdatSor("Város", null, null, v, null, null)));
        return sorok;
    }
}
