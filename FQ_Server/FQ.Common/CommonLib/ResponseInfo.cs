using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonLib
{
    /// <summary>
    /// Стандартный в рамках сервиса формат ответа сервиса
    /// </summary>
    public class FQResponseInfo
    {
        /// <summary>
        /// Результат работы
        /// </summary>
        public bool Successfuly { get; set; }

        /// <summary>
        /// Данные - результат работы или ошибка
        /// </summary>
        public string ResponseData { get; set; }

        /// <summary>
        /// Новый токен авторизации
        /// </summary>
        public string ActualToken { get; set; }

        /// <summary>
        /// Id авторизованного пользователя
        /// </summary>
        public Guid UserId { get; set; }

        public bool FirstLogin { get; set; }

        /// <summary>
        /// Идентификатор сесси от получения запроса в Gate, до выдачи ответа
        /// </summary>
        public Guid SessionId { get; set; }

        public FQResponseInfo()
        {

        }

        public FQResponseInfo(bool empty)
        {
            Successfuly = false;
            ResponseData = string.Empty;
            ActualToken = string.Empty;
            UserId = Guid.Empty;
            SessionId = Guid.Empty;
        }

        public FQResponseInfo(object responseData)
        {
            ResponseData = JsonConvert.SerializeObject(responseData);
        }
    }
}
