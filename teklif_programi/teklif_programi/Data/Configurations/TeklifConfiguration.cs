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
    public class TeklifConfiguration : IEntityTypeConfiguration<Teklif>
    {
        public void Configure(EntityTypeBuilder<Teklif> builder)
        {
            builder.ToTable("teklifler");

            builder.HasKey(t => t.TeklifId);

            builder.Property(t => t.OlusturmaTarihi)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(t => t.GenelIndirimOrani)
                .HasColumnType("decimal(5,2)") // Örnek: %100'e kadar, 2 ondalık
                .IsRequired();

            builder.Property(t => t.KdvOrani)
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            builder.Property(t => t.Durum)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(t => t.MusteriNotu)
                .HasMaxLength(1000); // Notlar uzun olabilir, ama sınır koyduk

            // İlişkiler
            builder.HasOne(t => t.Musteri)
                .WithMany(m => m.Teklifler)
                .HasForeignKey(t => t.MusteriId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(t => t.Personel)
                .WithMany(p => p.Teklifler)
                .HasForeignKey(t => t.PersonelId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
