using System;
using System.Collections.Generic;
using System.Text;

namespace Code.Models.REST
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
        /// Набор данных, описывающий клиента
        /// </summary>
        public UserCredentials Credentials { get; set; }

        public int ClientVersion { get; set; }

        public FQRequestInfo()
        {
            
        }

        public FQRequestInfo(bool empty)
        {
            RequestData = new ActionData(true);
            Credentials = new UserCredentials(true);
        }

        public FQRequestInfo Clone()
        {
            FQRequestInfo ri = new FQRequestInfo(true);

            UserCredentials uc = new UserCredentials(true);
            uc.Login = Credentials.Login;
            uc.PasswordHash = Credentials.PasswordHash;
            uc.tokenB64 = Credentials.tokenB64;
            uc.userId = Credentials.userId;
            uc.DeviceId = Credentials.DeviceId;

            ActionData rd = new ActionData(true);
            rd.actionName = RequestData.actionName;
            rd.postData = RequestData.postData;

            ri.Credentials = uc;
            ri.RequestData = rd;

            return ri;
        }
    }
}
