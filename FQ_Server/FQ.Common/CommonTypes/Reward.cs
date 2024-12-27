using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommonTypes
{
    /// <summary>
    /// Награда
    /// </summary>
    public class Reward
    {
        public enum RewardStatuses : int
        {
            Available = 0,
            Purchased,
            Handed,
            Deleted = 1000
        }

        //Системные
        public Guid id;            
        public Guid groupId;

        //Пользовательские
        private string _title;
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                if (value != null)
                {
                    _title = value;
                }
                else
                {
                    _title = string.Empty;
                }
            }
        }

        private string _description;
        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                if (value != null)
                {
                    _description = value;
                }
                else
                {
                    _description = string.Empty;
                }
            }
        }
        
        private int? _cost;
        public int? Cost
        {
            get
            {
                return _cost;
            }
            set
            {
                if (value != null)
                {
                    _cost = value;
                }
                else
                {
                    _cost = 0;
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

        public Guid creator;

        public Guid availableFor;

        private RewardStatuses _status;
        public RewardStatuses Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
            }
        }

        public DateTime CreationDate
        {
            get
            {
                return creationDate;
            }
            set
            {
                creationDate = value;
            }
        }

        public DateTime PurchaseDate
        {
            get
            {
                return purchaseDate;
            }
            set
            {
                purchaseDate = value;
            }
        }

        public DateTime HandedDate
        {
            get
            {
                return handedDate;
            }
            set
            {
                handedDate = value;
            }
        }

        private DateTime creationDate;

        private DateTime purchaseDate;

        private DateTime handedDate;

        //Конструкторы
        public Reward()
        {

        }

        public Reward(bool empty)
        {
            id = Guid.Empty;
            groupId = Guid.Empty;
            Title = string.Empty;
            Description = string.Empty;
            Cost = 0;
            Image = string.Empty;
        }
    }
}
