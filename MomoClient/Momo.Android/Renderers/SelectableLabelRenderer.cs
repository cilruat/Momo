using System.ComponentModel;

using Android.Content;
using Android.Widget;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

using Momo.Controls;
using Momo.Droid.Renderers;

[assembly: ExportRenderer(typeof(SelectableLabel), typeof(SelectableLabelRenderer))]
namespace Momo.Droid.Renderers
{
    public class SelectableLabelRenderer : LabelRenderer
    {
        public SelectableLabelRenderer(Context context) : base(context) { }

        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);
            Control.SetTextIsSelectable(true);
        }
    }
}