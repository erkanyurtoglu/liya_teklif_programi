using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using teklif_programi.Models;

namespace teklif_programi.Data
{
    public class TeklifDbContext : DbContext // DbContext'ten türetildi
    {
        public DbSet<Musteri> Musteriler { get; set; }
        public DbSet<Urun> Urunler { get; set; }
        public DbSet<Personel> Personeller { get; set; }
        public DbSet<Teklif> Teklifler { get; set; }
        public DbSet<TeklifUrun> TeklifUrunleri { get; set; }
        public DbSet<TeklifToplam> TeklifToplamlari { get; set; }



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=EXCALIBUR\\SQLEXPRESS;Database=LiyaTeklifVeriTabani;Integrated Security=True;Encrypt=False;");
        }
    }
}
