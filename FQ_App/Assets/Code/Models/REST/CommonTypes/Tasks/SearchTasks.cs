using Assets.Code.Models.REST.CommonTypes;
using Proyecto26;
using System;
using System.Collections.Generic;
using System.Text;
using static Assets.Code.Models.REST.CommonTypes.FQServiceException;

namespace Code.Models.REST.CommonType.Tasks
{
    /// <summary>
    /// Добавление задачи
    /// </summary>
    public class SearchTasksRequest : TaskRequestCreator
    {
        public override string ActionName => "SearchTasks";
        public SearchTasksRequest() : base()
        { }

        public SearchTasksRequest(Dictionary<string, string> inputParams) : base(inputParams)
        {
            if (inputParams.Count == 0)
            {
                //"Должен быть задан хотя бы один параметр поиска"
                throw new FQServiceException(FQServiceExceptionType.DefaultError);
            }
        }
    }

    /// <summary>
    /// Ответ
    /// </summary>
    public class SearchTasksResponse : FQResponse
    {
        public List<Dictionary<string, object>> FoundTasks
        {
            get
            {
                return this.ri.DeserializeResponseData<List<Dictionary<string, object>>>();                
            }
        }

        public SearchTasksResponse(ResponseHelper response) : base(response)
        { }
    }
}
