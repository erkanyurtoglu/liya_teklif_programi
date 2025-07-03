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
        public DbSet<Firma> Firmalar { get; set; }
        public DbSet<UrunData> Urunler { get; set; }
        public DbSet<PersonelData> Personeller { get; set; }
        public DbSet<Teklif> Teklifler { get; set; }
        public DbSet<TeklifDetay> TeklifDetaylari { get; set; }



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=EXCALIBUR\\SQLEXPRESS;Database=TeklifSistemiDB;Integrated Security=True;Encrypt=False;");
        }
    }
}
