using System.Collections.Generic;
using FFImageLoading.Cache;
using FFImageLoading.Forms;

namespace Momo.Models
{
    public class Group
    {
        public string Id { get; set; }
        public string LeaderId { get; set; }
        public string Name { get; set; }

        private string _image;
        public string Image
        { 
            get => _image; 
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _image = "Splash.png";
                    return;
                }

                string makeUrl = value;
                if (makeUrl.StartsWith(".."))
                    makeUrl = Common.UrlServer + value.Substring(3);

                if (_image == makeUrl)
                    return;

                if (string.IsNullOrEmpty(_image) == false)
                    CachedImage.InvalidateCache(_image, CacheType.All, true);

                _image = makeUrl;    
            }
        }
        public int Count { get; set; }
        public bool Free { get; set; }

        public List<Person> Persons = new List<Person>();
    }
}