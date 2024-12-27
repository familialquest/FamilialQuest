using System.Collections.Generic;
using Proyecto26;

namespace Code.Models.REST.CommonType.Tasks
{
    /// <summary>
    /// Добавление задачи
    /// </summary>
    public class UpdateTaskRequest : TaskRequestCreator
    {
        public override string ActionName => "UpdateTasks";
        public UpdateTaskRequest() { }
        public override List<string> RequiredParameters => new List<string>()
        {

        };

        public UpdateTaskRequest(Dictionary<string, string> inputParams) : base(inputParams)
        {

        }
    }

    /// <summary>
    /// Ответ
    /// </summary>
    public class UpdateTaskResponse : FQResponse
    {
        public BaseTask UpdatedTask
        {
            get
            {
                return this.ri.DeserializeResponseData<BaseTask>();
            }
        }

        public UpdateTaskResponse(ResponseHelper response) : base(response)
        { }
    }
}
