using Code.Models.RoleModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Code.Models.REST.Users
{
    public class User
    {
        //Системные
        private Guid _id;
        public Guid Id { get => _id; set => _id = value; }

        private int _coins;
        public int Coins { get => _coins; set => _coins = value; }

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

        private RoleTypes _role;
        public RoleTypes Role
        {
            get
            {
                return _role;
            }
            set
            {
                _role = value;
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

        private string _login;
        public string Login
        {
            get
            {
                return _login;
            }
            set
            {
                if (value != null)
                {
                    _login = value;
                }
                else
                {
                    _login = string.Empty;
                }
            }
        }

        private bool _selected;
        public bool Selected
        {
            get
            {
                return _selected;
            }
            set
            {
                _selected = value;
            }
        }

        private int _lastAction;
        public int LastAction
        {
            get
            {
                return _lastAction;
            }
            set
            {
                _lastAction = value;
            }
        }


        public User()
        {

        }
    }
}
