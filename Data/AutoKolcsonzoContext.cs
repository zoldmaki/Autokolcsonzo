using System;
using System.Collections.Generic;
using System.Text;

using Autokolcsonzo.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Autokolcsonzo.Data;

public class AutoKolcsonzoContext : DbContext
{
    public DbSet<Varos> Varosok { get; set; }
    public DbSet<Tipus> Tipusok { get; set; }
    public DbSet<Auto> Autok { get; set; }
    public DbSet<Berlo> Berlok { get; set; }
    public DbSet<Berles> Berlesek { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)      
            .AddJsonFile("appsettings.json")
            .Build();

        optionsBuilder.UseSqlServer(config.GetConnectionString("DefaultConnection"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {        
        modelBuilder.Entity<Berles>()
            .HasKey(b => new { b.KezdoDatum, b.AutoId, b.BerloId });
        
        modelBuilder.Entity<Berles>().Property(b => b.KezdoDatum).HasColumnType("date").IsRequired();
        modelBuilder.Entity<Berles>().Property(b => b.VegDatum).HasColumnType("date");
        modelBuilder.Entity<Berles>().Property(b => b.Szamla).IsRequired().HasMaxLength(20);
        modelBuilder.Entity<Berles>().HasIndex(b => b.Szamla).IsUnique();

        modelBuilder.Entity<Berles>().HasOne(b => b.Auto).WithMany(a => a.Berlesek)
            .HasForeignKey(b => b.AutoId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Berles>().HasOne(b => b.Berlo).WithMany(r => r.Berlesek)
            .HasForeignKey(b => b.BerloId).OnDelete(DeleteBehavior.Cascade);


        modelBuilder.Entity<Berlo>().Property(b => b.SzulDatum).HasColumnType("date");
        modelBuilder.Entity<Berlo>().Property(b => b.Nev).IsRequired().HasMaxLength(40);
        modelBuilder.Entity<Berlo>().Property(b => b.UtcaNev).HasMaxLength(25);       
        modelBuilder.Entity<Berlo>().Property(b => b.HazSzam).HasMaxLength(8);

        modelBuilder.Entity<Berlo>().HasOne(b => b.Varos).WithMany(v => v.Berlok)
            .HasForeignKey(b => b.VarosId).OnDelete(DeleteBehavior.SetNull);


        modelBuilder.Entity<Varos>().Property(v => v.IranySzam).HasMaxLength(10);
        modelBuilder.Entity<Varos>().HasIndex(v => v.IranySzam).IsUnique();
        modelBuilder.Entity<Varos>().Property(v => v.Nev).IsRequired().HasMaxLength(25);
        modelBuilder.Entity<Varos>().Property(v => v.Megye).IsRequired().HasMaxLength(20);        

        modelBuilder.Entity<Auto>().Property(a => a.NapiAr).IsRequired();
        modelBuilder.Entity<Auto>().Property(a => a.TipusId).IsRequired();
        modelBuilder.Entity<Auto>().Property(a => a.AlapKm).IsRequired();

        modelBuilder.Entity<Auto>().HasOne(a => a.Tipus).WithMany(t => t.Autok)
            .HasForeignKey(a => a.TipusId).OnDelete(DeleteBehavior.Restrict);


        modelBuilder.Entity<Tipus>().Property(t => t.Marka).IsRequired().HasMaxLength(20);
        modelBuilder.Entity<Tipus>().Property(t => t.Modell).IsRequired().HasMaxLength(20);
        modelBuilder.Entity<Tipus>().Property(t => t.Evjarat).IsRequired();
        modelBuilder.Entity<Tipus>().Property(t => t.Fogyasztas).IsRequired();
        modelBuilder.Entity<Tipus>().Property(t => t.Uzemanyag).IsRequired().HasMaxLength(10);
    }
}
