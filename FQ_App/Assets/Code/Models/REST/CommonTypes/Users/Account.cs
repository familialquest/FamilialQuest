using System;
using System.Net.Mail;

namespace Code.Models.REST.Users
{
    public class Account
    {
        public Guid userId;
        
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
              
       
        public Account()
        {

        }

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
