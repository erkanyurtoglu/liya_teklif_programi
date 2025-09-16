using teklif_programi.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using teklif_programi.Data;
using teklif_programi.Models;

namespace teklif_programi.ViewModels
{
    public class SevkBilgileriViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly TeklifDbContext _context;
        private readonly SevkBilgileri _sevkBilgileri;
        private bool _disposed;

        private string? _faturaBasligi;
        private string? _faturaAdresi;
        private string? _faturaVergiDairesi;
        private string? _faturaVergiNo;
        private string? _faturaYetkili;
        private string? _faturaEposta;
        private string? _irsaliyeBasligi;
        private string? _irsaliyeAdresi;
        private string? _irsaliyeVergiDairesi;
        private string? _irsaliyeVergiNo;
        private string? _irsaliyeYetkili;
        private string? _faturaTelefon;
        private string? _faturaFax;
        private string? _irsaliyeTelefon;
        private string? _irsaliyeEposta;
        private string? _siparisKdv;
        private string? _faturaSekli;
        private string? _faturaTipi;
        private string? _taksit;
        private string? _garanti;
        private string? _teslimat;
        private string? _faturaEdilecekKisi;
        private string? _ekFaturaNotu;
        private DateTime? _siparisTarihi;
        private string? _aciklamalar;

        public RelayCommand<Window> KaydetCommand { get; }
        public RelayCommand<Window> IptalCommand { get; }

        public SevkBilgileriViewModel(Teklif teklif)
        {
            ArgumentNullException.ThrowIfNull(teklif);

            _context = new TeklifDbContext();

            _sevkBilgileri = _context.SevkBilgileri
                .AsTracking()
                .FirstOrDefault(s => s.TeklifId == teklif.TeklifId)
                ?? new SevkBilgileri { TeklifId = teklif.TeklifId };

            InitializeFields();

            KaydetCommand = new RelayCommand<Window>(Kaydet);
            IptalCommand = new RelayCommand<Window>(Iptal);
        }

        public string? FaturaBasligi
        {
            get => _faturaBasligi;
            set => SetProperty(ref _faturaBasligi, value, v => _sevkBilgileri.FaturaBasligi = v);
        }

        public string? FaturaAdresi
        {
            get => _faturaAdresi;
            set => SetProperty(ref _faturaAdresi, value, v => _sevkBilgileri.FaturaAdresi = v);
        }

        public string? FaturaVergiDairesi
        {
            get => _faturaVergiDairesi;
            set => SetProperty(ref _faturaVergiDairesi, value, v => _sevkBilgileri.FaturaVergiDairesi = v);
        }

        public string? FaturaVergiNo
        {
            get => _faturaVergiNo;
            set => SetProperty(ref _faturaVergiNo, value, v => _sevkBilgileri.FaturaVergiNo = v);
        }

        public string? FaturaYetkili
        {
            get => _faturaYetkili;
            set => SetProperty(ref _faturaYetkili, value, v => _sevkBilgileri.FaturaYetkili = v);
        }

        public string? FaturaTelefon
        {
            get => _faturaTelefon;
            set => SetProperty(ref _faturaTelefon, value, v => _sevkBilgileri.FaturaTelefon = v);
        }

        public string? FaturaFax
        {
            get => _faturaFax;
            set => SetProperty(ref _faturaFax, value, v => _sevkBilgileri.FaturaFax = v);
        }


        public string? FaturaEposta
        {
            get => _faturaEposta;
            set => SetProperty(ref _faturaEposta, value, v => _sevkBilgileri.FaturaEposta = v);
        }

        public string? IrsaliyeBasligi
        {
            get => _irsaliyeBasligi;
            set => SetProperty(ref _irsaliyeBasligi, value, v => _sevkBilgileri.IrsaliyeBasligi = v);
        }

        public string? IrsaliyeAdresi
        {
            get => _irsaliyeAdresi;
            set => SetProperty(ref _irsaliyeAdresi, value, v => _sevkBilgileri.IrsaliyeAdresi = v);
        }

        public string? IrsaliyeVergiDairesi
        {
            get => _irsaliyeVergiDairesi;
            set => SetProperty(ref _irsaliyeVergiDairesi, value, v => _sevkBilgileri.IrsaliyeVergiDairesi = v);
        }

        public string? IrsaliyeVergiNo
        {
            get => _irsaliyeVergiNo;
            set => SetProperty(ref _irsaliyeVergiNo, value, v => _sevkBilgileri.IrsaliyeVergiNo = v);
        }

        public string? IrsaliyeYetkili
        {
            get => _irsaliyeYetkili;
            set => SetProperty(ref _irsaliyeYetkili, value, v => _sevkBilgileri.IrsaliyeYetkili = v);
        }

        public string? IrsaliyeTelefon
        {
            get => _irsaliyeTelefon;
            set => SetProperty(ref _irsaliyeTelefon, value, v => _sevkBilgileri.IrsaliyeTelefon = v);
        }

