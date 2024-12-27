using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Code.Models.REST
{
    /// <summary>
    /// Данные клиента, совершающего запрос
    /// </summary>
    public class UserCredentials
    {
        /// <summary>
        /// Имя клиента
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Внутренний индификатор клиента (заполняется по ходу обработки запроса)
        /// </summary>
        public Guid userId { get; set; }

        /// <summary>
        /// Солёный хэш пароля для первичной авторизации
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        /// Токен авторизации
        /// </summary>
        public string tokenB64 { get; set; }

        /// <summary>
        /// Уникальный идентификатор устройства
        /// </summary>
        public string DeviceId { get; set; }

        public UserCredentials()
        {

        }

        public UserCredentials(bool empty)
        {
            Login = string.Empty;
            userId = Guid.Empty;
            PasswordHash = string.Empty;
            tokenB64 = string.Empty;
            DeviceId = string.Empty;
        }        
    }
}
