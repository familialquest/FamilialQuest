using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Code.Models.REST
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

        public FQResponseInfo()
        {

        }

        public FQResponseInfo(bool empty)
        {
            ResponseData = string.Empty;
            ActualToken = string.Empty;
            UserId = Guid.Empty;
        }

        public FQResponseInfo(string token, object responseData)
        {
            ActualToken = token;
            ResponseData = JsonConvert.SerializeObject(responseData);
        }

        public T DeserializeResponseData<T>()
        {            
            return JsonConvert.DeserializeObject<T>(ResponseData);
        }

        //public Dictionary<string, string> DeserializeResponseData()
        //{
        //    var resp = JsonConvert.DeserializeObject<Dictionary<string, object>>(ResponseData);

        //    Dictionary<string, string> respDict = new Dictionary<string, string>();
        //    foreach(var pair in resp)
        //    {
        //        pair.
        //    }
        //}

        public FQResponseInfo(string responseData)
        {
            var fqResponse = JsonConvert.DeserializeObject<FQResponseInfo>(responseData);
            Successfuly = fqResponse.Successfuly;
            ResponseData = fqResponse.ResponseData;
            ActualToken = fqResponse.ActualToken;
            UserId = fqResponse.UserId;
            FirstLogin = fqResponse.FirstLogin;
        }
    }
}
