using Newtonsoft.Json;
using Proyecto26;
using System;
using System.Collections.Generic;
using System.Text;

namespace Code.Models.REST.HistoryEvent
{
    /// <summary>
    /// Получение всех наград
    /// </summary>
    public class GetHistoryEventsRequest
    {
        public FQRequestInfo request;

        public GetHistoryEventsRequest()
        {

        }

        /// <summary>
        /// Формирование запроса
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        /// <param name="sessionToken">Токен сессии</param>
        public GetHistoryEventsRequest(
             HistoryEvent historyEvent = null,
             int count = 0,
             DateTime? toDate = null
            )
        {            

            request = new FQRequestInfo(true);
            request.RequestData.actionName = "GetHistoryEvents";
            request.Credentials.Login = CredentialHandler.Instance.Credentials.Login;
            request.Credentials.tokenB64 = CredentialHandler.Instance.Credentials.tokenB64;

            Dictionary<string, object> postData = new Dictionary<string, object>();

            if (historyEvent != null)
            {
                postData.Add("conditionHistoryEvent", historyEvent);
            }

            if (count != 0)
            {
                postData.Add("count", count);
            }

            if (toDate != null)
            {
                postData.Add("toDate", toDate);
            }

            request.RequestData.postData = JsonConvert.SerializeObject(postData);
        }
    }

    /// <summary>
    /// Ответ
    /// </summary>
    public class GetHistoryEventsResponse : FQResponse
    {
        public List<HistoryEvent> HistoryEvents
        {
            get
            {
                return this.ri.DeserializeResponseData<List<HistoryEvent>>();
            }
        }

        public GetHistoryEventsResponse(ResponseHelper response) : base(response)
        { }
    }
}
