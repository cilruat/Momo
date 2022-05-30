using System.ComponentModel;

using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Momo.Controls;
using Momo.iOS.Renderers;

[assembly: ExportRenderer(typeof(CustomEditor), typeof(IOSCustomEditorRenderer))]
namespace Momo.iOS.Renderers
{
    class IOSCustomEditorRenderer : EditorRenderer
    {
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            Control.Layer.BorderWidth = 0;
        }
    }
}