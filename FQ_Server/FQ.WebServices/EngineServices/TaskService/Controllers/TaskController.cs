using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonLib;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TaskService.Services;
using TaskService.Models;
using static CommonTypes.User;
using static CommonLib.FQServiceException;

namespace TaskService.Controllers
{
    [ApiController]
    public class TaskController : ControllerBase
    {
        static NLog.Logger logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
        
        private readonly ITaskService _service;

        public TaskController(ITaskService services)
        {
            _service = services;
        }

        private ActionResult<FQResponseInfo> ExceptionHandler(Exception ex)
        {
            logger.Error(ex);

            if (ex is ArgumentException ||
                ex is ArgumentNullException ||
                ex is ArgumentOutOfRangeException ||
                ex is InvalidCastException)
            {
                return StatusCode(400, ex.Message);
            }

            // остальные ошибки
            return StatusCode(500);
        }

        private static List<Guid> DeserializeGuidList(string serializedString)
        {
            List<Guid> deserializedGuidList = new List<Guid>();
            try
            {
                deserializedGuidList = JsonConvert.DeserializeObject<List<Guid>>(serializedString);
                if (deserializedGuidList.Count == 0)
                    throw new Exception();
            }
            catch (Exception)
            {
                logger.Error("Ошибка получения входящих параметров");
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }            

            return deserializedGuidList;
        }


        private static BaseTask DeserializeBaseTask(string serializedString)
        {
            BaseTask deserializedTask = new BaseTask();
            try
            {
                deserializedTask = BaseTask.TryDeserialize(serializedString);
            }
            catch (Exception)
            {
                logger.Error("Ошибка получения входящих параметров");
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }

            return deserializedTask;
        }


        private static Dictionary<string, object> DeserializeDictStringObject(string serializedString)
        {
            Dictionary<string, object> deserializedDict = new Dictionary<string, object>();
            try
            {
                deserializedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(serializedString);
                if (deserializedDict.Count == 0)
                    throw new Exception();
            }
            catch (Exception)
            {
                logger.Error("Ошибка получения входящих параметров");
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }

            return deserializedDict;
        }

        /// <summary>
        /// Контроллер Action-а создания новой задачи
        /// </summary>
        /// <param name="ri"></param>
        /// <returns>Информация о созданной задаче</returns>
        [HttpPost]
        [Route("CreateTask")]
        public ActionResult<FQResponseInfo> CreateTaskController(FQRequestInfo ri)
        {
            logger.Trace("CreateTaskController");
            try
            {
                var inputTaskInfo = DeserializeBaseTask(ri.RequestData.postData.ToString());

                // проверка обязательных параметров
                if (inputTaskInfo.Cost == default(int) ||
                    inputTaskInfo.Name == string.Empty)
                {
                    logger.Error(FQServiceExceptionType.EmptyRequiredField.ToString());
                    throw new FQServiceException(FQServiceExceptionType.EmptyRequiredField);
                }

                if (inputTaskInfo.Cost < 1 || inputTaskInfo.Penalty < 0)
                {
                    logger.Error(FQServiceExceptionType.DefaultError.ToString());
                    throw new FQServiceException(FQServiceExceptionType.DefaultError);
                }

                inputTaskInfo.Creator = ri._User.Id;
                inputTaskInfo.OwnerGroup = ri._User.GroupId;
                if (inputTaskInfo.AvailableFor == null || inputTaskInfo.AvailableFor.Length == 0)
                {
                    logger.Debug($"При создании задачи не заданы пользователи в inputTaskInfo.AvailableFor. Задача будет доступна для группы '{inputTaskInfo.OwnerGroup}'.");
                    inputTaskInfo.AvailableFor = new Guid[0];
                }

                var createdTask = _service.CreateTask(ri, inputTaskInfo);

                FQResponseInfo response = new FQResponseInfo(createdTask);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("CreateTaskController leave");
            }
        }

