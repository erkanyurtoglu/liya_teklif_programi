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
    public class MusteriConfiguration : IEntityTypeConfiguration<Musteri>
    // Musteri modelinin veritabanı yapılandırmasını tanımlar.
    {
        public void Configure(EntityTypeBuilder<Musteri> builder)
        // Musteri tablosunun veritabanı şemasını özelleştirir.
        {
            builder.ToTable("musteriler");
            // Tablo adını "musteriler" olarak belirler.

            builder.HasKey(m => m.MusteriId);
            // MusteriId alanını birincil anahtar (primary key) olarak tanımlar.

            builder.Property(m => m.FirmaAdi)
                .IsRequired()
                .HasMaxLength(200);
            // FirmaAdi zorunlu bir alan, maksimum 200 karakter.

            builder.Property(m => m.FirmaAdresi)
                .IsRequired()
                .HasMaxLength(500);
            // FirmaAdresi zorunlu bir alan, maksimum 500 karakter.

            builder.Property(m => m.FirmaTelefonu)
                .IsRequired()
                .HasMaxLength(20);
            // FirmaTelefonu zorunlu bir alan, maksimum 20 karakter.

            builder.Property(m => m.FirmaEposta)
                .IsRequired()
                .HasMaxLength(100);
            // FirmaEposta zorunlu bir alan, maksimum 100 karakter.

            builder.Property(m => m.VergiDairesi)
                .IsRequired()
                .HasMaxLength(100);
            // VergiDairesi zorunlu bir alan, maksimum 100 karakter.

            builder.Property(m => m.VergiNumarasi)
                .IsRequired()
                .HasMaxLength(50);
            // VergiNumarasi zorunlu bir alan, maksimum 50 karakter.

            builder.Property(m => m.IlgiliKisi)
                .IsRequired()
                .HasMaxLength(100);
            // IlgiliKisi zorunlu bir alan, maksimum 100 karakter.

            builder.Property(m => m.IlgiliKisiTelefonu)
                .IsRequired()
                .HasMaxLength(20);
            // IlgiliKisiTelefonu zorunlu bir alan, maksimum 20 karakter.

            builder.Property(m => m.EklenmeTarihi)
                .HasDefaultValueSql("GETDATE()");
            // EklenmeTarihi alanına varsayılan olarak mevcut tarihi atar (SQL GETDATE fonksiyonu).
        }
    }
}
