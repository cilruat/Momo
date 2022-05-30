using System;
using System.Collections.Generic;
using FFImageLoading.Cache;
using FFImageLoading.Forms;

namespace Momo.Models
{
    public class Notice
    {
        public string Id { get; set; }
        public string GroupId { get; set; }
        public string GroupName { get; set; }
        public string PersonId { get; set; }        
        public string PersonName { get; set; }

        private string time;
        public string Time
        {
            get
            {
                DateTime date = DateTime.Parse(time);
                return date.ToLongDateString() + " " + date.ToShortTimeString();
            }
            set { time = value; }
        }

        public bool IsVisibleDesc { get; set; }
        public string Desc { get; set; }        
        public string CommentCnt { get; set; }
        public List<MediaFile> listFile = new List<MediaFile>();
        public bool AttachImage_1 { get; set; }
        public bool AttachImage_2 { get; set; }
        public bool AttachImage_3 { get; set; }
        public bool AttachImage_4 { get; set; }
        public bool IsMoreShow { get; set; }

        private string _attachImageUrl_1;
        private string _attachImageUrl_2;
        private string _attachImageUrl_3;
        private string _attachImageUrl_4;

        public string AttachImageUrl_1
        {
            get => _attachImageUrl_1;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _attachImageUrl_1 = "";
                    return;
                }

                string makeUrl = value;
                if (makeUrl.StartsWith(".."))
                    makeUrl = Common.UrlServer + value.Substring(3);

                if (_attachImageUrl_1 == makeUrl)
                    return;

                if (string.IsNullOrEmpty(_attachImageUrl_1) == false)
                    CachedImage.InvalidateCache(_attachImageUrl_1, CacheType.All, true);

                _attachImageUrl_1 = makeUrl;
            }
        }

        public string AttachImageUrl_2
        {
            get => _attachImageUrl_2;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _attachImageUrl_2 = "";
                    return;
                }

                string makeUrl = value;
                if (makeUrl.StartsWith(".."))
                    makeUrl = Common.UrlServer + value.Substring(3);

                if (_attachImageUrl_2 == makeUrl)
                    return;

                if (string.IsNullOrEmpty(_attachImageUrl_2) == false)
                    CachedImage.InvalidateCache(_attachImageUrl_2, CacheType.All, true);

                _attachImageUrl_2 = makeUrl;
            }
        }

        public string AttachImageUrl_3
        {
            get => _attachImageUrl_3;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _attachImageUrl_3 = "";
                    return;
                }

                string makeUrl = value;
                if (makeUrl.StartsWith(".."))
                    makeUrl = Common.UrlServer + value.Substring(3);

                if (_attachImageUrl_3 == makeUrl)
                    return;

                if (string.IsNullOrEmpty(_attachImageUrl_3) == false)
                    CachedImage.InvalidateCache(_attachImageUrl_3, CacheType.All, true);

                _attachImageUrl_3 = makeUrl;
            }
        }

        public string AttachImageUrl_4
        {
            get => _attachImageUrl_4;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _attachImageUrl_4 = "";
                    return;
                }

                string makeUrl = value;
                if (makeUrl.StartsWith(".."))
                    makeUrl = Common.UrlServer + value.Substring(3);

                if (_attachImageUrl_4 == makeUrl)
                    return;

                if (string.IsNullOrEmpty(_attachImageUrl_4) == false)
                    CachedImage.InvalidateCache(_attachImageUrl_4, CacheType.All, true);

                _attachImageUrl_4 = makeUrl;
            }
        }

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
                    makeUrl = Common.UrlServer + value.Substring(3);

                if (_personImage == makeUrl)
                    return;

                if (string.IsNullOrEmpty(_personImage) == false)
                    CachedImage.InvalidateCache(_personImage, CacheType.All, true);

                _personImage = makeUrl;
            }
        }
    }
}