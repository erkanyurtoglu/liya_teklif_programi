using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using teklif_programi.Models;

namespace teklif_programi.Data.Configurations
{
    public class SevkBilgileriConfiguration : IEntityTypeConfiguration<SevkBilgileri>
    {
        public void Configure(EntityTypeBuilder<SevkBilgileri> builder)
        {
            // SSMS’te görünen tablo adı ve şema
            builder.ToTable("SevkBilgileri", "dbo");

            // PK kolonu adı (SSMS’te "SevkBilgilerid" görünüyor)
            builder.HasKey(s => s.SevkBilgileriId);
            builder.Property(s => s.SevkBilgileriId).HasColumnName("SevkBilgileriId");

            // FK kolonu adı (SSMS’te "TeklifNoID")
            builder.Property(s => s.TeklifId).HasColumnName("TeklifNoID");

            builder.Property(s => s.FaturaBasligi).HasMaxLength(200);
            builder.Property(s => s.FaturaAdresi).HasMaxLength(500);
            builder.Property(s => s.FaturaVergiDairesi).HasMaxLength(150);
            builder.Property(s => s.FaturaVergiNo).HasMaxLength(50);
            builder.Property(s => s.FaturaYetkili).HasMaxLength(150);
            builder.Property(s => s.FaturaTelefon).HasMaxLength(50);
            builder.Property(s => s.FaturaFax).HasMaxLength(50);
            builder.Property(s => s.FaturaEposta).HasMaxLength(150);

            builder.Property(s => s.IrsaliyeBasligi).HasMaxLength(200);
            builder.Property(s => s.IrsaliyeAdresi).HasMaxLength(500);
            builder.Property(s => s.IrsaliyeVergiDairesi).HasMaxLength(150);
            builder.Property(s => s.IrsaliyeVergiNo).HasMaxLength(50);
            builder.Property(s => s.IrsaliyeYetkili).HasMaxLength(150);
            builder.Property(s => s.IrsaliyeTelefon).HasMaxLength(50);
            builder.Property(s => s.IrsaliyeEposta).HasMaxLength(150);

            builder.Property(s => s.SiparisKdv).HasMaxLength(50);
            builder.Property(s => s.FaturaSekli).HasMaxLength(150);
            builder.Property(s => s.Garanti).HasMaxLength(200);
            builder.Property(s => s.Teslimat).HasMaxLength(200);
            builder.Property(s => s.EkFaturaNotu).HasMaxLength(500);
            builder.Property(s => s.Aciklamalar).HasColumnType("nvarchar(max)");
            builder.Property(s => s.SiparisTarihi).HasColumnType("date");

            builder.HasOne(s => s.Teklif)
                .WithOne(t => t.SevkBilgileri)
                .HasForeignKey<SevkBilgileri>(s => s.TeklifId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
