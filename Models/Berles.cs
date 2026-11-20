using System;
using System.Collections.Generic;
using System.Text;

using System.ComponentModel.DataAnnotations.Schema;

namespace Autokolcsonzo.Models;

public class Berles
{    
    public DateTime KezdoDatum { get; set; }
    public int AutoId { get; set; }
    public Auto Auto { get; set; } = null!;

    public int BerloId { get; set; }
    public Berlo Berlo { get; set; } = null!;
    public string Szamla { get; set; } = null!;
    public DateTime? VegDatum { get; set; }      
    public int? Km { get; set; }                  

    [NotMapped]
    public int? Hossz => VegDatum.HasValue ? (VegDatum.Value - KezdoDatum).Days : null;

    [NotMapped]
    public int? Ar => Hossz.HasValue ? Hossz.Value * Auto.NapiAr : null;

    public string? Ellenorzes()
    {
        if (VegDatum.HasValue && VegDatum.Value.Date < KezdoDatum.Date)
            return "A vég dátum nem lehet a kezdő dátum előtt.";

        if (Km.HasValue)
        {
            if (!VegDatum.HasValue)
                return "Kilométer csak vég dátummal együtt adható meg.";
            if (VegDatum.Value.Date > DateTime.Today)
                return "Kilométer csak akkor adható meg, ha a vég dátum már elmúlt (vagy ma van).";
            if (Km.Value < 0)
                return "A kilométer nem lehet negatív.";
        }
        return null;
    }

    [NotMapped]
    public string Allapot => VegDatum.HasValue ? "Lezárt" : "Folyamatban";
}