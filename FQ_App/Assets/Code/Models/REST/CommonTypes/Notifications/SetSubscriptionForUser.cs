using Proyecto26;
using System;
using System.Collections.Generic;

namespace Code.Models.REST.Notifications
{
    public class SetSubscriptionForUserRequest : FQRequestInfoCreator
    {
        public override string ActionName => "SetSubscriptionForUser";
        public override object PostData => inputParameters;

        Dictionary<string, string> inputParameters;

        public SetSubscriptionForUserRequest()
        {
            FillRequest();
        }

        public SetSubscriptionForUserRequest(Guid userId, bool isSubscribed)
        {
            inputParameters = new Dictionary<string, string>();
            inputParameters.Add("userId", userId.ToString());
            inputParameters.Add("isSubscribed", isSubscribed.ToString());
            FillRequest();
        }
    }

    /// <summary>
    /// Ответ
    /// </summary>
    public class SetSubscriptionForUserResponse : FQResponse
    {
        public bool IsSubscribed
        {
            get
            {
                return this.ri.DeserializeResponseData<bool>();
            }
        }

        public SetSubscriptionForUserResponse(ResponseHelper response) : base(response)
        { }
    }
}
