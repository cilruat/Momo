using System;
using System.ComponentModel;

using Foundation;
using UIKit;

using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

using Momo.Controls;
using Momo.iOS.Renderers;

[assembly: ExportRenderer(typeof(SelectableLabel), typeof(IOSSelectableLabelRenderer))]
namespace Momo.iOS.Renderers
{
    public class IOSSelectableLabelRenderer : ViewRenderer<SelectableLabel, UITextView>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<SelectableLabel> e)
        {
            if (e.NewElement != null)
            {
                if (Control == null)
                    SetNativeControl(new UITextView());

                UpdateText();
            }

            base.OnElementChanged(e);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (e.PropertyName == nameof(Label.Text))
                UpdateText();
        }

        void UpdateText()
        {
            if (string.IsNullOrWhiteSpace(Element?.Text) && Element?.FormattedText?.Spans.Count == 0)
            {
                Control.Text = string.Empty;
                return;
            }

            NSError error = null;
            if (!string.IsNullOrWhiteSpace(Element?.Text))
            {
                Control.AttributedText = new NSAttributedString(NSData.FromString(Element.Text),
                                                           new NSAttributedStringDocumentAttributes { DocumentType = NSDocumentType.PlainText },
                                                           ref error);
            }

            if (Element?.FormattedText?.Spans.Count > 0)
            {
                foreach (var item in Element?.FormattedText.Spans)
                {
                    Control.AttributedText = new NSAttributedString(NSData.FromString(item.Text),
                                                           new NSAttributedStringDocumentAttributes { DocumentType = NSDocumentType.PlainText },
                                                           ref error);
                }
            }

            switch (Element.FontAttributes)
            {
                case FontAttributes.None:
                    Control.Font = UIFont.SystemFontOfSize(new nfloat(Element.FontSize));
                    break;
                case FontAttributes.Bold:
                    Control.Font = UIFont.BoldSystemFontOfSize(new nfloat(Element.FontSize));
                    break;
                case FontAttributes.Italic:
                    Control.Font = UIFont.ItalicSystemFontOfSize(new nfloat(Element.FontSize));
                    break;
                default:
                    Control.Font = UIFont.BoldSystemFontOfSize(new nfloat(Element.FontSize));
                    break;
            }

            Control.BackgroundColor = Element.BackgroundColor.ToUIColor();
            Control.Selectable = true;
            Control.Editable = false;
            Control.ScrollEnabled = false;
            Control.ShouldInteractWithUrl += delegate { return true; };
        }
    }
}