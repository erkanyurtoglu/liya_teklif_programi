using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace teklif_programi.Models
{
    /// <summary>
    /// Bir ürün için maliyet satırını temsil eder.
    /// </summary>
    public class UrunMaliyet : INotifyPropertyChanged
    {
        public int MasrafId { get; set; }

        public int UrunId { get; set; }

        [MaxLength(250)]
        public string Aciklama { get; set; } = string.Empty;

        private decimal _tutar;
        public decimal Tutar
        {
            get => _tutar;
            set
            {
                if (_tutar != value)
                {
                    _tutar = value;
                    OnPropertyChanged(nameof(Tutar));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}