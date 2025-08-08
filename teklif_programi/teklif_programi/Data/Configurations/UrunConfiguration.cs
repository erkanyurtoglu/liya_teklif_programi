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
    {
        public void Configure(EntityTypeBuilder<Urun> builder)
        {
            builder.ToTable("urunler");

            builder.HasKey(u => u.UrunId);

            builder.Property(u => u.UrunKodu)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.Kategori)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.UrunAciklamasi)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(u => u.BirimFiyat)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(u => u.MaliyetFiyati)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(u => u.FiyatTL)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(u => u.FiyatUSD)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(u => u.FiyatEUR)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(u => u.EklenmeTarihi)
                .HasDefaultValueSql("GETDATE()");
        }
    }
}
