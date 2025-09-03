using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using teklif_programi.Models;

namespace teklif_programi.Data.Configurations
{
    /// <summary>
    /// UrunMaliyet modelinin veritabanı şeması.
    /// </summary>
    public class UrunMaliyetConfiguration : IEntityTypeConfiguration<UrunMaliyet>
    {
        public void Configure(EntityTypeBuilder<UrunMaliyet> builder)
        {
            builder.ToTable("urun_maliyetleri");

            builder.HasKey(m => m.MasrafId);

            builder.Property(m => m.Aciklama)
                   .HasMaxLength(250);

            builder.Property(m => m.Tutar)
                   .HasColumnType("decimal(18,2)");

            builder.HasOne<Urun>()
                   .WithMany()
                   .HasForeignKey(m => m.UrunId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}