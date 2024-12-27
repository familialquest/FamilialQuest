using System;
using System.Collections.Generic;
using UnityEngine;

using Code.Models;
using Code.Models.REST.CommonType.Tasks;
using Code.Models.REST;
using Code.Controllers.MessageBox;
using UnityEngine.SceneManagement;

namespace Code.Controllers
{
    public class TaskLogicController : ScriptableObject
    {
        private static TaskLogicController _instance = null;
        public static TaskLogicController Instance
        {
            get
            {
                if (_instance == null)
                    _instance = ScriptableObject.CreateInstance<TaskLogicController>();

                return _instance;
            }
        }

        public static RSG.IPromise<DataModelOperationResult> CreateTask(Dictionary<string, string> inputValues)
        {
            // возможны дополнительные действия перед или после создания задачи        
            var promise = DataModel.Instance.Tasks.CreateTask(inputValues)
                .Then((result) =>
                {
                    Debug.Log($"status: {result.status}");

                    return result;
                });

            return promise;
        }

        public static RSG.IPromise<DataModelOperationResult> CreateDraftTask(Dictionary<string, string> inputValues)
        {
            // возможны дополнительные действия перед или после создания задачи


            var promise = DataModel.Instance.Tasks.CreateTask(inputValues)
                .Then((result) =>
                {
                    Debug.Log($"status: {result.status}");

                    if (result.result)
                    {
                        Debug.Log(((CreateTaskResponse)result.ParsedResponse).CreatedTask.Id);

                        var prom_UpdateTasks = DataModel.Instance.Tasks.UpdateTasks()
                            .Then((result_UpdateTasks) =>
                            {
                                return result_UpdateTasks;
                            });

                        return prom_UpdateTasks;
                    }
                    else
                    {
                        //Exception пустой потому что просто для возврата промиса
                        throw new Exception();
                    }
                });

            return promise;
        }

        public static RSG.IPromise<DataModelOperationResult> UpdateTask(Guid taskId, Dictionary<string, string> inputValues)
        {
            inputValues.Add("Id", taskId.ToString());

            var promise = DataModel.Instance.Tasks.UpdateTask(inputValues)
                .Then((result) =>
                {
                    Debug.Log($"status: {result.status}");
                    if (result.result)
                    {
                        var prom_UpdateTasks = DataModel.Instance.Tasks.UpdateTasks()
                            .Then((result_UpdateTasks) =>
                            {
                                return result_UpdateTasks;
                            });

                        return prom_UpdateTasks;
                    }
                    else
                    {
                        //Exception пустой потому что просто для возврата промиса
                        throw new Exception();
                    }
                });

            return promise;
        }

        private static void SetStatusValue(Dictionary<string, string> inputValues, BaseTaskStatus newStatus)
        {
            if (inputValues.ContainsKey("Status"))
            {
                inputValues["Status"] = Utils.StatusToString(newStatus);
            }
            else
            {
                inputValues.Add("Status", Utils.StatusToString(newStatus));
            }
        }
        
        public static bool CheckTransition(BaseTaskStatus current, BaseTaskStatus next)
        {
            return TaskStatusRelations.ValidateTransition(current, next);
        }

        public static RSG.IPromise<DataModelOperationResult> ChangeStatus(Guid taskId, int status)
        {
            List<Guid> list = new List<Guid>() { taskId };
            return ChangeStatus(list, status);
        }

        public static RSG.IPromise<DataModelOperationResult> ChangeStatus(Guid taskId, BaseTaskStatus status)
        {
            return ChangeStatus(taskId, (int)status);
        }

        public static RSG.IPromise<DataModelOperationResult> ChangeStatus(List<Guid> taskIds, int status)
        {
            // возможны дополнительные действия перед или после создания задачи
            BaseTaskStatus newStatus = BaseTaskStatus.None;
            try
            {
                newStatus = (BaseTaskStatus)status;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }

            var promise = DataModel.Instance.Tasks.ChangeTaskStatus(taskIds, newStatus)
                .Then((result) =>
                {
                    Debug.Log($"status: {result.status}");

                    if (result.result)
                    {
                        var prom_UpdateTasks = DataModel.Instance.Tasks.UpdateTasks()
                            .Then((result_UpdateTasks) =>
                            {
                                return result_UpdateTasks;
                            });

                        return prom_UpdateTasks;
                    }
                    else
                    {
                        //Exception пустой потому что просто для возврата промиса
                        throw new Exception();
                    }
                });

            return promise;
        }
    }
}