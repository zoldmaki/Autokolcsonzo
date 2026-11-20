using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace Autokolcsonzo.Models;

public class Auto
{
    public int AutoId { get; set; }
    public int AlapKm { get; set; }              
    public int NapiAr { get; set; }

    public int TipusId { get; set; }
    public Tipus Tipus { get; set; } = null!;

    public ICollection<Berles> Berlesek { get; set; } = new List<Berles>();

    [NotMapped]
    public int MegtettKm => AlapKm + Berlesek.Sum(b => b.Km ?? 0);

    [NotMapped]
    public string Megnevezes => $"#{AutoId} – {Tipus?.Marka} {Tipus?.Modell}";
}