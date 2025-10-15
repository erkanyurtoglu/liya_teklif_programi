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
    public class TeklifUrunConfiguration : IEntityTypeConfiguration<TeklifUrun>
    // TeklifUrun modelinin veritabanı yapılandırmasını tanımlar.
    {
        public void Configure(EntityTypeBuilder<TeklifUrun> builder)
        // TeklifUrun tablosunun veritabanı şemasını özelleştirir.
        {
            builder.ToTable("teklif_urunleri");
            // Tablo adını "teklif_urunleri" olarak belirler.

            builder.HasKey(tu => tu.TeklifUrunId);
            // TeklifUrunId alanını birincil anahtar (primary key) olarak tanımlar.

            builder.Property(tu => tu.Adet)
                .IsRequired();
            // Adet zorunlu bir alan (ürün miktarı).

            builder.Property(tu => tu.BirimFiyat)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            // BirimFiyat zorunlu bir alan, 2 ondalık basamaklı decimal.

            builder.Property(tu => tu.IndirimliBirimFiyat)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            // IndirimliBirimFiyat zorunlu bir alan, 2 ondalık basamaklı decimal.

            builder.Property(tu => tu.ToplamTutar)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            // ToplamTutar zorunlu bir alan, 2 ondalık basamaklı decimal.

            builder.Property(tu => tu.MaliyetFiyati)
                .HasColumnType("decimal(18,2)")
                .IsRequired(false);
            // MaliyetFiyati opsiyonel alan, teklif sırasında kaydedilen maliyeti saklar.


            builder.Property(tu => tu.UrunAciklamasi)
                .HasColumnType("nvarchar(max)")
                .HasColumnName("UrunAciklamasi");

            builder.Property(tu => tu.UrunAciklamasiTr)
                .HasColumnType("nvarchar(max)")
                .HasColumnName("UrunAciklamasiTr");

            builder.Property(tu => tu.UrunAciklamasiEn)
                .HasColumnType("nvarchar(max)")
                .HasColumnName("UrunAciklamasiEn");


            builder.Property(tu => tu.ParaBirimi)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            // ParaBirimi zorunlu bir alan, 2 ondalık basamaklı decimal (Not: Bu alanın string olması daha uygun olabilir).

            builder.Property(tu => tu.FiyatTL)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            // FiyatTL zorunlu bir alan, TL cinsinden fiyat, 2 ondalık basamaklı decimal.

            builder.Property(tu => tu.FiyatUSD)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            // FiyatUSD zorunlu bir alan, USD cinsinden fiyat, 2 ondalık basamaklı decimal.

            builder.Property(tu => tu.FiyatEUR)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            // FiyatEUR zorunlu bir alan, EUR cinsinden fiyat, 2 ondalık basamaklı decimal.

            builder.Property(tu => tu.Tamamlandi)
                   .HasDefaultValue(false);


            builder.Property(tu => tu.UretimNotu)
                .HasMaxLength(1000);

            builder.HasOne(tu => tu.Teklif)
                .WithMany(t => t.TeklifUrunleri)
                .HasForeignKey(tu => tu.TeklifId)
                .OnDelete(DeleteBehavior.Cascade);
            // TeklifUrun ile Teklif arasında 1-N ilişki tanımlar; Teklif silinirse ilgili TeklifUrun'ler de silinir.

            builder.HasOne(tu => tu.Urun)
                .WithMany(u => u.TeklifUrunleri)
                .HasForeignKey(tu => tu.UrunId)
                .OnDelete(DeleteBehavior.Cascade);
            // TeklifUrun ile Urun arasında 1-N ilişki tanımlar; Urun silinirse ilgili TeklifUrun'ler de silinir.
        }
    }
}
