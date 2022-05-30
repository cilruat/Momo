using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Momo.Views
{
    public partial class MemoryViewPage : ContentView
    {
        private readonly IMemoryService memory;

        public MemoryViewPage()
        {
            InitializeComponent();

            //DependencyService.Get<IMemoryService>();
            memory = DependencyService.Resolve<IMemoryService>();
            RefreshScreen();
        }

        protected void HandleClicked(object sender, EventArgs e)
        {
            RefreshScreen();
        }

        void RefreshScreen()
        {
            UsedMemory.Text = "";
            FreeMemory.Text = "";
            HeapMemory.Text = "";
            MaxMemory.Text = "";
            HeapUsage.Text = "";
            TotalUsage.Text = "";

            UsedMemory.TextColor = Color.Black;
            FreeMemory.TextColor = Color.Black;
            HeapMemory.TextColor = Color.Black;
            MaxMemory.TextColor = Color.Black;
            HeapUsage.TextColor = Color.Black;
            TotalUsage.TextColor = Color.Black;

            if (memory != null)
            {
                MemoryInfo info = memory.GetInfo();
                if (info != null)
                {
                    UsedMemory.Text = String.Format("{0:N}", info.UsedMemory);

                    FreeMemory.Text = String.Format("{0:N}", info.FreeMemory);

                    HeapMemory.Text = String.Format("{0:N}", info.TotalMemory);

                    MaxMemory.Text = String.Format("{0:N}", info.MaxMemory);
                    HeapUsage.Text = String.Format("{0:P}", info.HeapUsage());
                    TotalUsage.Text = String.Format("{0:P}", info.Usage());



                    if (info.Usage() > 0.8)
                    {
                        FreeMemory.TextColor = Color.Red;
                        UsedMemory.TextColor = Color.Red;
                        TotalUsage.TextColor = Color.Red;
                    }
                }
            }
        }
    }
}