using FFImageLoading.Cache;
using FFImageLoading.Forms;

namespace Momo.Models
{
    public class Chat
    {
        public string Id { get; set; }
        public string RoomId { get; set; }
        public string PersonId { get; set; }
        public string PersonName { get; set; }
        public string PersonImage { get; set; }
        public string Msg { get; set; }
        public string Time { get; set; }
        public bool MyChat { get; set; }
        public bool OtherChat { get; set; }
    }
}
