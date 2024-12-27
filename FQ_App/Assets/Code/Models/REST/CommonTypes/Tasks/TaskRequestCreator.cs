using Assets.Code.Models.REST.CommonTypes;
using System;
using System.Collections.Generic;
using static Assets.Code.Models.REST.CommonTypes.FQServiceException;

namespace Code.Models.REST.CommonType.Tasks
{
    public class TaskRequestCreator : FQRequestInfoCreator
    {
        public override object PostData => TaskRequestedParams;

        public object TaskRequestedParams = null;

        public virtual List<string> RequiredParameters => new List<string>();

        public TaskRequestCreator()
        {
            FillRequest();
        }

        public TaskRequestCreator(Dictionary<string, string> inputParams)
        {
            if (!CheckRequiredParameters(inputParams))
            {
                throw new FQServiceException(FQServiceExceptionType.EmptyRequiredField);
            }

            TaskRequestedParams = inputParams;

            FillRequest();
        }

        protected virtual bool CheckRequiredParameters(Dictionary<string, string> TaskRequestedParams)
        {
            foreach(var paramName in RequiredParameters)
            {
                if (!TaskRequestedParams.ContainsKey(paramName))
                    return false;                    
            }

            return true;
        }
    }
}
