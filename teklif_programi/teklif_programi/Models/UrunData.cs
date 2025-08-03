using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.ComponentModel.DataAnnotations;

namespace teklif_programi.Models
{
    public class UrunData : INotifyPropertyChanged
    {
        [Key]
        public string UrunKoduID { get; set; }

        public string Kategori { get; set; }
        public string Aciklama { get; set; }

        public int _adet;
        public int Adet
        {
            get => _adet;
            set
            {
                if (value != _adet)
                {
                    _adet = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal BirimSatisFiyati { get; set; }

        public decimal YurticiMaliyet { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