        /// <summary>
        /// Контроллер Action-а создания стартовых примеров задач (при регистрации новой группы)
        /// ВНУТРЕННИЙ ТЕХНИЧЕСКИЙ МЕТОД
        /// </summary>
        /// <param name="ri"></param>
        [HttpPost]
        [Route("CreateGroupStartingTasks")]
        public ActionResult<FQResponseInfo> CreateGroupStartingTasksController(FQRequestInfo ri)
        {
            logger.Trace("CreateGroupStartingTasksController");
            try
            {
                var startingTask_1 = new BaseTask();
                startingTask_1.Creator = ri._User.Id;
                startingTask_1.OwnerGroup = ri._User.GroupId;
                startingTask_1.AvailableFor = new Guid[0];
                startingTask_1.Name = "Сделать зарядку";
                startingTask_1.Description = "Выполнить упражнения для всех мышечных групп:\n - вращения для головы, туловища;\n - сгибание, разгибание, повороты, круговые движения для рук, ног;\n - наклоны вперед, назад, в сторону, ходьба, бег, прыжки).";
                startingTask_1.Cost = 10;
                _service.CreateTask(ri, startingTask_1, true);

                var startingTask_2 = new BaseTask();
                startingTask_2.Creator = ri._User.Id;
                startingTask_2.OwnerGroup = ri._User.GroupId;
                startingTask_2.AvailableFor = new Guid[0];
                startingTask_2.Name = "Прочитать 5 страниц";
                startingTask_2.Description = "Прочитать и пересказать содержание прочитанного (художественная или научно-популярная литература).";
                startingTask_2.Cost = 10;
                _service.CreateTask(ri, startingTask_2, true);

                ////Объявление
                //List<Guid> taskList = new List<Guid>();
                //taskList.Add(startingTask_1.Id);
                //taskList.Add(startingTask_2.Id);
                //_service.UpdateTaskStatus(taskList, BaseTaskStatus.Assigned, ri, null, true);

                FQResponseInfo response = new FQResponseInfo(true);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("CreateGroupStartingTasksController leave");
            }
        }

        /// <summary>
        /// Контроллер Action-а создания персональной задачи "Завершить обучение" (при добавлении нового пользователя)
        /// ВНУТРЕННИЙ ТЕХНИЧЕСКИЙ МЕТОД
        /// </summary>
        /// <param name="ri"></param>
        [HttpPost]
        [Route("CreateUserStartingTask")]
        public ActionResult<FQResponseInfo> CreateUserStartingTaskController(FQRequestInfo ri)
        {
            logger.Trace("CreateUserStartingTaskController");
            try
            {
                Guid destinationUserId = Guid.Empty;

                try
                {
                    destinationUserId = Guid.Parse(ri.RequestData.postData.ToString());

                    if (destinationUserId == Guid.Empty)
                    {
                        throw new Exception("Guid is Empty.");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    throw new Exception(FQServiceExceptionType.DefaultError.ToString());
                }                

                var startingTask_1 = new BaseTask();
                startingTask_1.Creator = ri._User.Id;
                startingTask_1.OwnerGroup = ri._User.GroupId;
                startingTask_1.AvailableFor = new Guid[1] { destinationUserId };
                startingTask_1.Name = "Завершить обучение";
                startingTask_1.Description = " - Начать выполнение задания;\n - перейти к вкладке 'Активные';\n - выбрать выполняемое задание;\n - подтвердить выполнение.";
                startingTask_1.Cost = 5;

                //Создан черновик
                var createdTask = _service.CreateTask(ri, startingTask_1, true);

                //Объявление
                List<Guid> taskList = new List<Guid>();
                taskList.Add(createdTask.Id);
                _service.UpdateTaskStatus(taskList, BaseTaskStatus.Assigned, ri, null, true);

                FQResponseInfo response = new FQResponseInfo(true);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("CreateUserStartingTaskController leave");
            }
        }

        /// <summary>
        /// Контроллер Action-а получения задачю
        /// ВНУТРЕННИЙ ТЕХНИЧЕСКИЙ МЕТОД: НЕ ВЫНОСИТЬ В ПУБЛИЧНЫЙ ДОСТУП - ОТСУТСТВУЕТ ПРОВЕРКА СООТВЕТСТВИЯ ГРУППЫ И РОЛИ
        /// </summary>
        /// <param name="ri"></param>
        /// <returns>Информация о созданной задаче</returns>
        [HttpPost]
        [Route("GetTasks")]
        public ActionResult<FQResponseInfo> GetTasksController(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("GetTasksController");

                var inputGuidList = DeserializeGuidList(ri.RequestData.postData.ToString());

                var foundTask = _service.GetTasks(inputGuidList);

                FQResponseInfo response = new FQResponseInfo(foundTask);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("GetTasksController leave");
            }
        }

