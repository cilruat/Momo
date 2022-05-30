using FFImageLoading.Cache;
using FFImageLoading.Forms;

namespace Momo.Models
{
    public class ChatRoom
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ViewName { get; set; }
        public string GroupId { get; set; }
        public string GroupName { get; set; }
        public string PersonIds { get; set; }
        public string PersonImgs { get; set; }
        public string LastChatMsg { get; set; }
        public string LastTime { get; set; }
        public short UpdateCnt { get; set; }
        public bool Profile_1 { get; set; }
        public bool Profile_2 { get; set; }
        public bool Profile_3 { get; set; }
        public bool Profile_4 { get; set; }

        private string _profile_person_1;
        private string _profile_person_2;
        private string _profile_person_3;
        private string _profile_person_4;

        public string Profile_Person_1
        {
            get => _profile_person_1;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _profile_person_1 = "";
                    return;
                }

                string makeUrl = value;
                if (makeUrl.StartsWith(".."))
                    makeUrl = Common.UrlServer + value.Substring(3);

                if (_profile_person_1 == makeUrl)
                    return;

                if (string.IsNullOrEmpty(_profile_person_1) == false)
                    CachedImage.InvalidateCache(_profile_person_1, CacheType.All, true);

                _profile_person_1 = makeUrl;
            }
        }

        public string Profile_Person_2
        {
            get => _profile_person_2;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _profile_person_2 = "";
                    return;
                }

                string makeUrl = value;
                if (makeUrl.StartsWith(".."))
                    makeUrl = Common.UrlServer + value.Substring(3);

                if (_profile_person_2 == makeUrl)
                    return;

                if (string.IsNullOrEmpty(_profile_person_2) == false)
                    CachedImage.InvalidateCache(_profile_person_2, CacheType.All, true);

                _profile_person_2 = makeUrl;
            }
        }

        public string Profile_Person_3
        {
            get => _profile_person_3;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _profile_person_3 = "";
                    return;
                }

                string makeUrl = value;
                if (makeUrl.StartsWith(".."))
                    makeUrl = Common.UrlServer + value.Substring(3);

                if (_profile_person_3 == makeUrl)
                    return;

                if (string.IsNullOrEmpty(_profile_person_3) == false)
                    CachedImage.InvalidateCache(_profile_person_3, CacheType.All, true);

                _profile_person_3 = makeUrl;
            }
        }

        public string Profile_Person_4
        {
            get => _profile_person_4;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _profile_person_4 = "";
                    return;
                }

                string makeUrl = value;
                if (makeUrl.StartsWith(".."))
                    makeUrl = Common.UrlServer + value.Substring(3);

                if (_profile_person_4 == makeUrl)
                    return;

                if (string.IsNullOrEmpty(_profile_person_4) == false)
                    CachedImage.InvalidateCache(_profile_person_4, CacheType.All, true);

                _profile_person_4 = makeUrl;
            }
        }
    }
}
