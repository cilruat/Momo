using System.Collections.Generic;
using Momo.Views;
using Xamarin.Forms;

namespace Momo
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(JoinPage), typeof(JoinPage));
            Routing.RegisterRoute(nameof(GroupDetailPage), typeof(GroupDetailPage));
        }
    }
}