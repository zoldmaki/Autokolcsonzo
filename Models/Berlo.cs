using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace Autokolcsonzo.Models;

public class Berlo
{
    public const int MinKor = 16;

    public int BerloId { get; set; }
    public string Nev { get; set; } = "";
    public DateTime SzulDatum { get; set; }     
    public string? UtcaNev { get; set; }
    public string? HazSzam { get; set; }
    public int? VarosId { get; set; }             
    public Varos? Varos { get; set; }

    public ICollection<Berles> Berlesek { get; set; } = new List<Berles>();

    [NotMapped]
    public int Eletkor
    {
        get
        {
            var ma = DateTime.Today;
            int kor = ma.Year - SzulDatum.Year;
            if (SzulDatum.Date > ma.AddYears(-kor)) kor--;
            return kor;
        }
    }
    public string? Ellenorzes()
    {
        if (string.IsNullOrWhiteSpace(Nev))
            return "A név megadása kötelező.";
        if (SzulDatum.Date > DateTime.Today)
            return "A születési dátum nem lehet a jövőben.";
        if (Eletkor < MinKor)
            return $"A bérlőnek legalább {MinKor} évesnek kell lennie.";
        return null;
    }
}
