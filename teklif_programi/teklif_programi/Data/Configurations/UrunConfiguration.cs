using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using teklif_programi.Models;

namespace teklif_programi.Data.Configurations
{
    public class UrunConfiguration : IEntityTypeConfiguration<Urun>
    // Urun modelinin veritabanı yapılandırmasını tanımlar.
    {
        public void Configure(EntityTypeBuilder<Urun> builder)
        // Urun tablosunun veritabanı şemasını özelleştirir.
        {
            builder.ToTable("urunler");
            // Tablo adını "urunler" olarak belirler.

            builder.HasKey(u => u.UrunId);
            // UrunId alanını birincil anahtar (primary key) olarak tanımlar.

            builder.Property(u => u.UrunKodu)
                .IsRequired()
                .HasMaxLength(50);
            // UrunKodu zorunlu bir alan, maksimum 50 karakter.

            builder.Property(u => u.Kategori)
                .IsRequired()
                .HasMaxLength(100);
            // Kategori zorunlu bir alan, maksimum 100 karakter.

            builder.Property(u => u.UrunAciklamasi)
                .IsRequired()
                .HasMaxLength(500);
            // UrunAciklamasi zorunlu bir alan, maksimum 500 karakter.

            builder.Property(u => u.UrunAciklamasiEn)
            .IsRequired()
            .HasMaxLength(500);
            // UrunAciklamasiEn zorunlu bir alan, maksimum 500 karakter.


            builder.Property(u => u.BirimFiyat)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            // BirimFiyat zorunlu bir alan, 2 ondalık basamaklı decimal.

            builder.Property(u => u.MaliyetFiyati)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            // MaliyetFiyati zorunlu bir alan, 2 ondalık basamaklı decimal.

            builder.Property(u => u.FiyatTL)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            // FiyatTL zorunlu bir alan, TL cinsinden fiyat, 2 ondalık basamaklı decimal.

            builder.Property(u => u.FiyatUSD)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            // FiyatUSD zorunlu bir alan, USD cinsinden fiyat, 2 ondalık basamaklı decimal.

            builder.Property(u => u.FiyatEUR)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            // FiyatEUR zorunlu bir alan, EUR cinsinden fiyat, 2 ondalık basamaklı decimal.

            builder.Property(u => u.EklenmeTarihi)
                .HasDefaultValueSql("GETDATE()");
            // EklenmeTarihi alanına varsayılan olarak mevcut tarihi atar (SQL GETDATE fonksiyonu).
        }
    }
}
