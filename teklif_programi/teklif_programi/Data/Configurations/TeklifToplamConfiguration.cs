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
    public class TeklifToplamConfiguration : IEntityTypeConfiguration<TeklifToplam>
    // TeklifToplam modelinin veritabanı yapılandırmasını tanımlar.
    {
        public void Configure(EntityTypeBuilder<TeklifToplam> builder)
        // TeklifToplam tablosunun veritabanı şemasını özelleştirir.
        {
            builder.ToTable("teklif_toplamlari");
            // Tablo adını "teklif_toplamlari" olarak belirler.

            builder.HasKey(t => t.ToplamId);
            // ToplamId alanını birincil anahtar (primary key) olarak tanımlar.

            builder.Property(t => t.IndirimliToplam)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            // IndirimliToplam zorunlu bir alan, 2 ondalık basamaklı decimal.

            builder.Property(t => t.KdvTutari)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            // KdvTutari zorunlu bir alan, 2 ondalık basamaklı decimal.

            builder.Property(t => t.GenelToplam)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            // GenelToplam zorunlu bir alan, 2 ondalık basamaklı decimal.

            builder.Property(t => t.PaketlemeUcreti)
                   .HasColumnType("decimal(18,2)")
                   .HasDefaultValue(0m)
                   .ValueGeneratedNever()   // 0 dâhil her değeri EF gönderir
                   .IsRequired();

            // Paketleme ücreti alanı, varsayılan 0 değerli.

            builder.HasOne(t => t.Teklif)
                .WithOne(tk => tk.TeklifToplam)
                .HasForeignKey<TeklifToplam>(t => t.TeklifId)
                .OnDelete(DeleteBehavior.Cascade);
            // TeklifToplam ile Teklif arasında 1-1 ilişki tanımlar; Teklif silinirse TeklifToplam da silinir.
        }
    }
}
