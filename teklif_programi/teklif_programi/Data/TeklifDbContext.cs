using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using teklif_programi.Models;
using System.Configuration;

namespace teklif_programi.Data
// Veritabanı işlemleri ad alanı.
{
    public class TeklifDbContext : DbContext
    // Veritabanı ile iletişim için Entity Framework Core sınıfı.
    {
        public DbSet<Musteri> Musteriler { get; set; }
        // Musteriler tablosunu temsil eder.

        public DbSet<Urun> Urunler { get; set; }
        // Urunler tablosunu temsil eder.

        public DbSet<Personel> Personeller { get; set; }
        // Personeller tablosunu temsil eder.

        public DbSet<Teklif> Teklifler { get; set; }
        // Teklifler tablosunu temsil eder.

        public DbSet<TeklifUrun> TeklifUrunleri { get; set; }
        // Teklif-ürün ilişkisi tablosunu temsil eder.

        public DbSet<TeklifToplam> TeklifToplamlari { get; set; }
        // Teklif toplamları tablosunu temsil eder.

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        // Veritabanı bağlantısını yapılandırır.
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = ConfigurationManager
                    .ConnectionStrings["TeklifDb"].ConnectionString;
                // app.config'dan bağlantı string'ini okur.

                optionsBuilder.UseSqlServer(connectionString);
                // SQL Server bağlantısını kurar.
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        // Veritabanı modelini özelleştirir.
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TeklifDbContext).Assembly);
            // Projedeki model yapılandırmalarını uygular.
        }
    }
}