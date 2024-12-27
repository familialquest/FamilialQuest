using Newtonsoft.Json;
using System;

namespace Code.Models.REST.Notifications
{
    public class NotifiedDevice
    {
        //Системные
        private Guid _userId;
        public Guid UserId { get => _userId; set => _userId = value; }
        private string _deivceId;
        public string DeviceId { get => _deivceId; set => _deivceId = value; }
        private string _regToken;
        public string RegToken { get => _regToken; set => _regToken = value; }

        public static NotifiedDevice GetNotifiedDeviceFromPostData(string inputParams)
        {
            return JsonConvert.DeserializeObject<NotifiedDevice>(inputParams);
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }

    }
}