        public string? IrsaliyeEposta
        {
            get => _irsaliyeEposta;
            set => SetProperty(ref _irsaliyeEposta, value, v => _sevkBilgileri.IrsaliyeEposta = v);
        }

        public string? SiparisKdv
        {
            get => _siparisKdv;
            set => SetProperty(ref _siparisKdv, value, v => _sevkBilgileri.SiparisKdv = v);
        }

        public string? FaturaSekli
        {
            get => _faturaSekli;
            set => SetProperty(ref _faturaSekli, value, v => _sevkBilgileri.FaturaSekli = v);
        }

        public string? FaturaTipi
        {
            get => _faturaTipi;
            set => SetProperty(ref _faturaTipi, value, v => _sevkBilgileri.FaturaTipi = v);
        }

        public string? Taksit
        {
            get => _taksit;
            set => SetProperty(ref _taksit, value, v => _sevkBilgileri.Taksit = v);
        }

        public string? Garanti
        {
            get => _garanti;
            set => SetProperty(ref _garanti, value, v => _sevkBilgileri.Garanti = v);
        }

        public string? Teslimat
        {
            get => _teslimat;
            set => SetProperty(ref _teslimat, value, v => _sevkBilgileri.Teslimat = v);
        }

        public string? FaturaEdilecekKisi
        {
            get => _faturaEdilecekKisi;
            set => SetProperty(ref _faturaEdilecekKisi, value, v => _sevkBilgileri.FaturaEdilecekKisi = v);
        }

        public string? EkFaturaNotu
        {
            get => _ekFaturaNotu;
            set => SetProperty(ref _ekFaturaNotu, value, v => _sevkBilgileri.EkFaturaNotu = v);
        }

        public DateTime? SiparisTarihi
        {
            get => _siparisTarihi;
            set => SetProperty(ref _siparisTarihi, value, v => _sevkBilgileri.SiparisTarihi = v);
        }

        public string? Aciklamalar
        {
            get => _aciklamalar;
            set => SetProperty(ref _aciklamalar, value, v => _sevkBilgileri.Aciklamalar = v);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void Kaydet(Window? window)
        {
            try
            {
                if (_sevkBilgileri.SevkBilgileriId == 0)
                {
                    _context.SevkBilgileri.Add(_sevkBilgileri);
                }

                _context.SaveChanges();
                EventHub.RaiseTeklifGuncellendi(_sevkBilgileri.TeklifId);
                MessageBox.Show("Sevk bilgileri başarıyla kaydedildi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                window?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sevk bilgileri kaydedilirken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void Iptal(Window? window) => window?.Close();

        private void InitializeFields()
        {
            _faturaBasligi = _sevkBilgileri.FaturaBasligi;
            _faturaAdresi = _sevkBilgileri.FaturaAdresi;
            _faturaVergiDairesi = _sevkBilgileri.FaturaVergiDairesi;
            _faturaVergiNo = _sevkBilgileri.FaturaVergiNo;
            _faturaYetkili = _sevkBilgileri.FaturaYetkili;
            _faturaTelefon = _sevkBilgileri.FaturaTelefon;
            _faturaFax = _sevkBilgileri.FaturaFax;
            _faturaEposta = _sevkBilgileri.FaturaEposta;
            _irsaliyeBasligi = _sevkBilgileri.IrsaliyeBasligi;
            _irsaliyeAdresi = _sevkBilgileri.IrsaliyeAdresi;
            _irsaliyeVergiDairesi = _sevkBilgileri.IrsaliyeVergiDairesi;
            _irsaliyeVergiNo = _sevkBilgileri.IrsaliyeVergiNo;
            _irsaliyeYetkili = _sevkBilgileri.IrsaliyeYetkili;
            _irsaliyeTelefon = _sevkBilgileri.IrsaliyeTelefon;
            _irsaliyeEposta = _sevkBilgileri.IrsaliyeEposta;
            _siparisKdv = _sevkBilgileri.SiparisKdv;
            _faturaSekli = _sevkBilgileri.FaturaSekli;
            _faturaTipi = _sevkBilgileri.FaturaTipi;
            _taksit = _sevkBilgileri.Taksit;
            _garanti = _sevkBilgileri.Garanti;
            _teslimat = _sevkBilgileri.Teslimat;
            _faturaEdilecekKisi = _sevkBilgileri.FaturaEdilecekKisi;
            _ekFaturaNotu = _sevkBilgileri.EkFaturaNotu;
            _siparisTarihi = _sevkBilgileri.SiparisTarihi;
            _aciklamalar = _sevkBilgileri.Aciklamalar;
        }

        private bool SetProperty<T>(ref T field, T value, Action<T> updateAction, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            updateAction(value);
            OnPropertyChanged(propertyName);
            return true;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            if (_disposed) return;

            _context.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}