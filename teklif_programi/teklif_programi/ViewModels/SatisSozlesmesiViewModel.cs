using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace teklif_programi.ViewModels
{
    public class SatisSozlesmesiViewModel : INotifyPropertyChanged
    {
        private string _sozlesmeMetni;
        public string SozlesmeMetni
        {
            get => _sozlesmeMetni;
            set
            {
                if (_sozlesmeMetni != value)
                {
                    _sozlesmeMetni = value;
                    OnPropertyChanged(nameof(SozlesmeMetni));
                }
            }
        }

        public SatisSozlesmesiViewModel()
        {
            SozlesmeMetni =
            @"
            1. Fiyatımız DOLAR cinsinden belirtilmiş olup, KDV dahildir. Fatura kesim tarihinde geçerli olan TCMB efektif satış kuru esas alınacaktır.
            2. Cihaz ücreti: %30’u sipariş sırasında peşin, kalan tutar teslimatta ödenecektir.
            3. Cihazlar; 1 yıl mekanik, 2 yıl elektronik parça olarak ücretsiz servis garantilidir. 10 yıl süreyle ücreti karşılığı teknik servis ve eğitim hizmeti verilecektir.
            4. Cihaz Teslimatı: Siparişe istinaden 1 hafta içinde teslim.
            5. Teklif Opsiyonu: Teklif tarihinden itibaren 3 gündür.
            6. Nakliye: Satıcı firmaya aittir.
            7. Alternatif olarak sunulan cihaz bedelleri, toplam teklif tutarına dahil edilmemiştir.
            8. Banka Bilgilerimiz: Liya Laboratuvar Test Cihazları İmalat ve Dış Ticaret A.Ş.
               - İŞ BANKASI TR16 0006 4000 0014 1520 1653 38
               - HALK BANKASI TR51 0001 2009 4140 0010 2645 69";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
