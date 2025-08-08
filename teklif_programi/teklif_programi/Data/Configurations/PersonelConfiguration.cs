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
    {
        public void Configure(EntityTypeBuilder<Personel> builder)
        {
            builder.ToTable("personeller");

            builder.HasKey(p => p.PersonelId);

            builder.Property(p => p.AdSoyad)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(p => p.Telefon)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(p => p.Pozisyon)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.Sifre)
                .IsRequired()
                .HasMaxLength(100); // Şifre hash ise bu uzunluk yeterli olabilir

            builder.Property(p => p.EklenmeTarihi)
                .HasDefaultValueSql("GETDATE()");
        }
    }
}
