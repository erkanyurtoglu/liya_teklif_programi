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
    {
        public void Configure(EntityTypeBuilder<TeklifUrun> builder)
        {
            builder.ToTable("teklif_urunleri");

            builder.HasKey(tu => tu.TeklifUrunId);

            builder.Property(tu => tu.Adet)
                .IsRequired();

            builder.Property(tu => tu.BirimFiyat)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(tu => tu.IndirimliBirimFiyat)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(tu => tu.ToplamTutar)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(tu => tu.ParaBirimi)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(tu => tu.FiyatTL)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(tu => tu.FiyatUSD)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(tu => tu.FiyatEUR)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.HasOne(tu => tu.Teklif)
                .WithMany(t => t.TeklifUrunleri)
                .HasForeignKey(tu => tu.TeklifId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(tu => tu.Urun)
                .WithMany(u => u.TeklifUrunleri)
                .HasForeignKey(tu => tu.UrunId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
