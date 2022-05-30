using System;
using System.Collections.Generic;
using System.Linq;
using Momo.ViewModels;
using Xamarin.Forms;

namespace Momo.Views
{
    public partial class SplashPage : ContentPage
    {
        public SplashPage()
        {
            InitializeComponent();
            this.BindingContext = new SplashViewModel();
        }
    }
}