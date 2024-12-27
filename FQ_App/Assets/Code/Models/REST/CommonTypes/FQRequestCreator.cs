using Assets.Code.Models.REST.CommonTypes;
using Code.Controllers.MessageBox;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using static Assets.Code.Models.REST.CommonTypes.FQServiceException;

namespace Code.Models.REST
{
    public class FQRequestInfoCreator
    {
        public virtual string ActionName => "";
        public virtual object PostData => null;

        public FQRequestInfo RequestInfo;

        public FQRequestInfoCreator()
        {

        }

        protected virtual void FillRequest()
        {
            RequestInfo = new FQRequestInfo();

            RequestInfo.Credentials = CredentialHandler.Instance.Credentials;            

            RequestInfo.RequestData = new ActionData(true);
            RequestInfo.RequestData.actionName = ActionName;
            RequestInfo.RequestData.postData = PostData;
        }
    }
}
