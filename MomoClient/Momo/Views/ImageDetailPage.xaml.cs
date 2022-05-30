using System;
using System.Collections.Generic;
using Momo.ViewModels;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Momo.Views
{
    public partial class ImageDetailPage : ContentPage
    {
        ImageDetailViewModel _viewModel;

        public ImageDetailPage(string url)
        {
            InitializeComponent();
            BindingContext = _viewModel = new ImageDetailViewModel(url);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }
    }
}