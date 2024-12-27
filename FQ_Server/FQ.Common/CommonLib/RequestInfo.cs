using CommonTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonLib
{
    /// <summary>
    /// Стандартный в рамках сервиса формат входных данных от клиента
    /// </summary>
    public class FQRequestInfo
    {
        /// <summary>
        /// Сам клиентский запрос к сервису: имя + данные
        /// </summary>
        public ActionData RequestData { get; set; }

        /// <summary>
        /// Входящий набор данных, описывающий клиента
        /// </summary>
        public UserCredentials Credentials { get; set; } 
        
        public int ClientVersion { get; set; }

        /// <summary>
        /// Аккаунт авторизованного пользователя
        /// При проверке GroupId - не всегда заполнено. Обращаться к ri._User.GroupId
        /// </summary>
        public Account _Account { get; set; }

        /// <summary>
        /// Информация об авторизованном пользователе
        /// ri._User.GroupId - здесь группа всегда
        /// </summary>
        public User _User { get; set; }

        /// <summary>
        /// Информация о группе авторизованного пользователя
        /// При проверке Id - не всегда заполнено. Обращаться к ri._User.GroupId
        /// </summary>
        public Group _Group { get; set; }

        public FQRequestInfo()
        {
            
        }

        public FQRequestInfo(bool empty)
        {
            RequestData = new ActionData(true);
            Credentials = new UserCredentials(true);
            ClientVersion = 0;
            _Account = new Account(true);
            _User = new User(true);
            _Group = new Group(true);
        }

        public FQRequestInfo Clone()
        {
            FQRequestInfo ri = new FQRequestInfo(true);

            UserCredentials uc = new UserCredentials(true);
            uc.Login = Credentials.Login;
            uc.PasswordHash = Credentials.PasswordHash;
            uc.tokenB64 = Credentials.tokenB64;
            uc.DeviceId = Credentials.DeviceId;

            ActionData rd = new ActionData(true);
            rd.actionName = RequestData.actionName;
            rd.postData = RequestData.postData;

            Account account = new Account(true);
            if (_Account != null)
            {
                account.failedLoginTryings = _Account.failedLoginTryings;
                account.ConfirmCode = _Account.ConfirmCode;
                account.CreationDate = _Account.CreationDate;
                account.Email = _Account.Email;
                account.isMain = _Account.isMain;
                account.LastAction = _Account.LastAction;
                account.Login = _Account.Login;
                account.PasswordHashCurrent = _Account.PasswordHashCurrent;
                account.PasswordHashNew = _Account.PasswordHashNew;
                account.Token = _Account.Token;
                account.DeviceId = _Account.DeviceId;
                account.userId = _Account.userId;
                account._confirmCode = _Account._confirmCode;
            }

            User user = new User(true);
            if (_User != null)
            {
                user.Id = _User.Id;
                user.GroupId = _User.GroupId;
                user.Login = _User.Login;
                user.Name = _User.Name;
                user.Title = _User.Title;
                user.Role = _User.Role;
                user.Coins = _User.Coins;
                user.Image = _User.Image;
            }

            Group group = new Group(true);
            if (_Group != null)
            {
                group.Id = _Group.Id;
                group.Name = _Group.Name;
                group.SubscriptionIsActive = _Group.SubscriptionIsActive;
                group.SubscriptionExpiredDate = _Group.SubscriptionExpiredDate;
                group.Image = _Group.Image;
            }

            ri.Credentials = uc;
            ri.ClientVersion = ClientVersion;
            ri._Account = account;
            ri._User = user;
            ri._Group = group;
            ri.RequestData = rd;

            return ri;
        }
    }
}