        /// <summary>
        /// Контроллер Action-а поиска задач
        /// </summary>
        /// <param name="ri"></param>
        /// <returns>Информация о найденной задаче</returns>
        [HttpPost]
        [Route("SearchTasks")]
        public ActionResult<FQResponseInfo> SearchTasksController(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("SearchTasksController");

                CheckRequestInfo(ri, checkPostData: false);

                BaseTask inputTaskInfo = new BaseTask();
                if (ri.RequestData.postData != null)
                {
                    inputTaskInfo = DeserializeBaseTask(ri.RequestData.postData.ToString()); // если упадем тут, что-то не так во входящих параметрах   
                }

                // Ограничиваем выдачу 
                // в любом случае выдаем только группу текущего пользователя
                logger.Trace("Получение группы пользователя.");
                inputTaskInfo.OwnerGroup = ri._User.GroupId;
                logger.Trace($"inputTaskInfo.OwnerGroup: {inputTaskInfo.OwnerGroup}");

                // не админ не должен видеть Created, Closed, Deleted, None
                List<BaseTask> foundTask = new List<BaseTask>();
                if (ri._User.Role == RoleTypes.Parent)
                {
                    foundTask = _service.GetTasks(BaseTask.ToDictionary(inputTaskInfo));
                }
                else
                {
                    foundTask = _service.GetUserTasks(ri, BaseTask.ToDictionary(inputTaskInfo));

                    //Не будем показывать пользователю в завершенных задачи AvailableUntilPassed, которые истекли до добавления пользователя, чтобы не засорять и не дезориентировать
                    foundTask = new List<BaseTask>(foundTask.Where(x => !(x.Status == BaseTaskStatus.AvailableUntilPassed && x.ModificationTime <= ri._Account.CreationDate)));
                }

                FQResponseInfo response = new FQResponseInfo(foundTask);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("SearchTasksController leave.");
            }
        }

