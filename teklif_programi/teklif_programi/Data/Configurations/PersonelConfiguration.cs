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
    public class PersonelConfiguration : IEntityTypeConfiguration<Personel>
    // Personel modelinin veritabanı yapılandırmasını tanımlar.
    {
        public void Configure(EntityTypeBuilder<Personel> builder)
        // Personel tablosunun veritabanı şemasını özelleştirir.
        {
            builder.ToTable("personeller");
            // Tablo adını "personeller" olarak belirler.

            builder.HasKey(p => p.PersonelId);
            // PersonelId alanını birincil anahtar (primary key) olarak tanımlar.

            builder.Property(p => p.AdSoyad)
                .IsRequired()
                .HasMaxLength(150);
            // AdSoyad zorunlu bir alan, maksimum 150 karakter.

            builder.Property(p => p.KullaniciAdi)
                .IsRequired()
                .HasMaxLength(50);
            builder.HasIndex(p => p.KullaniciAdi).IsUnique();
            // Kullanıcı adı zorunlu ve benzersizdir, maksimum 50 karakter.


            builder.Property(p => p.Telefon)
                .IsRequired()
                .HasMaxLength(20);
            // Telefon zorunlu bir alan, maksimum 20 karakter.

            builder.Property(p => p.Pozisyon)
                .IsRequired()
                .HasMaxLength(100);
            // Pozisyon zorunlu bir alan, maksimum 100 karakter.

            builder.Property(p => p.Sifre)
                .IsRequired()
                .HasMaxLength(100);
            // Sifre zorunlu bir alan, maksimum 100 karakter (hash için uygun uzunluk).

            builder.Property(p => p.EklenmeTarihi)
                .HasDefaultValueSql("GETDATE()");
            // EklenmeTarihi alanına varsayılan olarak mevcut tarihi atar (SQL GETDATE fonksiyonu).
        }
    }
}
