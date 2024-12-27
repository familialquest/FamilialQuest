using System;
using System.Net.Mail;

namespace CommonTypes
{
    public class Account
    {
        public Guid userId;
        public int failedLoginTryings;

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
                    _login = value.ToLower();
                }
                else
                {
                    _login = string.Empty;
                }
            }
        }

        private string _email;
        public string Email
        {
            get
            {
                return _email;
            }
            set
            {
                if (value != null)
                {
                    _email = value.ToLower();
                }
                else
                {
                    _email = string.Empty;
                }
            }
        }

        public bool isMain;

        private DateTime _lastAction;
        public DateTime LastAction
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

        private string _passwordHashNew;
        public string PasswordHashNew
        {
            get
            {
                return _passwordHashNew;
            }
            set
            {
                try
                {
                    Convert.FromBase64String(value);
                    _passwordHashNew = value;
                }
                catch (Exception)
                {
                    _passwordHashNew = string.Empty;
                }
            }
        }

        private string _passwordHashCurrent;        
        public string PasswordHashCurrent
        {
            get
            {
                return _passwordHashCurrent;
            }
            set
            {
                try
                {
                    Convert.FromBase64String(value);
                    _passwordHashCurrent = value;
                }
                catch (Exception)
                {
                    _passwordHashCurrent = string.Empty;
                }
            }
        }

        private string _token;        
        public string Token
        {
            get
            {
                return _token;
            }
            set
            {
                try
                {
                    Convert.FromBase64String(value);
                    _token = value;
                }
                catch (Exception)
                {
                    _token = string.Empty;
                }
            }
        }

        private string _deviceId;
        public string DeviceId
        {
            get
            {
                return _deviceId;
            }
            set
            {
                try
                {
                    Convert.FromBase64String(value);
                    _deviceId = value;
                }
                catch (Exception)
                {
                    _deviceId = string.Empty;
                }
            }
        }

        //Для TempAccount
        public string _confirmCode;
        public string ConfirmCode
        {
            get
            {
                return _confirmCode;
            }
            set
            {
                if (value != null && value.Length == 6)
                {
                    _confirmCode = value;                    
                }
                else
                {
                    _confirmCode = string.Empty;
                }
            }
        }

        public DateTime CreationDate { get => creationDate; set => creationDate = value; }

        private DateTime creationDate;

        public Account()
        {

        }

        public Account(bool empty)
        {
            userId = Guid.Empty;
            failedLoginTryings = 0;
            Login = string.Empty;
            Email = string.Empty;
            isMain = false;
            LastAction = Constants.POSTGRES_DATETIME_MINVALUE;
            PasswordHashNew = string.Empty;
            PasswordHashCurrent = string.Empty;
            Token = string.Empty;
            DeviceId = string.Empty;
            CreationDate = Constants.POSTGRES_DATETIME_MINVALUE;
        }

        //public Account(UserCredentials uc)
        //{
        //    this.Email = uc.Login;
        //    this.PasswordHashNew = uc.PasswordHash;
        //    this.ConfirmCode = uc.ConfirmCode;
        //    this.creationDate = DateTime.Now;
        //}


        /// <summary>
        /// Проверка что логин - это емаил адрес
        /// </summary>
        /// <param name="login"></param>
        public static bool VerifyLoginAsEmail(string login)
        {
            //проверка структуры логина
            if (string.IsNullOrEmpty(login) ||
                login.Length > 320)
            {
                return false;
            }

            var emailParts = login.Split('@');

            if (emailParts.Length != 2 ||
                (emailParts[0] == string.Empty || emailParts[0].Length > 64) ||
                (emailParts[1] == string.Empty || emailParts[1].Length > 255))
            {
                return false;
            }

            try
            {
                //проверка на допустимые символы
                if (!System.Text.RegularExpressions.Regex.IsMatch(login, @"^[a-z0-9_.-]+@[a-z0-9-.]+\.[a-z0-9-.]+$", System.Text.RegularExpressions.RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500)))
                {
                    return false;
                }

                new MailAddress(login);
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Проверка, что логин - FQInnerLogin
        /// </summary>
        /// <param name="login"></param>
        /// <param name="firstPartOnly">проверка только имени до # (до FQ-тэга)</param>
        /// <returns></returns>
        public static bool VerifyLoginAsFQInnerLogin(string login, bool firstPartOnly = false)
        {
            if (firstPartOnly)
            {
                //проверка структуры логина
                if (string.IsNullOrEmpty(login) ||
                    login.Length > 64)
                {
                    return false;
                }

                try
                {
                    //проверка на допустимые символы
                    if (!System.Text.RegularExpressions.Regex.IsMatch(login, @"^[a-z0-9]+$", System.Text.RegularExpressions.RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500)))
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                //проверка структуры логина
                if (string.IsNullOrEmpty(login) ||
                    login.Length > 75) //до64символов#до10символов
                {
                    return false;
                }

                var fqInnerLoginParts = login.Split('#');

                if (fqInnerLoginParts.Length != 2 ||
                    (fqInnerLoginParts[0] == string.Empty || fqInnerLoginParts[0].Length > 64) ||
                    (fqInnerLoginParts[1].Length < 4 || fqInnerLoginParts[1].Length > 10))
                {
                    return false;
                }

                try
                {
                    //проверка на допустимые символы
                    if (!System.Text.RegularExpressions.Regex.IsMatch(login, @"^[a-z0-9]+#[0-9]+$", System.Text.RegularExpressions.RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500)))
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }
    }
}
