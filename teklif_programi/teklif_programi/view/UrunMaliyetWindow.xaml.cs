using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using teklif_programi.Data;
using teklif_programi.Models;

namespace teklif_programi.view
{
    /// <summary>
    /// Ürün maliyet satırlarının girildiği pencere.
    /// </summary>
    public partial class UrunMaliyetWindow : Window
    {
        private readonly Urun _urun;
        private readonly ObservableCollection<UrunMaliyet> _masraflar = new();

        public UrunMaliyetWindow(Urun urun)
        {
            InitializeComponent();
            _urun = urun;

            using (var db = new TeklifDbContext())
            {
                var mevcut = db.UrunMaliyetleri
                               .Where(m => m.UrunId == _urun.UrunId)
                               .ToList();
                foreach (var item in mevcut)
                {
                    _masraflar.Add(item);
                }
            }

            _masraflar.CollectionChanged += Masraflar_CollectionChanged;
            foreach (var item in _masraflar)
                item.PropertyChanged += Item_PropertyChanged;

            dataGridMasraflar.ItemsSource = _masraflar;
            UpdateTotal();
        }

        private void Masraflar_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (UrunMaliyet item in e.NewItems)
                    item.PropertyChanged += Item_PropertyChanged;
            }
            if (e.OldItems != null)
            {
                foreach (UrunMaliyet item in e.OldItems)
                    item.PropertyChanged -= Item_PropertyChanged;
            }
            UpdateTotal();
        }

        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(UrunMaliyet.Tutar))
            {
                UpdateTotal();
            }
        }

        private void UpdateTotal()
        {
            var toplam = _masraflar.Sum(m => m.Tutar);
            txtToplamMaliyet.Text = toplam.ToString("N2");
        }

        private void BtnKaydet_Click(object sender, RoutedEventArgs e)
        {
            using var db = new TeklifDbContext();

            var eskiKayitlar = db.UrunMaliyetleri.Where(m => m.UrunId == _urun.UrunId);
            db.UrunMaliyetleri.RemoveRange(eskiKayitlar);

            foreach (var masraf in _masraflar.Where(m => !string.IsNullOrWhiteSpace(m.Aciklama)))
            {
                masraf.UrunId = _urun.UrunId;
                db.UrunMaliyetleri.Add(new UrunMaliyet
                {
                    UrunId = _urun.UrunId,
                    Aciklama = masraf.Aciklama,
                    Tutar = masraf.Tutar
                });
            }

            db.SaveChanges();

            var toplam = db.UrunMaliyetleri.Where(m => m.UrunId == _urun.UrunId).Sum(m => m.Tutar);
            var urun = db.Urunler.First(u => u.UrunId == _urun.UrunId);
            urun.MaliyetFiyati = toplam;
            db.SaveChanges();

            _urun.MaliyetFiyati = toplam;
            DialogResult = true;
            Close();
        }

        private void BtnIptal_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}