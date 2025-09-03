using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using teklif_programi.Models;

namespace teklif_programi.Data.Configurations
{
    /// <summary>
    /// Admin tablosunun veritabanı yapılandırmasını tanımlar.
    /// </summary>
    public class AdminConfiguration : IEntityTypeConfiguration<Admin>
    {
        public void Configure(EntityTypeBuilder<Admin> builder)
        {
            builder.ToTable("adminler");
            // Tablo adını belirler.

            builder.HasKey(a => a.AdminId);
            // Birincil anahtar.

            builder.Property(a => a.KullaniciAdi)
                   .IsRequired()
                   .HasMaxLength(50);
            // Kullanıcı adı zorunlu ve maksimum 50 karakter.

            builder.Property(a => a.Sifre)
                   .IsRequired()
                   .HasMaxLength(100);
            // Şifre zorunlu ve maksimum 100 karakter.
        }
    }
}