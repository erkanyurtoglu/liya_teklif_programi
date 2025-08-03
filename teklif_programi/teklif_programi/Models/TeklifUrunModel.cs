using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Sepetteki ürünlerin temsilini yapacak model.

namespace teklif_programi.Models
{
    public class TeklifUrunModel : INotifyPropertyChanged
    {
        public int urun_id { get; set; }
        public string urun_kodu { get; set; }
        public string urun_aciklamasi { get; set; }
        public decimal birim_fiyat { get; set; }

        private int _adet = 1;
        public int adet
        {
            get => _adet;
            set
            {
                if (_adet != value)
                {
                    _adet = value;
                    OnPropertyChanged(nameof(adet));
                    OnPropertyChanged(nameof(toplam));
                }
            }
        }

        public decimal indirimli_fiyat { get; set; }

        public decimal toplam => adet * indirimli_fiyat;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
