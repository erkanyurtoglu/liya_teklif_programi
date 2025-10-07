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
    // Teklif modelinin veritabanı yapılandırmasını tanımlar.
    {
        public void Configure(EntityTypeBuilder<Teklif> builder)
        // Teklif tablosunun veritabanı şemasını özelleştirir.
        {
            builder.ToTable("teklifler");
            // Tablo adını "teklifler" olarak belirler.

            builder.HasKey(t => t.TeklifId);
            // TeklifId alanını birincil anahtar (primary key) olarak tanımlar.

            builder.Property(t => t.OlusturmaTarihi)
                .HasDefaultValueSql("GETDATE()");
            // OlusturmaTarihi alanına varsayılan olarak mevcut tarihi atar (SQL GETDATE fonksiyonu).

            builder.Property(t => t.GenelIndirimOrani)
                .HasColumnType("decimal(5,2)")
                .IsRequired();
            // GenelIndirimOrani zorunlu bir alan, 2 ondalık basamaklı decimal (%100'e kadar).

            builder.Property(t => t.KdvOrani)
                .HasColumnType("decimal(5,2)")
                .IsRequired();
            // KdvOrani zorunlu bir alan, 2 ondalık basamaklı decimal.

            builder.Property(t => t.Durum)
                .IsRequired()
                .HasMaxLength(50);
            // Durum zorunlu bir alan, maksimum 50 karakter.

            builder.Property(t => t.MusteriNotu)
                .HasMaxLength(1000);
            // MusteriNotu opsiyonel bir alan, maksimum 1000 karakter.

            builder.Property(t => t.Dil)
                .IsRequired()
                .HasMaxLength(5)
                .HasDefaultValue("TR");
            // Teklifin dili zorunlu bir alan, maksimum 5 karakter ve varsayılan "TR".

            builder.Property(t => t.SatisSozlesmesiMetni)
                .HasColumnType("nvarchar(max)");
            // Teklife ait satış sözleşmesi metni (opsiyonel, uzun metinler için nvarchar(max)).


            builder.Property(t => t.KabulTarihi)
                .HasColumnType("datetime2");
            // Teklifin kabul edildiği tarih (opsiyonel).

            builder.Property(t => t.UretimPdfTarihi)
                .HasColumnType("datetime2");
            // Üretim PDF'inin indirildiği tarih (opsiyonel).



            builder.Property(t => t.IlgiliKisi)
                 .HasMaxLength(100);
            // Teklifte belirtilen ilgili kişi adı (opsiyonel, max 100 karakter).

            builder.Property(t => t.IlgiliKisiTelefonu)
                .HasMaxLength(20);
            // İlgili kişinin telefon numarası (opsiyonel, max 20 karakter).

            builder.Property(t => t.IlgiliKisiEposta)
                .HasMaxLength(100);
            // İlgili kişinin e-posta adresi (opsiyonel, max 100 karakter).


            // İlişkiler
            builder.HasOne(t => t.Musteri)
                .WithMany(m => m.Teklifler)
                .HasForeignKey(t => t.MusteriId)
                .OnDelete(DeleteBehavior.Cascade);
            // Teklif ile Musteri arasında 1-N ilişki tanımlar; Musteri silinirse ilgili teklifler de silinir.

            builder.HasOne(t => t.Personel)
                .WithMany(p => p.Teklifler)
                .HasForeignKey(t => t.PersonelId)
                .OnDelete(DeleteBehavior.SetNull);
            // Teklif ile Personel arasında 1-N ilişki tanımlar; Personel silinirse Teklif'in PersonelId'si null olur.
        }
    }
}
