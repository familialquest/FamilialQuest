using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Code.Models.REST.Group
{
    public class Group
    {
        //Системные
        private Guid _id;
        public Guid Id { get => _id; set => _id = value; }

        //Пользовательские
        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (value != null)
                {
                    _name = value;
                }
                else
                {
                    _name = string.Empty;
                }
            }
        }

        private string _image;
        public string Image
        {
            get
            {
                return _image;
            }
            set
            {
                try
                {
                    Convert.FromBase64String(value);
                    _image = value;
                }
                catch (Exception)
                {
                    _image = string.Empty;
                }
            }
        }

        private bool _subscriptionIsActive;
        public bool SubscriptionIsActive
        {
            get
            {
                return _subscriptionIsActive;
            }
            set
            {
                _subscriptionIsActive = value;
            }
        }

        private DateTime _subscriptionExpiredDate;
        public DateTime SubscriptionExpiredDate
        {
            get
            {
                return _subscriptionExpiredDate;
            }
            set
            {
                _subscriptionExpiredDate = value;
            }
        }

        //Конструкторы
        public Group()
        {

        }

        public Group(bool empty)
        {
            Id = Guid.Empty;
            Name = string.Empty;
            Image = string.Empty;
            SubscriptionIsActive = false;
            SubscriptionExpiredDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc); //TODO: CommonData.dateTime_FQDB_MinValue - зацыкливание референсов
        }
    }
}
