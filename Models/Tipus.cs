using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace Autokolcsonzo.Models;

public class Tipus
{
    public int TipusId { get; set; }
    public int Evjarat { get; set; }
    public string Marka { get; set; } = "";
    public string Modell { get; set; } = "";
    public double Fogyasztas { get; set; }
    public string Uzemanyag { get; set; } = "";
    public static readonly string[] Uzemanyagok = { "Benzin", "Dízel", "Hibrid", "Elektromos" };
    public ICollection<Auto> Autok { get; set; } = new List<Auto>();

    [NotMapped]
    public string Megnevezes => $"{Marka} {Modell} ({Evjarat}, {Uzemanyag})";
}