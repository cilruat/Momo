using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using Xamarin.Forms;

using Momo.Models;
using Momo.Views;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Acr.UserDialogs;

namespace Momo.ViewModels
{
    public class MyWriteTabsViewModel : BaseViewModel
    {
        private int _selectedViewModelIndex = 0;

        public int SelectedViewModelIndex
        {
            get => _selectedViewModelIndex;
            set
            {
                if (value == 1)
                    MyCommentsViewModel.OnBindingContextChanged();

                SetProperty(ref _selectedViewModelIndex, value);
            }
        }

        public MyNoticesViewModel MyNoticesViewModel { get; }
        public MyCommentsViewModel MyCommentsViewModel { get; }

        public MyWriteTabsViewModel()
        {
            MyNoticesViewModel = new MyNoticesViewModel();
            MyCommentsViewModel = new MyCommentsViewModel();
        }

        public void OnAppearing()
        {
        }
    }
}
