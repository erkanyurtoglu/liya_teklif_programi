using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace teklif_programi.Models
{
    public class DovizKuru
    {
        // Nullable değil, bu yüzden constructor'da zorunlu atanmalı
        public string DovizCinsi { get; set; } = string.Empty;
        public decimal Alis { get; set; }
        public decimal Satis { get; set; }

        public DovizKuru() { }

        public DovizKuru(string dovizCinsi, decimal alis, decimal satis)
        {
            DovizCinsi = dovizCinsi;
            Alis = alis;
            Satis = satis;
        }

    }
}

