using Proyecto26;
using System;
using System.Collections.Generic;
using System.Text;

namespace Code.Models.REST.CommonType.Tasks
{
    /// <summary>
    /// Изменение статуса задачи
    /// </summary>
    public class UpdateTaskStatusRequest : TaskRequestCreator
    {
        public override string ActionName => "UpdateTaskStatus"; 
        public UpdateTaskStatusRequest() { }
        
        public UpdateTaskStatusRequest(List<Guid> taskIds, BaseTaskStatus status) : base()
        {
            Dictionary<string, object> paramDict = new Dictionary<string, object>();
            paramDict.Add("GuidList", taskIds);
            paramDict.Add("NewStatus", status);

            TaskRequestedParams = paramDict;

            FillRequest();
        }
    }

    /// <summary>
    /// Ответ
    /// </summary>
    public class UpdateTaskStatusResponse : FQResponse
    {
        public BaseTask UpdatedTask
        {
            get
            {
                return ri.DeserializeResponseData<BaseTask>();
            }
        }
        public UpdateTaskStatusResponse(ResponseHelper response) : base(response)
        {

        }
    }
}
