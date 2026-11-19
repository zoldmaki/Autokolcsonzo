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
}