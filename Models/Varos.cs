using System;
using System.Collections.Generic;
using System.Text;

namespace Autokolcsonzo.Models;

public class Varos
{
    public int VarosId { get; set; }          
    public string Nev { get; set; } = "";
    public string IranySzam { get; set; } = "";   
    public string Megye { get; set; } = "";

    public ICollection<Berlo> Berlok { get; set; } = new List<Berlo>();
}
