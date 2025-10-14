// Gerekli isim alanları: Temel sistem, koleksiyonlar ve veri bağlama için
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ViewModel sınıflarının isim alanı
namespace teklif_programi.ViewModels
{
    // SatisSozlesmesiViewModel: Satış sözleşmesi metnini yöneten ViewModel
    public class SatisSozlesmesiViewModel : INotifyPropertyChanged
    {
        // _sozlesmeMetni: Sözleşme metnini saklayan özel alan
        private string _sozlesmeMetni = string.Empty;

        // SozlesmeMetni: UI ile bağlı sözleşme metni, değiştiğinde PropertyChanged tetiklenir
        public string SozlesmeMetni
        {
            get => _sozlesmeMetni;
            set
            {
                if (_sozlesmeMetni != value)
                {
                    _sozlesmeMetni = value;
                    OnPropertyChanged(nameof(SozlesmeMetni)); // UI’yi günceller
                }
            }
        }

        // Kurucu: Varsayılan sözleşme metnini başlatır
        public SatisSozlesmesiViewModel()
        {
            // SozlesmeMetni: Örnek bir satış sözleşmesi metni atanır
        SozlesmeMetni =
        @"
        1. Fiyatımız DOLAR cinsinden belirtilmiş olup, KDV dahildir. İş bu fatura ödemesinin, ödeme tarihindeki TCMB DÖVİZ EFEKTİF SATIŞ KURU ile Türk Lirası’na çevrilerek yapılması gerekmektedir. Aksi durumda kesilecek kur farkı faturasının tahsili yapılacaktır.
        2. Cihaz ücreti: %30’u sipariş sırasında peşin, kalan tutar teslimatta ödenecektir.
        3. Cihazlar; 1 yıl mekanik, 2 yıl elektronik parça olarak ücretsiz servis garantilidir. 10 yıl süreyle ücreti karşılığı teknik servis ve eğitim hizmeti verilecektir.
        4. Cihaz Teslimatı: Siparişe istinaden 1 hafta içinde teslim.
        5. Teklif Opsiyonu: Teklif tarihinden itibaren 3 gündür.
        6. Nakliye: Alıcı firmaya aittir.
        7. Alternatif olarak sunulan cihaz bedelleri, toplam teklif tutarına dahil edilmemiştir.
        8. Banka Bilgilerimiz: Liya Laboratuvar Test Cihazları İmalat ve Dış Ticaret A.Ş.
            - İŞ BANKASI TR16 0006 4000 0014 1520 1653 38
            - HALK BANKASI TR51 0001 2009 4140 0010 2645 69";
        }

        // PropertyChanged: UI veri bağlama için özellik değişim olayı
        public event PropertyChangedEventHandler? PropertyChanged;

        // OnPropertyChanged: Özellik değiştiğinde UI’yi günceller
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}