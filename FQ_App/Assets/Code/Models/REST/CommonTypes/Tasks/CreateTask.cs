using System;
using System.Collections.Generic;
using System.Text;
using Code.Models.REST.CommonTypes;
using Newtonsoft.Json;
using Proyecto26;

namespace Code.Models.REST.CommonType.Tasks
{
    /// <summary>
    /// Добавление задачи
    /// </summary>
    public class CreateTaskRequest : TaskRequestCreator
    {
        public override string ActionName => "CreateTask";
        public CreateTaskRequest() { }
        public override List<string> RequiredParameters => new List<string>()
        {
            "Name",
            "Cost"
        };

        public CreateTaskRequest(Dictionary<string, string> inputParams) : base(inputParams) 
        { 

        }
    }

    /// <summary>
    /// Ответ
    /// </summary>
    public class CreateTaskResponse : FQResponse
    {
        public BaseTask CreatedTask
        {
            get
            {
                return this.ri.DeserializeResponseData<BaseTask>();
            }
        }
            
        public CreateTaskResponse(ResponseHelper response) : base(response)
        { }
    }
}
