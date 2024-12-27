using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonTypes
{
    public class NotificationInfo
    {
        //
        // Summary:
        //     Gets or sets a collection of key-value pairs that will be added to the message
        //     as data fields. Keys and the values must not be null.   
        public IDictionary<string, string> Data { get => _data; set => _data = value; }
        public List<Guid> TargetsIds { get => _targetsIds; set => _targetsIds = value; }
        public bool IsSigninificant { get => _isSigninificant; set => _isSigninificant = value; }
        public string Title { get => _title; set => _title = value; }
        public string Body { get => _body; set => _body = value; }
        public string ImageURL { get => _imageUrl; set => _imageUrl = value; }

        public NotificationInfo()
        {
            _data = new Dictionary<string, string>();
            _targetsIds = new List<Guid>();
            _isSigninificant = false; 
        }

        //public static NotificationInfo Create(HistoryEvent historyEvent)
        //{
        //    NotificationInfo info = new NotificationInfo();

        //    info.Body = historyEvent.EventText;
        //    info.Title = historyEvent.EventTitle;
        //    info.TargetsIds = new List<Guid>();

        //    switch (historyEvent.Visability)
        //    {
        //        case HistoryEvent.VisabilityEnum.Default:
        //            break;
        //        case HistoryEvent.VisabilityEnum.Group:
        //            // get all users in group historyEvent.GroupId
        //            break;
        //        case HistoryEvent.VisabilityEnum.Children:
        //            // 
        //            info.TargetsIds.AddRange(historyEvent.AvailableFor);
        //            break;
        //        case HistoryEvent.VisabilityEnum.Parents:
        //            // get all parent in group historyEvent.GroupId
        //            break;
        //        default:
        //            break;
        //    }

        //    if (historyEvent.AvailableFor != null && historyEvent.AvailableFor == 0)

        //    return info;
        //}


        public static NotificationInfo Deserialize(string input)
        {
            try
            {
                return JsonConvert.DeserializeObject<NotificationInfo>(input);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }

        // данные для интепретации на стороне приложения
        private IDictionary<string, string> _data;
        // данные для отображения в оповещении
        private string _title;
        private string _body;
        private string _imageUrl;
        // идентификаторы целевых пользователей или групп (от 1)
        private List<Guid> _targetsIds;
        // будет отправлено как очень важное с максимальным игнором всех индивидуальных настроек пользователя
        // использовать только для критических событий
        private bool _isSigninificant;

    }
}
