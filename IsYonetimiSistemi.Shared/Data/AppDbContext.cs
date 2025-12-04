using Microsoft.EntityFrameworkCore;
using IsYonetimiSistemi.Shared.Models;

namespace IsYonetimiSistemi.Shared.Data;

public class AppDbContext : DbContext
{
    public DbSet<Kullanici> Kullanicilar { get; set; }
    public DbSet<Personel> Personeller { get; set; }
    public DbSet<Islem> Islemler { get; set; }
    public DbSet<Hatirlatma> Hatirlatmalar { get; set; }
    public DbSet<Gorev> Gorevler { get; set; }

    // Telegram bot i?in constructor
    public AppDbContext() { }
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // ====== SOMEE.COM CLOUD SQL SERVER ======
            var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ??
                                  "Server=sql6033.site4now.net;" +
                                  "Database=db_ac18b3_isyonetimi;" +
                                  "User Id=db_ac18b3_isyonetimi_admin;" +
                                  "Password=IsYonetim2025!@#;" +
                                  "Encrypt=False;" +
                                  "TrustServerCertificate=True;" +
                                  "MultipleActiveResultSets=True;";
            
            optionsBuilder.UseSqlServer(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Decimal hassasiyeti (Muhasebe icin onemli)
        modelBuilder.Entity<Islem>()
            .Property(i => i.Tutar)
            .HasPrecision(18, 2);
            
        modelBuilder.Entity<Islem>()
            .Property(i => i.OrijinalTutar)
            .HasPrecision(18, 2);
            
        modelBuilder.Entity<Personel>()
            .Property(p => p.KomisyonOrani)
            .HasPrecision(5, 2);
            
        modelBuilder.Entity<Islem>()
            .Property(i => i.PaylasimOrani)
            .HasPrecision(5, 2);
    }
}

