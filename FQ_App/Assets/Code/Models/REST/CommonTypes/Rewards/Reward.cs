using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Assets.Code.Models.Reward.BaseReward;

namespace Code.Models.REST.Rewards
{
    /// <summary>
    /// Награда
    /// </summary>
    public class Reward
    {
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
        
        public BaseRewardStatus Status
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

        public Guid Creator { get => creator; set => creator = value; }
        public Guid Id { get => id; set => id = value; }
        public Guid AvailableFor { get => availableFor; set => availableFor = value; }

        private Guid id;

        private Guid availableFor;

        private Guid creator;

        private string _title;

        private string _description;

        private int? _cost;

        private string _image;

        private BaseRewardStatus _status;


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
    }
}
