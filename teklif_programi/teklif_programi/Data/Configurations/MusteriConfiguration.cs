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
    {
        public void Configure(EntityTypeBuilder<Musteri> builder)
        {
            builder.ToTable("musteriler");

            builder.HasKey(m => m.MusteriId);

            builder.Property(m => m.FirmaAdi)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(m => m.FirmaAdresi)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(m => m.FirmaTelefonu)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(m => m.FirmaEposta)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(m => m.VergiDairesi)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(m => m.VergiNumarasi)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(m => m.IlgiliKisi)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(m => m.IlgiliKisiTelefonu)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(m => m.EklenmeTarihi)
                .HasDefaultValueSql("GETDATE()");
        }
    }
}
