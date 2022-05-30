using System.ComponentModel;
using System.Collections.Generic;

using FFImageLoading.Cache;
using FFImageLoading.Forms;

namespace Momo.Models
{
    public class Person
    {
        public string Id { get; set; }
        public bool Leader { get; set; }
        public string PersonName { get; set; }
        public string Grade { get; set; }
        public string PhoneNum { get; set; }
        public string Etc { get; set; }

        public string GoogleId { get; set; }
        public string GoogleEmail { get; set; }


        private string _personImage;
        public string PersonImage
        { 
            get => _personImage;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _personImage = "Icon_profile.png";
                    return;
                }

                string makeUrl = value;
                if (makeUrl.StartsWith(".."))
                    makeUrl =  Common.UrlServer + value.Substring(3);

                if (_personImage == makeUrl)
                    return;

                if (string.IsNullOrEmpty(_personImage) == false)
                    CachedImage.InvalidateCache(_personImage, CacheType.All, true);

                _personImage = makeUrl;
            }
        }
    }

    public class SelectableItemPerson : INotifyPropertyChanged
    {
        public SelectableItemPerson(Person person)
        {
            IsSelected = false;
            ChangeImage = "icon_uncheck.png";
            this.Person = person;
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };


        private string _changeImage;
        public string ChangeImage
        {
            get => _changeImage;
            set
            {
                _changeImage = value;
                PropertyChanged(this, new PropertyChangedEventArgs("ChangeImage"));
            }
        }

        public bool IsSelected { get; set; }
        public Person Person { get; set; }
    }
}