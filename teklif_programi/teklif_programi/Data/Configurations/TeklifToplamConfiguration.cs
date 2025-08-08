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
    {
        public void Configure(EntityTypeBuilder<TeklifToplam> builder)
        {
            builder.ToTable("teklif_toplamlari");

            builder.HasKey(t => t.ToplamId);

            builder.Property(t => t.IndirimliToplam)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(t => t.KdvTutari)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
                
            builder.Property(t => t.GenelToplam)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.HasOne(t => t.Teklif)
                .WithOne(tk => tk.TeklifToplam)
                .HasForeignKey<TeklifToplam>(t => t.TeklifId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
