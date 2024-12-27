using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;
using Assets.Code.Models.REST.CommonTypes.Common;

namespace Code.Models.REST.HistoryEvent
{
    public class HistoryEvent
    {
        public enum ItemTypeEnum
        {
            Default = 0,

            Task,
            Reward,
            User
        }

        public enum MessageTypeEnum
        {
            Default = 0,

            Task_Created,                               //Созданная родителем задача в статусе черновика
            Task_Removed,                               //Задача в статусе черновика удалена
            Task_Published,                             //Задача переводится в Доступные из черновика
            Task_Canceled_Available,                    //Доступная ребенку задача переводится назад в черновик
            Task_Canceled_InProgress,                   //Выполняемая ребенком задача отменена родителем
            Task_Canceled_Related,                      //Отмена (перевод в статус "Не актуально" AvailableUntilPassed) задач удаляемого пользователя
            Task_ChangedStatus_InProgress,              //Доступная задача взята ребенком (в статус InProgress)
            Task_ChangedStatus_Verification,            //Выполняемая ребенком задача переведена в статус "требует проверки"
            Task_ChangedStatus_Declined,                //Выполняемая ребенком задача переведена в статус "отклонена"
            Task_ChangedStatus_Redo,                    //Проверяемая родителем задача переведена в статус "требует доработки" (снова InProgress)
            Task_ChangedStatus_Completed,               //Проверяемая родителем задача переведена в статус выполнена успешно
            Task_ChangedStatus_Failed,                  //Задача провалена: результат выполнения задачи был рассмотрен и оказался НЕудовлетворительным
            Task_ChangedStatus_SolutionTimeOver,        //Задача провалена: время, выделенное на выполнение задачи, истекло
            Task_ChangedStatus_AvailableUntilPassed,    //Задача провалена: время, выделенное на принятие задача, истекло

            Reward_Created,
            Reward_Removed,
            Reward_Purchased,
            Reward_Handed,

            User_Created,
            User_PasswordChanged,
            User_Removed
        }

        public enum VisabilityEnum
        {
            Default = 0,

            Group,      //Родители + дети
            Children,   //Родители + конкретный ребенок
            Parents     //Родители
        }

        private Guid _id;
        public Guid Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        private Guid _groupId;
        public Guid GroupId
        {
            get
            {
                return _groupId;
            }
            set
            {
                _groupId = value;
            }
        }

        private DateTime _creationDate;
        public DateTime CreationDate
        {
            get
            {
                return _creationDate;
            }
            set
            {
                _creationDate = value;
            }
        }

        private int _creationDateMinAgo;
        public int CreationDateMinAgo
        {
            get
            {
                return _creationDateMinAgo;
            }
            set
            {
                _creationDateMinAgo = value;
            }
        }

        private ItemTypeEnum _itemType;
        public ItemTypeEnum ItemType
        {
            get
            {
                return _itemType;
            }
            set
            {
                _itemType = value;
            }
        }

        private MessageTypeEnum _messageType;
        public MessageTypeEnum MessageType
        {
            get
            {
                return _messageType;
            }
            set
            {
                _messageType = value;
            }
        }

        private VisabilityEnum _visability;
        public VisabilityEnum Visability
        {
            get
            {
                return _visability;
            }
            set
            {
                _visability = value;
            }
        }

        private Guid _targetItem;
        public Guid TargetItem
        {
            get
            {
                return _targetItem;
            }
            set
            {
                _targetItem = value;
            }
        }

        private List<Guid> _availableFor;
        public List<Guid> AvailableFor
        {
            get
            {
                return _availableFor;
            }
            set
            {
                _availableFor = value;
            }
        }

        private Guid _doer;
        public Guid Doer
        {
            get
            {
                return _doer;
            }
            set
            {
                _doer = value;
            }
        }

        private string _eventTitle;
        public string EventTitle
        {
            get
            {
                return _eventTitle;
            }
            set
            {
                _eventTitle = value;
            }
        }

        private string _eventText;
        public string EventText
        {
            get
            {
                return _eventText;
            }
            set
            {
                _eventText = value;
            }
        }

        private string _targetItemName;
        public string TargetItemName
        {
            get
            {
                return _targetItemName;
            }
            set
            {
                _targetItemName = value;
            }
        }

        public HistoryEvent()
        {

        }

        public HistoryEvent(bool isEmpty)
        {
            Id = Guid.Empty;
            GroupId = Guid.Empty;
            ItemType = HistoryEvent.ItemTypeEnum.Default;
            CreationDate = CommonData.dateTime_FQDB_MinValue;
            CreationDateMinAgo = -1;
            AvailableFor = new List<Guid>();
            Doer = Guid.Empty;
            MessageType = HistoryEvent.MessageTypeEnum.Default;
            TargetItem = Guid.Empty;
            Visability = HistoryEvent.VisabilityEnum.Default;
            EventTitle = string.Empty;
            EventText = string.Empty;
            TargetItemName = string.Empty;
        }

        public HistoryEvent(ItemTypeEnum itemType, MessageTypeEnum messageType, VisabilityEnum visability, Guid targetItem, List<Guid> availableFor, Guid doer)
        {
            ItemType = itemType;
            MessageType = messageType;
            Visability = visability;
            TargetItem = targetItem;
            AvailableFor = availableFor;
            Doer = doer;
        }
        public static HistoryEvent Deserialize(string input)
        {
            try
            {
                return JsonConvert.DeserializeObject<HistoryEvent>(input);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public string Serialize(HistoryEvent input)
        {
            return JsonConvert.SerializeObject(input);
        }
    }
}