        /// <summary>
        /// Контроллер Action-а удаления задач
        /// Зарезервирован - наружу не смотрит
        /// </summary>
        /// <param name="ri"></param>
        /// <returns>Информация о созданной задаче</returns>
        [HttpPost]
        [Route("RemoveTasks")]
        public ActionResult<FQResponseInfo> RemoveTasksController(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("RemoveTasksController");
                CheckRequestInfo(ri);

                var inputGuidList = DeserializeGuidList(ri.RequestData.postData.ToString());
                                
                _service.DeleteTasks(ri, inputGuidList);

                FQResponseInfo response = new FQResponseInfo(true);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("RemoveTasksController leave");
            }
        }

        /// <summary>
        /// Контроллер Action-а поиска задач
        /// Зарезервирован - наружу не смотрит
        /// </summary>
        /// <param name="ri"></param>
        /// <returns>Информация о созданной задаче</returns>
        [HttpPost]
        [Route("RemoveSearchTasks")]
        public ActionResult<FQResponseInfo> RemoveSearchTasksController(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("RemoveSearchTasksController");
                CheckRequestInfo(ri);

                var inputTaskInfo = DeserializeBaseTask(ri.RequestData.postData.ToString()); // если упадем тут, что-то не так во входящих параметрах
                                                                                             //var inputSearchInfo = DeserializeDictStringObject(ri.RequestData.postData.ToString());
                
                _service.DeleteTasks(ri, BaseTask.ToDictionary(inputTaskInfo));

                FQResponseInfo response = new FQResponseInfo(true);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("RemoveSearchTasksController leave");
            }
        }
        /// <summary>
        /// Контроллер Action-а изменения задач
        /// </summary>
        /// <param name="ri"></param>
        /// <returns>Информация о созданной задаче</returns>
        [HttpPost]
        [Route("UpdateTasks")]
        public ActionResult<FQResponseInfo> UpdateTasksController(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("UpdateTasksController");
                CheckRequestInfo(ri);

                var inputTaskInfo = DeserializeBaseTask(ri.RequestData.postData.ToString()); // если упадем тут, что-то не так во входящих параметрах
                var inputSearchInfo = DeserializeDictStringObject(ri.RequestData.postData.ToString());

                // TODO: переделать изменение отдельных полей 
                // (особенно сейчас страдает приведение поля к дефолтному значению)
                foreach(var inTaskParam in BaseTask.ToDictionaryFull(inputTaskInfo))
                {
                    if (inputSearchInfo.ContainsKey(inTaskParam.Key))
                        inputSearchInfo[inTaskParam.Key] = inTaskParam.Value;
                }

                //Проверка статуса пользователя
                if (ri._User.Role != RoleTypes.Parent)
                {
                    logger.Error(FQServiceExceptionType.NotEnoughRights.ToString());
                    throw new FQServiceException(FQServiceExceptionType.NotEnoughRights);
                }

                // проверка обязательных параметров
                int cost = 0;
                if ((inputSearchInfo.ContainsKey("Name") && inputSearchInfo["Name"].ToString() == string.Empty) ||
                    (inputSearchInfo.ContainsKey("Cost") && (!Int32.TryParse(inputSearchInfo["Cost"].ToString(), out cost) || cost == default(int))))
                {
                    logger.Error(FQServiceExceptionType.EmptyRequiredField.ToString());
                    throw new FQServiceException(FQServiceExceptionType.EmptyRequiredField);
                }

                if (cost < 1 ||
                    (inputSearchInfo.ContainsKey("Penalty") && (!Int32.TryParse(inputSearchInfo["Penalty"].ToString(), out int penalty) || penalty < 0)))
                {
                    logger.Error(FQServiceExceptionType.DefaultError.ToString());
                    throw new FQServiceException(FQServiceExceptionType.DefaultError);
                }

                //Сначала проверка - существует и доступна ли для апдейта задача
                var selectSearhInfo = new Dictionary<string, object>();
                selectSearhInfo.Add("Id", inputTaskInfo.Id);
                selectSearhInfo.Add("OwnerGroup", ri._User.GroupId);

                var foundTask = _service.GetTasks(selectSearhInfo).FirstOrDefault();

                if (foundTask == null || foundTask.Status == BaseTaskStatus.Deleted)
                {
                    throw new FQServiceException(FQServiceExceptionType.ItemNotFound);
                }

                //Теперь при редактировании задания отправляем текущий статус
                //Чтобы другой родитель, который не обновил инфу и видит задание (реально находящееся на проверке) в статусе черновика, 
                //не мог вносить в него правки
                if ((foundTask.Status != BaseTaskStatus.Created && foundTask.Status != BaseTaskStatus.PendingReview) ||
                    !inputSearchInfo.ContainsKey("Status") ||
                    !Int32.TryParse(inputSearchInfo["Status"].ToString(), out int statusFrominput) ||
                    (BaseTaskStatus)statusFrominput != foundTask.Status)
                {
                    logger.Error("foundTask.Status: " + foundTask.Status.ToString());
                    throw new FQServiceException(FQServiceExceptionType.UnsupportedStatusChanging);
                }

                _service.UpdateTask(inputTaskInfo.Id, ri._User.GroupId, inputSearchInfo);

                var updatedTask = _service.GetTasks(new List<Guid>() { inputTaskInfo.Id } );

                FQResponseInfo response = new FQResponseInfo(updatedTask[0]);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("UpdateTasksController leave");
            }
        }


        [HttpPost]
        [Route("UpdateRelatedTasks")]
        public ActionResult<FQResponseInfo> UpdateRelatedTasksController(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("UpdateRelatedTasksController");
                
                _service.UpdateRelatedTasksStatus(ri);               

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("UpdateRelatedTasksController leave");
            }
        }

        /// <summary>
        /// Контроллер Action-а изменения статуса задач
        /// </summary>
        /// <param name="ri"></param>
        /// <returns>Информация о задачах</returns>
        [HttpPost]
        [Route("UpdateTaskStatus")]
        public ActionResult<FQResponseInfo> UpdateTaskStatusController(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("UpdateTaskStatusController");
                CheckRequestInfo(ri);

                //var inputTaskInfo = DeserializeBaseTask(ri.RequestData.postData.ToString()); // если упадем тут, что-то не так во входящих параметрах
                var inputSearchInfo = DeserializeDictStringObject(ri.RequestData.postData.ToString());
                List<Guid> taskList = DeserializeGuidList(inputSearchInfo["GuidList"].ToString());
                BaseTaskStatus newStatus = Utils.StatusFromString(inputSearchInfo["NewStatus"].ToString());

                //Проверка статуса пользователя
                if (newStatus == BaseTaskStatus.Accepted ||
                    newStatus == BaseTaskStatus.Completed ||
                    newStatus == BaseTaskStatus.PendingReview ||
                    newStatus == BaseTaskStatus.Declined)
                {
                    if (ri._User.Role != RoleTypes.Children)
                    {
                        logger.Error("Ошибка: операция доступна только для ребенка.");
                        throw new FQServiceException(FQServiceExceptionType.DefaultError); //вовзращаем дефолтный еррор, потому что это не "недостаточно прав"
                    }
                }
                else
                {
                    if (ri._User.Role != RoleTypes.Parent)
                    {
                        logger.Error(FQServiceExceptionType.NotEnoughRights.ToString());
                        throw new FQServiceException(FQServiceExceptionType.NotEnoughRights); //здесь "недостаточно прав"
                    }
                }                

                _service.UpdateTaskStatus(taskList, newStatus, ri);

                FQResponseInfo response = new FQResponseInfo(true);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("UpdateTaskStatusController leave");
            }
        }
        
        private static void CheckRequestInfo(FQRequestInfo ri, bool checkCredentials = true, bool checkAccount = true, bool checkUser = true, bool checkRequestData = true, bool checkPostData = true)
        {
            try
            {
                if (ri == null)
                {
                    logger.Error("Запрос пуст (ri == null)");
                    throw new Exception();
                }
                if (checkCredentials && ri.Credentials == null)
                {
                    logger.Error("Учетные данные запроса пусты (Credentials == null)");
                    throw new Exception();
                }
                if (checkAccount && ri._Account == null)
                {
                    logger.Error("Аккаунт пользователя пуст (_Account == null)");
                    throw new Exception();
                }
                if (checkUser && ri._User == null)
                {
                    logger.Error("Учетные данные пользователя пусты (_User == null)");
                    throw new Exception();
                }
                if (checkRequestData && ri.RequestData == null)
                {
                    logger.Error("Тело запроса пусто (RequestData == null)");
                    throw new Exception();
                }
                if (checkPostData && ri.RequestData.postData == null)
                {
                    logger.Error("Тело запроса пусто (postData == null)");
                    throw new Exception();
                }
            }
            catch
            {
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }
        }

    }
}