using System;
using System.Windows;
using teklif_programi.Models;
using teklif_programi.ViewModels;

namespace teklif_programi.view
{
    public partial class SevkBilgileriWindow : Window
    {
        private readonly SevkBilgileriViewModel _viewModel;

        public SevkBilgileriWindow(Teklif teklif)
        {
            InitializeComponent();
            _viewModel = new SevkBilgileriViewModel(teklif);
            DataContext = _viewModel;
            Closed += OnClosed;
        }

        private void OnClosed(object? sender, EventArgs e)
        {
            Closed -= OnClosed;
            _viewModel.Dispose();
        }
    }
}