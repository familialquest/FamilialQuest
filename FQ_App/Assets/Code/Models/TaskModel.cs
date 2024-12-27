using System;
using System.Collections.Generic;
using Code.Controllers.MessageBox;
using Code.Models.REST;
using Code.Models.REST.CommonType.Tasks;
using Proyecto26;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Models
{
    public class ListChangedEventArgs : EventArgs
    {
        public List<Dictionary<string, object>> List;
    }
    //public class TaskModelOperationResult : DataModelOperationResult
    //{
    //    public TaskModelOperationResult(ResponseHelper in_response)
    //    {
    //        RawResponse = in_response;
    //        ParseResult();
    //    }
    //    public TaskModelOperationResult(ResponseHelper in_response, FQResponse in_parsed)
    //    {
    //        RawResponse = in_response;
    //        ParsedResponse = in_parsed;
    //        ParseResult();
    //    }


    //    public void ParseResult()
    //    {
    //        if (!RawResponse.Request.isNetworkError)
    //        {
    //            //success
    //            result = true;
    //            code = RawResponse.StatusCode;
    //        }
    //        else
    //        {
    //            //error
    //            result = false;
    //            code = RawResponse.StatusCode;
    //            status = RawResponse.Error;
    //            description = "Ошибка при запросе сервиса задач";
    //        }
    //    }

    //    public static DataModelOperationResult Wrap(ResponseHelper in_response)
    //    {
    //        return new TaskModelOperationResult(in_response);
    //    }
    //    public static DataModelOperationResult Wrap(ResponseHelper in_response, FQResponse in_parsed)
    //    {
    //        return new TaskModelOperationResult(in_response, in_parsed);
    //    }
    //}

    public class TaskModel
    {
        readonly string taskServiceUri = "http://localhost:56023/";
        public enum BaseTaskFilter
        {
            Available = 0,
            Active,
            Finished
        }
        public enum FinishedTaskFilter
        {
            All = 0,
            Successed,
            Failed,
            SolutionTimeOver,
            Declined,
            Canceled,
            AvailableUntilPassed
        }
        public enum AvailableTaskFilter
        {
            All = 0,
            Fresh
        }
        public enum AdminAvailableTaskFilter
        {
            All = 0,
            //Fresh //TODO: на будущее для НОВЫХ
            Draft,
            Announced
        }
        public enum ActiveTaskFilter
        {
            All = 0,
            InProgress,
            PendingReview
        }

        public static Dictionary<BaseTaskFilter, List<BaseTaskStatus>> BaseFilterToStatus = new Dictionary<BaseTaskFilter, List<BaseTaskStatus>>()
        {
            {
                BaseTaskFilter.Available, new List<BaseTaskStatus>() {
                                    BaseTaskStatus.Created,
                                    BaseTaskStatus.Assigned }
            },
            {
                BaseTaskFilter.Active, new List<BaseTaskStatus>() {
                                    BaseTaskStatus.Accepted,
                                    BaseTaskStatus.InProgress,
                                    BaseTaskStatus.Completed,
                                    BaseTaskStatus.PendingReview }
            },
            {
                BaseTaskFilter.Finished, new List<BaseTaskStatus>() {
                                    BaseTaskStatus.Closed,
                                    BaseTaskStatus.Canceled,
                                    BaseTaskStatus.Declined,
                                    BaseTaskStatus.Failed,
                                    BaseTaskStatus.AvailableUntilPassed,
                                    BaseTaskStatus.SolutionTimeOver,
                                    BaseTaskStatus.Successed }
            },
        };

        public static Dictionary<FinishedTaskFilter, List<BaseTaskStatus>> FilterToFinishedStatus = new Dictionary<FinishedTaskFilter, List<BaseTaskStatus>>()
        {
            {
                FinishedTaskFilter.Successed, new List<BaseTaskStatus>() {
                                    BaseTaskStatus.Successed,
                                    BaseTaskStatus.Completed }
            },
            {
                FinishedTaskFilter.Failed, new List<BaseTaskStatus>() {
                                    BaseTaskStatus.Failed }
            },
            {
                FinishedTaskFilter.SolutionTimeOver, new List<BaseTaskStatus>() {
                                    BaseTaskStatus.SolutionTimeOver }
            },
            {
                FinishedTaskFilter.Declined, new List<BaseTaskStatus>() {
                                    BaseTaskStatus.Declined }
            },
            {
                FinishedTaskFilter.Canceled, new List<BaseTaskStatus>() {
                                    BaseTaskStatus.Canceled }
            },
            {
                FinishedTaskFilter.AvailableUntilPassed, new List<BaseTaskStatus>() {
                                    BaseTaskStatus.AvailableUntilPassed }
            }
        };

        public static Dictionary<AdminAvailableTaskFilter, List<BaseTaskStatus>> FilterToAdminAvailableStatus = new Dictionary<AdminAvailableTaskFilter, List<BaseTaskStatus>>()
        {
            {
                AdminAvailableTaskFilter.Draft, new List<BaseTaskStatus>() {
                                    BaseTaskStatus.Created}
            },
            {
                AdminAvailableTaskFilter.Announced, new List<BaseTaskStatus>() {
                                    BaseTaskStatus.Assigned}
            }                
        };

        public static Dictionary<ActiveTaskFilter, List<BaseTaskStatus>> FilterToActiveStatus = new Dictionary<ActiveTaskFilter, List<BaseTaskStatus>>()
        {
            {
                ActiveTaskFilter.InProgress, new List<BaseTaskStatus>() {
                                    BaseTaskStatus.Accepted,
                                    BaseTaskStatus.InProgress }
            },
            {
                ActiveTaskFilter.PendingReview, new List<BaseTaskStatus>() {
                                    BaseTaskStatus.Completed,
                                    BaseTaskStatus.PendingReview }
            }
        };

        List<Dictionary<string, object>> m_tasks = null;

        public event EventHandler<ListChangedEventArgs> OnListChanged;

        protected virtual void ListChanged(ListChangedEventArgs e)
        {
            EventHandler<ListChangedEventArgs> handler = OnListChanged;
            handler?.Invoke(this, e);
        }

        //Метод для принудительного обновления UI страницы извне
        public void UpdatePageUI()
        {
            ListChanged(new ListChangedEventArgs());
        }

        public TaskModel() { }

        public TaskModel(string uri)
        {
            taskServiceUri = uri;
            UpdateTasks();
        }

        internal RSG.IPromise<DataModelOperationResult> CreateTask(Dictionary<string, string> taskParameters)
        {
            CreateTaskRequest taskRequest = new CreateTaskRequest(taskParameters);

            var prom = RestClientEx.PostEx(taskRequest.RequestInfo)
                .Then((res) => DataModelOperationResult.Wrap(res.RawResponse, new CreateTaskResponse(res.RawResponse)))
                .Catch((ex) => DataModelOperationResult.Wrap(ex));

            return prom;
        }

        internal RSG.IPromise<DataModelOperationResult> UpdateTask(Dictionary<string, string> taskParameters)
        {
            UpdateTaskRequest taskRequest = new UpdateTaskRequest(taskParameters);

            var prom = RestClientEx.PostEx(taskRequest.RequestInfo)
                .Then((res) => DataModelOperationResult.Wrap(res.RawResponse, new UpdateTaskResponse(res.RawResponse)))
                .Catch((ex) => DataModelOperationResult.Wrap(ex));

            return prom;
        }

        internal RSG.IPromise<DataModelOperationResult> ChangeTaskStatus(List<Guid> taskIds, BaseTaskStatus newStatus)
        {
            UpdateTaskStatusRequest taskRequest = new UpdateTaskStatusRequest(taskIds, newStatus);

            var prom = RestClientEx.PostEx(taskRequest.RequestInfo)
                .Then((res) => DataModelOperationResult.Wrap(res.RawResponse, new CreateTaskResponse(res.RawResponse)))
                .Catch((ex) => DataModelOperationResult.Wrap(ex));

            return prom;
        }

        internal RSG.IPromise<DataModelOperationResult> GetTasks()
        {
            SearchTasksRequest taskRequest = new SearchTasksRequest();

            var prom = RestClientEx.PostEx(taskRequest.RequestInfo)
                .Then((res) => DataModelOperationResult.Wrap(res.RawResponse, new SearchTasksResponse(res.RawResponse)))
                .Catch((ex) => DataModelOperationResult.Wrap(ex));

            return prom;
        }


        public List<Dictionary<string, object>> Tasks { get => m_tasks; set => m_tasks = value; }


        public List<Dictionary<string, object>> AvailableTasks { 
            get
            {
                if (Tasks != null)
                {
                    return Tasks.FindAll((item) =>
                    {
                        BaseTaskStatus itemStatus = Utils.StatusFromString(item["Status"].ToString());

                        if (BaseFilterToStatus[BaseTaskFilter.Available].Contains(itemStatus))
                            return true;

                        return false;
                    });
                }
                else
                {
                    return new List<Dictionary<string, object>>();
                }                
            }
        }
        public List<Dictionary<string, object>> ActiveTasks
        {
            get
            {
                if (Tasks != null)
                {
                    return Tasks.FindAll((item) =>
                    {
                        BaseTaskStatus itemStatus = Utils.StatusFromString(item["Status"].ToString());

                        if (BaseFilterToStatus[BaseTaskFilter.Active].Contains(itemStatus))
                            return true;

                        return false;
                    });
                }
                else
                {
                    return new List<Dictionary<string, object>>();
                }
            }
        }

        public List<Dictionary<string, object>> FinishedTasks
        {
            get
            {
                if (Tasks != null)
                {
                    return Tasks.FindAll((item) =>
                    {
                        BaseTaskStatus itemStatus = Utils.StatusFromString(item["Status"].ToString());

                        if (BaseFilterToStatus[BaseTaskFilter.Finished].Contains(itemStatus))
                            return true;

                        return false;
                    });
                }
                else
                {
                    return new List<Dictionary<string, object>>();
                }                
            }
        }

        public RSG.IPromise<DataModelOperationResult> UpdateTasks()
        {
            var prom = GetTasks()
                .Then((res) =>
                {
                    if (res.result)
                    {
                        Debug.Log(((SearchTasksResponse)res.ParsedResponse).FoundTasks?.Count);
                        UpdateList(res);
                    }
                    
                    return res;                    
                });

            return prom;
        }

        public void UpdateList(DataModelOperationResult result)
        {
            m_tasks = ((SearchTasksResponse)result.ParsedResponse).FoundTasks;

            ListChanged(new ListChangedEventArgs { List = m_tasks });
        }
    }
}
