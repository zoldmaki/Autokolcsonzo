using System;
using System.Collections.Generic;
using System.Text;

namespace Autokolcsonzo.Models;

public class Tipus
{
    public int TipusId { get; set; }
    public int Evjarat { get; set; }
    public string Marka { get; set; } = "";
    public string Modell { get; set; } = "";
    public double Fogyasztas { get; set; }
    public string Uzemanyag { get; set; } = "";

    public ICollection<Auto> Autok { get; set; } = new List<Auto>();
}