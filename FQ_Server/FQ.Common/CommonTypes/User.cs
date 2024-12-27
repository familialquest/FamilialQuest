using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommonTypes
{
    public class User
    {
        // TODO: иметь в виду, что есть бинарные условия и добавление нового варианта их сломает;  переделать на явные сравнения?
        public enum RoleTypes
        {
            Children = 0,
            Parent
        }

        public enum UserStatus
        {
            Active = 0,
            Removed = 1000
        }

        //Системные
        private Guid _id;
        public Guid Id { get => _id; set => _id = value; }

        private Guid _groupId;
        public Guid GroupId { get => _groupId; set => _groupId = value; }

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

        private UserStatus _status;
        public UserStatus Status
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

        //Ниже - дубли полей из Account.
        //Костыль для возврата одной сущности.
        //TODO: мб сводить в одну сущность?
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


        //Конструкторы
        public User()
        {

        }

        public User(bool empty)
        {
            Id = Guid.Empty;
            GroupId = Guid.Empty;
            Name = string.Empty;
            Title = string.Empty;
            Role = RoleTypes.Children;
            Coins = 0;
            Image = string.Empty;
            Login = string.Empty;
            LastAction = 0;
            Status = UserStatus.Active;
        }

        //Клонирование
        public User Clone()
        {
            User copyUser = new User(true);

            copyUser.Id = Id;
            copyUser.GroupId = GroupId;
            copyUser.Name = Name;
            copyUser.Title = Title;
            copyUser.Role = Role;
            copyUser.Coins = Coins;
            copyUser.Image = Image;
            copyUser.Login = Login;
            copyUser.LastAction = LastAction;
            copyUser.Status = Status;

            return copyUser;
        }
    }
}
