using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CommonLib;
using CommonRoutes;
using CommonTypes;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using TaskService.Models;
using static CommonLib.FQServiceException;
using static CommonTypes.User;

namespace TaskService.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class TaskService : ITaskService
    {
        static NLog.Logger logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        private readonly IHttpContextAccessor _httpContextAccessor;

        private FQRequestInfo inputRequest = null; // применяется только единожды. Глобальной объявил - лень прокидывать параметром через стек вызовов.

        private Timer timerCloseExpiredTasks = null;

        private void TimerCallback_CloseExpiredTasks(object e)
        {
            try
            {
                CloseExpiredTasks();
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
            }
        }

        /// <summary>
        /// Default constructor with HTTPContext
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        public TaskService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;

            //_ = Periodic_CloseExpiredTasks();
            timerCloseExpiredTasks = new Timer(
                this.TimerCallback_CloseExpiredTasks,
                null,
                CommonLib.Settings.Current[Settings.Name.Task.TempAccountTTL, CommonData.closeExpiredTasksPeriod] * 1000,
                CommonLib.Settings.Current[Settings.Name.Task.TempAccountTTL, CommonData.closeExpiredTasksPeriod] * 1000
            );
        }

        private async Task Periodic_CloseExpiredTasks()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    //logger.Debug($"CloseExpiredTasks started");

                    try
                    {
                        CloseExpiredTasks();
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex);
                    }
                    Task.Delay(CommonLib.Settings.Current[Settings.Name.Task.TempAccountTTL, CommonData.closeExpiredTasksPeriod] * 1000);
                }
            });
        }

        /// <summary>
        /// Поиск задач, время исполнения которых истекло, и первод в статус "AvailableUntilPassed" или "SolutionTimeOver" со списанием штрафа
        /// </summary>
        public void CloseExpiredTasks()
        {
            List<BaseTask> expiredTasks = DBWorker.GetExpiredTasks();
            List<BaseTask> expiredTasks_Assigned = new List<BaseTask>(expiredTasks.Where(x => x.Status == BaseTaskStatus.Assigned));
            List<BaseTask> expiredTasks_InProgress = new List<BaseTask>(expiredTasks.Where(x => x.Status == BaseTaskStatus.InProgress));

            List<Guid> diffGroupsIds = new List<Guid>(expiredTasks.Select(x => x.OwnerGroup).Distinct());

            foreach (var groupId in diffGroupsIds)
            {
                List<BaseTask> currentGroup_expiredTasks_Assigned = new List<BaseTask>(expiredTasks_Assigned.Where(x => x.OwnerGroup == groupId));
                List<BaseTask> currentGroup_expiredTasks_InProgress = new List<BaseTask>(expiredTasks_InProgress.Where(x => x.OwnerGroup == groupId));

                //Нужно для прокидования в Event-сервис
                FQRequestInfo ri = new FQRequestInfo(true);
                ri._User.GroupId = groupId;

                if (currentGroup_expiredTasks_Assigned.Count > 0)
                {
                    UpdateTaskStatus(null, BaseTaskStatus.AvailableUntilPassed, ri, currentGroup_expiredTasks_Assigned); //AvailableUntilPassed - это правильно
                }

                if (currentGroup_expiredTasks_InProgress.Count > 0)
                {
                    UpdateTaskStatus(null, BaseTaskStatus.SolutionTimeOver, ri, currentGroup_expiredTasks_InProgress);
                }
            }
        }

        /// <summary>
        /// Создание задачи с указанными параметрами
        /// </summary>
        /// <param name="name">Имя</param>
        /// <param name="description">Описание</param>
        /// <param name="cost">Стоимость</param>
        /// <param name="penalty">Штраф</param>
        /// <param name="availableUntil">Время актуальности задачи</param>
        /// <param name="solutionTime">Время решения задачи</param>
        /// <param name="speedBonus">Награда за преждевременное выполнение</param>
        /// <param name="creator">Создатель</param>
        /// <param name="executor">Получатель</param>
        /// <returns>Объект задачи</returns>
        public BaseTask CreateTask(FQRequestInfo ri, string name, string description, int cost, int penalty, DateTime availableUntil, TimeSpan solutionTime, int speedBonus, Guid creator, Guid executor)
        {
            var newTask = new BaseTask()
            {
                Cost = cost,
                Creator = creator,
                Description = description,
                AvailableUntil = availableUntil,
                Name = name,
                Penalty = penalty,
                Executor = executor,
                SolutionTime = solutionTime,
                SpeedBonus = speedBonus
            };
            return CreateTask(ri, newTask);
        }

        /// <summary>
        /// Создание задачи из "референс"-объекта
        /// </summary>
        /// <param name="newTask">"Референс"-обьект</param>
        /// <returns>Измененный объект задачи</returns>
        public BaseTask CreateTask(FQRequestInfo ri, BaseTask newTask, bool isStartingItem = false)
        {
            try
            {
                logger.Debug($"Создание задачи");

                if (ri._User.Role != RoleTypes.Parent)
                {
                    throw new FQServiceException(FQServiceExceptionType.NotEnoughRights);
                }

                var allTasksCount = DBWorker.GetAllTasksCount(ri._User.GroupId);
                if (allTasksCount >= CommonLib.Settings.Current[Settings.Name.Task.maxTasks_Total, CommonData.maxTasks_Total])
                {
                    throw new FQServiceException(FQServiceExceptionType.TotalItemsLimitAchieved);
                }

                // ввод данных полей игнорируются, они создаются только сервером
                newTask.Id = Guid.NewGuid();
                newTask.CreationDate = DateTime.UtcNow;
                newTask.ModificationTime = newTask.CreationDate;
                newTask.Status = BaseTaskStatus.Created;

                DBWorker.AddTask(newTask);

                if (!isStartingItem)
                {
                    //Запись HistoryEvent-а
                    try
                    {
                        FQRequestInfo ri_CreateHistoryEvent = ri.Clone();

                        List<Guid> availableFor = new List<Guid>();

                        HistoryEvent he = new HistoryEvent(HistoryEvent.ItemTypeEnum.Task, HistoryEvent.MessageTypeEnum.Task_Created, HistoryEvent.VisabilityEnum.Parents, newTask.Id, availableFor, ri._User.Id);

                        ri_CreateHistoryEvent.RequestData.actionName = "CreateHistoryEvent";
                        ri_CreateHistoryEvent.RequestData.postData = JsonConvert.SerializeObject(he);

                        RouteInfo.RouteToService(ri_CreateHistoryEvent, _httpContextAccessor);
                    }
                    catch (Exception)
                    {
                        //Если по какой-то причине не улаось записать HistoryEvent - не повод  прерывать операцию и кидать пользователю Error.
                    }
                }

                return newTask;
            }
            catch (FQServiceException fqEx)
            {
                logger.Error(fqEx);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }
        }

        /// <summary>
        /// Удаление задач по идентификаторам
        /// </summary>
        /// <param name="taskIdList">Список идентификаторов задач</param>
        public void DeleteTasks(FQRequestInfo ri, List<Guid> taskIdList)
        {
            try
            {
                logger.Debug($"Поиск и удаление задач по идентификаторам");

                //Проверка статуса пользователя
                if (ri._User.Role != RoleTypes.Parent)
                {
                    logger.Error(FQServiceExceptionType.NotEnoughRights.ToString());
                    throw new FQServiceException(FQServiceExceptionType.NotEnoughRights);
                }

                DBWorker.DeleteTasks(ri._User.GroupId, taskIdList);
            }
            catch (FQServiceException fqEx)
            {
                logger.Error(fqEx);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }
        }

        /// <summary>
        /// Удаление задач по признакам.
        /// </summary>
        /// <param name="searchParams">Искомые значения полей, с которыми нужно удалить задачи</param>
        public void DeleteTasks(FQRequestInfo ri, Dictionary<string, object> searchParams)
        {
            try
            {
                logger.Debug($"Поиск и удаление задач");

                //Проверка статуса пользователя
                if (ri._User.Role != RoleTypes.Parent)
                {
                    logger.Error(FQServiceExceptionType.NotEnoughRights.ToString());
                    throw new FQServiceException(FQServiceExceptionType.NotEnoughRights);
                }

                searchParams["OwnerGroup"] = ri._User.GroupId.ToString();

                DBWorker.DeleteTasks(searchParams);                
            }
            catch (FQServiceException fqEx)
            {
                logger.Error(fqEx);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }
        }

        /// <summary>
        /// Получение задач по списку идентификаторов
        /// </summary>
        /// <param name="taskIdList">Список идентификаторов</param>
        /// <returns>Список задач</returns>
        public List<BaseTask> GetTasks(List<Guid> taskIdList)
        {
            try
            {
                logger.Debug($"Получение задач по идентификаторам");

                var tasks = DBWorker.GetTasks(taskIdList);

                if (tasks.Count != taskIdList.Count)
                {
                    throw new FQServiceException(FQServiceExceptionType.ItemNotFound);
                }

                return tasks;
            }
            catch (FQServiceException fqEx)
            {
                logger.Error(fqEx);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }
        }

        /// <summary>
        /// Получение задач по признакам
        /// </summary>
        /// <param name="searchParams">Искомые значения полей, задачи с которыми нужно вернуть</param>
        /// <returns>Список задач</returns>        
        public List<BaseTask> GetTasks(Dictionary<string, object> searchParams)
        {
            try
            {
                logger.Debug($"Поиск задач");
                return DBWorker.GetTasks(searchParams);
            }
            catch (FQServiceException fqEx)
            {
                logger.Error(fqEx);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }
        }

        /// <summary>
        /// Получение задач по признакам (с ограничениям польователей)
        /// </summary>
        /// <param name="searchParams">Искомые значения полей, задачи с которыми нужно вернуть</param>
        /// <returns>Список задач</returns>        
        public List<BaseTask> GetUserTasks(FQRequestInfo ri, Dictionary<string, object> searchParams)
        {
            try
            {
                logger.Debug($"Поиск пользовательских задач");
                return DBWorker.GetUserTasks(ri, searchParams);
            }
            catch (FQServiceException fqEx)
            {
                logger.Error(fqEx);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }
        }

        /// <summary>
        /// Обновление информации о задаче.
        /// </summary>
        /// <remarks>
        /// Поля <see cref="BaseTask.Id"/> и <see cref="BaseTask.Creator"/> не изменяются.
        /// Поле <see cref="BaseTask.Status"/> обновляется через функцию <see cref="UpdateTaskStatus(List{Guid}, BaseTaskStatus, FQRequestInfo)"/>.
        /// Поля <see cref="BaseTask.CreationDate"/>, <see cref="BaseTask.CompletionDate"/> не изменяются напрямую.
        /// Поле <see cref="BaseTask.ModificationTime"/> изменяется автоматически.
        /// </remarks>
        /// <param name="taskId">Идентификатор обновляемой задачи</param>
        /// <param name="newParams">Набор новых значений полей задачи</param>        
        /// <returns>Идентификатор измененной задачи</returns>
        public void UpdateTask(Guid taskId, Guid groupId, Dictionary<string, object> newParams)
        {
            try
            {
                logger.Debug($"Обновление задачи '{taskId}'");

                // Изменение ID недопустимо
                if (newParams.ContainsKey("Id"))
                    newParams.Remove("Id");

                if (newParams.ContainsKey("OwnerGroup"))
                {
                    newParams.Remove("OwnerGroup");
                }

                // Изменение времени недопустимо
                if (newParams.ContainsKey("CreationDate"))
                    newParams.Remove("CreationDate");
                if (newParams.ContainsKey("CompletionDate"))
                    newParams.Remove("CompletionDate");

                // Изменение создателя недопустимо
                if (newParams.ContainsKey("Creator"))
                    newParams.Remove("Creator");

                // Изменение статуса напрямую недопустимо
                if (newParams.ContainsKey("Status"))
                    newParams.Remove("Status");

                // время последнего изменения устанавливается сервером самостоятельно
                if (newParams.ContainsKey("ModificationTime"))
                    newParams.Remove("ModificationTime");
                newParams.Add("ModificationTime", DateTime.UtcNow);

                DBWorker.UpdateTask(taskId, groupId, newParams);

            }
            catch (FQServiceException fqEx)
            {
                logger.Error(fqEx);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }
        }

        /// <summary>
        /// Изменение статуса задачи.
        /// </summary>
        /// <remarks>
        /// Статусы могут изменяться только в соответствии с графом в <see cref="TaskStatusRelations.Rels"/>.
        /// </remarks>
        /// <param name="taskIds">Идентификатор задачи</param>
        /// <param name="newStatus">Новый статус</param>
        /// <returns>Установленный статус</returns>
        /// TODO: рудимент? не добавлял сюда формирование хистори эвентов; не используется 
        //public BaseTaskStatus UpdateTaskStatus(List<Guid> taskIds, BaseTaskStatus newStatus)
        //{
        //    try
        //    {
        //        logger.Debug($"Обновление статуса {taskIds.Count} задач на '{Enum.GetName(typeof(BaseTaskStatus), newStatus)}'");

        //        // проверка правомерности установки нового статуса
        //        var currentTasks = DBWorker.GetTasks(taskIds);

        //        //исключим задания в статусе Deleted
        //        currentTasks = new List<BaseTask>(currentTasks.Where(x => x.Status != BaseTaskStatus.Deleted));

        //        if (currentTasks.Count != taskIds.Count)
        //        {
        //            throw new FQServiceException(FQServiceExceptionType.ItemNotFound);
        //        }

        //        foreach (var task in currentTasks)
        //        {
        //            if (!TaskStatusRelations.ValidateTransition(task.Status, newStatus))
        //                throw new Exception($"Изменение статуса задачи '{task.Id.ToString()}' на указанный непредусмотрено. "); 
        //        }

        //        var now = DateTime.UtcNow;
        //        Dictionary<string, object> keyValuePairs = new Dictionary<string, object>();
        //        switch (newStatus)
        //        {
        //            case BaseTaskStatus.Created:
        //                break;
        //            case BaseTaskStatus.Assigned:
        //                break;
        //            case BaseTaskStatus.Accepted:
        //                newStatus = BaseTaskStatus.InProgress;
                        
        //                break;
        //            case BaseTaskStatus.InProgress:
        //                break;
        //            case BaseTaskStatus.Completed:
        //                keyValuePairs.Add("CompletionDate", now);
        //                newStatus = BaseTaskStatus.PendingReview;
        //                break;
        //            case BaseTaskStatus.PendingReview:
        //                break;
        //            case BaseTaskStatus.Successed:
        //                TaskCompletedSuccessfulyHandler(currentTasks);
        //                break;
        //            case BaseTaskStatus.Closed:
        //                break;
        //            case BaseTaskStatus.Deleted:
        //                break;
        //            case BaseTaskStatus.Declined:
        //                TaskFailedManuallyHandler(currentTasks);
        //                break;
        //            case BaseTaskStatus.AvailableUntilPassed:
        //                TaskFailedAutomaticallyHandler(currentTasks);
        //                break;
        //            case BaseTaskStatus.SolutionTimeOver:
        //                TaskFailedAutomaticallyHandler(currentTasks);
        //                break;
        //            case BaseTaskStatus.Canceled:
        //                TaskFailedManuallyHandler(currentTasks);
        //                break;
        //            case BaseTaskStatus.Failed:
        //                TaskFailedManuallyHandler(currentTasks);
        //                break;
        //            case BaseTaskStatus.None:
        //                break;
        //            default:
        //                break;
        //        }

        //        keyValuePairs.Add("Status", (int)newStatus);
        //        keyValuePairs.Add("ModificationTime", now);

        //        // TODO: переделать на один запрос
        //        foreach (var task in taskIds)
        //        {
        //            DBWorker.UpdateTask(task, keyValuePairs); 
        //        }

        //        return newStatus;
        //    }
        //    catch (FQServiceException fqEx)
        //    {
        //        logger.Error(fqEx);
        //        throw;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex);
        //        throw new Exception(FQServiceExceptionType.DefaultError.ToString());
        //    }
        //}

        /// <summary>
        /// Изменение статуса задачи.
        /// </summary>
        /// <remarks>
        /// Статусы могут изменяться только в соответствии с графом в <see cref="TaskStatusRelations.Rels"/>.
        /// </remarks>
        /// <param name="taskIds">Идентификатор задачи</param>
        /// <param name="newStatus">Новый статус</param>
        /// <param name="ri">Данные запроса</param>
        /// <returns>Установленный статус</returns>
        public BaseTaskStatus UpdateTaskStatus(List<Guid> taskIds, BaseTaskStatus newStatus, FQRequestInfo ri, List<BaseTask> _currentTasks = null, bool isStartingItem = false)
        {
            try
            {
                if (_currentTasks == null)
                {
                    logger.Debug($"Обновление статуса {taskIds.Count} задач на '{Enum.GetName(typeof(BaseTaskStatus), newStatus)}'");
                }
                else
                {
                    logger.Debug($"Обновление статуса {_currentTasks.Count} задач на '{Enum.GetName(typeof(BaseTaskStatus), newStatus)}'");
                }

                inputRequest = ri.Clone();

                List<BaseTask> currentTasks = new List<BaseTask>();

                // проверка правомерности установки нового статуса
                if (_currentTasks == null)
                {
                    currentTasks = DBWorker.GetTasks(taskIds);

                    //исключим задания в статусе Deleted
                    currentTasks = new List<BaseTask>(currentTasks.Where(x => x.Status != BaseTaskStatus.Deleted));

                    if (currentTasks.Count != taskIds.Count)
                    {
                        throw new FQServiceException(FQServiceExceptionType.ItemNotFound);
                    }
                }
                else
                {
                    currentTasks = _currentTasks;
                }

                foreach (var task in currentTasks)
                {
                    if (!TaskStatusRelations.ValidateTransition(task.Status, newStatus))
                    {
                        logger.Error($"Изменение статуса задачи '{task.Id.ToString()}' на '{Enum.GetName(typeof(BaseTaskStatus), newStatus)} непредусмотрено. ");
                        throw new FQServiceException(FQServiceExceptionType.UnsupportedStatusChanging);
                    }
                }

                var now = DateTime.UtcNow;
                Dictionary<string, object> keyValuePairs = new Dictionary<string, object>();
                switch (newStatus)
                {
                    case BaseTaskStatus.Created:
                        break;
                    case BaseTaskStatus.Assigned:
                        keyValuePairs.Add("Creator", ri._Account.userId);
                        keyValuePairs.Add("CreationDate", now);

                        //Проверка превышения лимита активных задач
                        var activeTasksCount = DBWorker.GetActiveTasksCount(ri._User.GroupId);

                        if (ri._Group.SubscriptionIsActive)
                        {
                            if (activeTasksCount >= CommonLib.Settings.Current[Settings.Name.Task.maxTasks_Extension, CommonData.maxTasks_Extension])
                            {
                                throw new FQServiceException(FQServiceExceptionType.LimitAchieved);
                            }
                        }
                        else
                        {
                            if (activeTasksCount >= CommonLib.Settings.Current[Settings.Name.Task.maxTasks_NotExtension, CommonData.maxTasks_NotExtension])
                            {
                                throw new FQServiceException(FQServiceExceptionType.LimitAchieved);
                            }
                        }                        

                        break;
                    case BaseTaskStatus.Accepted:
                        newStatus = BaseTaskStatus.InProgress;
                        keyValuePairs.Add("Executor", ri._Account.userId);
                        break;
                    case BaseTaskStatus.InProgress:
                        break;
                    case BaseTaskStatus.Completed:
                        keyValuePairs.Add("CompletionDate", now);
                        newStatus = BaseTaskStatus.PendingReview;
                        break;
                    case BaseTaskStatus.PendingReview:
                        break;
                    case BaseTaskStatus.Successed:
                        TaskCompletedSuccessfulyHandler(currentTasks);
                        break;
                    case BaseTaskStatus.Closed:
                        break;
                    case BaseTaskStatus.Deleted:
                        break;
                    case BaseTaskStatus.Declined:
                        TaskFailedManuallyHandler(currentTasks);
                        break;
                    case BaseTaskStatus.AvailableUntilPassed:
                        TaskFailedAutomaticallyHandler(currentTasks);
                        break;
                    case BaseTaskStatus.SolutionTimeOver:
                        TaskFailedAutomaticallyHandler(currentTasks);
                        break;
                    case BaseTaskStatus.Canceled:
                        TaskCompletedSuccessfulyHandler(currentTasks);
                        break;
                    case BaseTaskStatus.Failed:
                        TaskFailedManuallyHandler(currentTasks);
                        break;
                    case BaseTaskStatus.None:
                        break;
                    default:
                        break;
                }

                keyValuePairs.Add("Status", (int)newStatus);
                keyValuePairs.Add("ModificationTime", now);

                // TODO: переделать на один запрос
                foreach (var task in currentTasks)
                {
                    DBWorker.UpdateTask(task.Id, ri._User.GroupId, keyValuePairs);

                    if (!isStartingItem)
                    {
                        //Запись HistoryEvent-а
                        try
                        {
                            FQRequestInfo ri_CreateHistoryEvent = ri.Clone();

                            //Инициализация параметров
                            bool shouldWrite = false;
                            HistoryEvent.MessageTypeEnum msgType = HistoryEvent.MessageTypeEnum.Default;
                            HistoryEvent.VisabilityEnum msgVisability = HistoryEvent.VisabilityEnum.Default;

                            List<Guid> availableFor = new List<Guid>();

                            Guid doer = ri._User.Id;

                            //Заполнение в зависимости от нового статуса
                            switch (newStatus)
                            {
                                case BaseTaskStatus.Created:
                                    {
                                        break;
                                    }
                                case BaseTaskStatus.Assigned:
                                    {
                                        shouldWrite = true;

                                        msgType = HistoryEvent.MessageTypeEnum.Task_Published;

                                        if (task.AvailableFor.Length == 0)
                                        {
                                            msgVisability = HistoryEvent.VisabilityEnum.Group;
                                        }
                                        else
                                        {
                                            msgVisability = HistoryEvent.VisabilityEnum.Children;
                                            availableFor = new List<Guid>(task.AvailableFor);
                                        }

                                        break;
                                    }
                                case BaseTaskStatus.Accepted:
                                    {
                                        break;
                                    }
                                case BaseTaskStatus.InProgress:
                                    {
                                        shouldWrite = true;

                                        if (task.Status == BaseTaskStatus.Assigned || task.Status == BaseTaskStatus.Accepted)
                                        {
                                            msgType = HistoryEvent.MessageTypeEnum.Task_ChangedStatus_InProgress;

                                            if (task.AvailableFor.Length == 0)
                                            {
                                                msgVisability = HistoryEvent.VisabilityEnum.Group;
                                            }
                                            else
                                            {
                                                msgVisability = HistoryEvent.VisabilityEnum.Children;

                                                if (task.AvailableFor.Length > 1)
                                                {
                                                    //Провернем хитрость:
                                                    //Кроме Executor-а, которому (в HistoryEvent-сервисе) покажем "Началось выполнение...",
                                                    //нужно сохранить всех остальных из AvailableFor, которым покажем, что задачу забрал другой юзер.
                                                    //Чтобы отличить Executor-a (в HistoryEvent-сервисе) условимся, что Executor последний в списке.

                                                    List<Guid> availableForWithOutExecutor = new List<Guid>(task.AvailableFor);
                                                    availableForWithOutExecutor.Remove(ri._User.Id);

                                                    availableFor = new List<Guid>(availableForWithOutExecutor);
                                                }

                                                availableFor.Add(ri._User.Id);
                                            }
                                        }
                                        else //if (task.Status == BaseTaskStatus.PendingReview)                                                                                                           
                                        {
                                            msgType = HistoryEvent.MessageTypeEnum.Task_ChangedStatus_Redo;
                                            msgVisability = HistoryEvent.VisabilityEnum.Children;
                                            availableFor.Add(task.Executor);
                                        }

                                        break;
                                    }
                                case BaseTaskStatus.Completed:
                                    {
                                        break;
                                    }
                                case BaseTaskStatus.PendingReview:
                                    {
                                        shouldWrite = true;
                                        msgType = HistoryEvent.MessageTypeEnum.Task_ChangedStatus_Verification;
                                        msgVisability = HistoryEvent.VisabilityEnum.Children;
                                        availableFor.Add(task.Executor);
                                        break;
                                    }
                                case BaseTaskStatus.Successed:
                                    {
                                        shouldWrite = true;
                                        msgType = HistoryEvent.MessageTypeEnum.Task_ChangedStatus_Completed;
                                        msgVisability = HistoryEvent.VisabilityEnum.Children;
                                        availableFor.Add(task.Executor);
                                        break;
                                    }
                                case BaseTaskStatus.Closed:
                                    {
                                        break;
                                    }
                                case BaseTaskStatus.Deleted:
                                    {
                                        shouldWrite = true;
                                        msgType = HistoryEvent.MessageTypeEnum.Task_Removed;
                                        msgVisability = HistoryEvent.VisabilityEnum.Parents;
                                        break;
                                    }
                                case BaseTaskStatus.Declined:
                                    {
                                        shouldWrite = true;
                                        msgType = HistoryEvent.MessageTypeEnum.Task_ChangedStatus_Declined;
                                        msgVisability = HistoryEvent.VisabilityEnum.Children;
                                        availableFor.Add(task.Executor);
                                        break;
                                    }
                                case BaseTaskStatus.AvailableUntilPassed:
                                    {
                                        shouldWrite = true;
                                        msgType = HistoryEvent.MessageTypeEnum.Task_ChangedStatus_AvailableUntilPassed;

                                        if (task.AvailableFor.Length == 0)
                                        {
                                            msgVisability = HistoryEvent.VisabilityEnum.Group;
                                        }
                                        else
                                        {
                                            msgVisability = HistoryEvent.VisabilityEnum.Children;
                                            availableFor = new List<Guid>(task.AvailableFor);
                                        }

                                        doer = Guid.Empty;

                                        break;
                                    }
                                case BaseTaskStatus.SolutionTimeOver:
                                    {
                                        shouldWrite = true;
                                        msgType = HistoryEvent.MessageTypeEnum.Task_ChangedStatus_SolutionTimeOver;
                                        msgVisability = HistoryEvent.VisabilityEnum.Children;
                                        availableFor.Add(task.Executor);
                                        doer = Guid.Empty;
                                        break;
                                    }
                                case BaseTaskStatus.Canceled:
                                    {
                                        shouldWrite = true;

                                        if (task.Status == BaseTaskStatus.Assigned)
                                        {
                                            msgType = HistoryEvent.MessageTypeEnum.Task_Canceled_Available;

                                            if (task.AvailableFor.Length == 0)
                                            {
                                                msgVisability = HistoryEvent.VisabilityEnum.Group;
                                            }
                                            else
                                            {
                                                msgVisability = HistoryEvent.VisabilityEnum.Children;
                                                availableFor = new List<Guid>(task.AvailableFor);
                                            }
                                        }
                                        else //if (task.Status == BaseTaskStatus.InProgress)
                                        {
                                            msgType = HistoryEvent.MessageTypeEnum.Task_Canceled_InProgress;
                                            msgVisability = HistoryEvent.VisabilityEnum.Children;
                                            availableFor.Add(task.Executor);
                                        }

                                        break;
                                    }
                                case BaseTaskStatus.Failed:
                                    {
                                        shouldWrite = true;
                                        msgType = HistoryEvent.MessageTypeEnum.Task_ChangedStatus_Failed;
                                        msgVisability = HistoryEvent.VisabilityEnum.Children;
                                        availableFor.Add(task.Executor);
                                        break;
                                    }
                                case BaseTaskStatus.None:
                                    break;
                                default:
                                    break;
                            }

                            if (shouldWrite)
                            {
                                HistoryEvent he = new HistoryEvent(HistoryEvent.ItemTypeEnum.Task, msgType, msgVisability, task.Id, availableFor, doer);

                                ri_CreateHistoryEvent.RequestData.actionName = "CreateHistoryEvent";
                                ri_CreateHistoryEvent.RequestData.postData = JsonConvert.SerializeObject(he);

                                RouteInfo.RouteToService(ri_CreateHistoryEvent, _httpContextAccessor);
                            }
                        }
                        catch (Exception)
                        {
                            //Если по какой-то причине не улаось записать HistoryEvent - не повод  прерывать операцию и кидать пользователю Error.
                        }
                    }
                }

                return newStatus;
            }
            catch (FQServiceException fqEx)
            {
                logger.Error(fqEx);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }
            finally
            {
                if (inputRequest != null)
                {
                    inputRequest = null;
                }
            }
        }


        public void UpdateRelatedTasksStatus(FQRequestInfo ri)
        {
            try
            {
                Guid removingUserId = JsonConvert.DeserializeObject<Guid>(ri.RequestData.postData.ToString());

                logger.Trace($"userId: ${ri._Account.userId}");
                logger.Trace($"removingUserId: ${removingUserId}");

                if (removingUserId == Guid.Empty)
                {
                    throw new Exception("Ошибка: removingUserId == Guid.Empty.");
                }

                //Проверка статуса пользователя
                if (ri._User.Role != RoleTypes.Parent)
                {
                    throw new FQServiceException(FQServiceExceptionType.NotEnoughRights);
                }

                List<Guid> relatedTasks = DBWorker.GetRelatedTasks(ri._User.GroupId, removingUserId);
                                
                Dictionary<string, object> keyValuePairs = new Dictionary<string, object>();
                keyValuePairs.Add("Status", (int)BaseTaskStatus.Deleted);
                keyValuePairs.Add("ModificationTime", DateTime.UtcNow);

                // TODO: переделать на один запрос
                foreach (var taskId in relatedTasks)
                {
                    DBWorker.UpdateTask(taskId, ri._User.GroupId, keyValuePairs);

                    //TODO: пока отключим чтобы не заваливать ленту юзера
                    ////Запись HistoryEvent-а
                    //try
                    //{
                    //    FQRequestInfo ri_CreateHistoryEvent = ri.Clone();

                    //    List<Guid> availableFor = new List<Guid>();

                    //    HistoryEvent he = new HistoryEvent(HistoryEvent.ItemTypeEnum.Task, HistoryEvent.MessageTypeEnum.Task_Canceled_Related, HistoryEvent.VisabilityEnum.Parents, taskId, availableFor, ri._User.Id);

                    //    ri_CreateHistoryEvent.RequestData.actionName = "CreateHistoryEvent";
                    //    ri_CreateHistoryEvent.RequestData.postData = JsonConvert.SerializeObject(he);

                    //    RouteInfo.RouteToService(ri_CreateHistoryEvent, _httpContextAccessor);
                    //}
                    //catch (Exception)
                    //{
                    //    //Если по какой-то причине не улаось записать HistoryEvent - не повод  прерывать операцию и кидать пользователю Error.
                    //}                  
                }

                //DBWorker.UpdateRelatedTasks_Executor(ri._User.GroupId, removingUserId);
                //DBWorker.UpdateRelatedTasks_AvailableFor(ri._User.GroupId, removingUserId);

                DBWorker.UpdateRelatedTasks(ri._User.GroupId, removingUserId);                
            }
            catch (FQServiceException fqEx)
            {
                logger.Error(fqEx);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }
            finally
            {
                
            }
        }

        private void TaskCompletedSuccessfulyHandler(List<BaseTask> tasks)
        {
            // начисление бабосов
            foreach (var task in tasks)
            {
                if (task.Cost > 0 && task.Executor != Guid.Empty)
                    ChangeUserCoins(task.Executor, task.Cost);
            }
            
            // проверка начисления бонусов

            return;
        }

        private void TaskFailedAutomaticallyHandler(List<BaseTask> tasks)
        {
            //применить штраф
            foreach (var task in tasks)
            {
                if (task.Penalty > 0 && task.Executor != Guid.Empty)
                    ChangeUserCoins(task.Executor, (-task.Penalty));
            }
            return;
        }
        private void TaskFailedManuallyHandler(List<BaseTask> tasks)
        {
            //применить штраф
            foreach (var task in tasks)
            {
                if (task.Penalty > 0 && task.Executor != Guid.Empty)
                    ChangeUserCoins(task.Executor, (-task.Penalty));
            }

            return;
        }

        private void ChangeUserCoins(Guid userId, int coinValue)
        {
            try
            {
                logger.Debug($"Запрос на изменение количества монет пользователя {userId}.");

                var requestToCredentialService = inputRequest.Clone();
                requestToCredentialService.RequestData.actionName = "MakePayment";

                Dictionary<string, object> postData = new Dictionary<string, object>
                {
                    { "Id", userId },
                    { "Coins", coinValue }
                };
                requestToCredentialService.RequestData.postData = JsonConvert.SerializeObject(postData);

                //Если не вываливаемся в catch - значит, всё успешно. Доп проверки не нужны.
                RouteInfo.RouteToService(requestToCredentialService, _httpContextAccessor);
            }
            catch (FQServiceException fqEx)
            {
                logger.Error(fqEx);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }
        }
    }
}
