using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using CommonLib;
using CommonRoutes;
using CommonTypes;
using EventService.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using static CommonLib.FQServiceException;

namespace EventService.Services
{
    /// <summary>
    /// Реализация интерфейса сервиса-приёмника запросов клиента
    /// </summary>
    public class EventServices : IEventServices
    {
        static NLog.Logger logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        private List<Dictionary<string, object>> targetTasks = new List<Dictionary<string, object>>();
        private List<Reward> targetRewards = new List<Reward>();
        private List<User> targetUsers = new List<User>();

        private Dictionary<HistoryEvent.MessageTypeEnum, Dictionary<string, string>> MessagTypeText_Parent;
        private Dictionary<HistoryEvent.MessageTypeEnum, Dictionary<string, string>> MessagTypeText_Children;

        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Default constructor with HTTPContext
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        public EventServices(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public HistoryEvent GetHistoryEventFromPostData(object inputParams)
        {
            try
            {
                logger.Trace("GetHistoryEventFromPostData started.");

                HistoryEvent historyEvent = new HistoryEvent(true);
                historyEvent = JsonConvert.DeserializeObject<HistoryEvent>(inputParams.ToString());

                logger.Trace($"ItemType: {historyEvent.ItemType.ToString()}");
                logger.Trace($"MessageType: {historyEvent.MessageType.ToString()}");
                logger.Trace($"Visability: {historyEvent.Visability.ToString()}");
                logger.Trace($"TargetItem: {historyEvent.TargetItem.ToString()}");
                logger.Trace($"AvailableFor: {historyEvent.AvailableFor.Count.ToString()}");
                logger.Trace($"Doer: {historyEvent.Doer.ToString()}");

                return historyEvent;
            }
            finally
            {
                logger.Trace("GetHistoryEventFromPostData leave.");
            }
        }

        public void CreateHistoryEvent(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("CreateHistoryEvent started.");

                HistoryEvent historyEvent = GetHistoryEventFromPostData(ri.RequestData.postData);                
                
                if (ri._User == null || ri._User.GroupId == Guid.Empty)
                {
                    throw new Exception("Ошибка: не удалось получить GroupId.");
                }

                if (historyEvent.ItemType == HistoryEvent.ItemTypeEnum.Default ||
                    historyEvent.MessageType == HistoryEvent.MessageTypeEnum.Default ||
                    historyEvent.Visability == HistoryEvent.VisabilityEnum.Default ||
                    (historyEvent.Visability == HistoryEvent.VisabilityEnum.Children && (historyEvent.AvailableFor.Count == 0)) ||                    
                    historyEvent.TargetItem == Guid.Empty)
                {
                    throw new Exception("Ошибка: не заполнены обязательные поля.");
                }

                historyEvent.Id = Guid.NewGuid();
                historyEvent.GroupId = ri._User.GroupId;

                DBWorker.CreateHistoryEvent(historyEvent);

                //Получение шаблона и заполнение
                List<HistoryEvent> selectedEvents = new List<HistoryEvent>();
                selectedEvents.Add(historyEvent);
                HistoryEvent filledHistoryEvent = FillTemplates(ri, selectedEvents).FirstOrDefault();

                //Формирование (превентивного) списка пользователей, кому показывать пуши
                List<User> allUsers = GetAllUsers(ri);
                List<Guid> targetUsersIds = new List<Guid>();

                switch (filledHistoryEvent.Visability)
                {
                    case HistoryEvent.VisabilityEnum.Group:
                        targetUsersIds = new List<Guid>(allUsers.Select(x => x.Id));
                        break;
                    case HistoryEvent.VisabilityEnum.Parents:
                        targetUsersIds = new List<Guid>(allUsers.Where(x => (x.Role == User.RoleTypes.Parent))
                                                                    .Select(x => x.Id));
                        break;
                    case HistoryEvent.VisabilityEnum.Children:
                        targetUsersIds = new List<Guid>(allUsers.Where(x => (x.Role == User.RoleTypes.Parent) ||
                                                                            (x.Role == User.RoleTypes.Children && filledHistoryEvent.AvailableFor.Contains(x.Id)))
                                                                    .Select(x => x.Id));
                        break;
                }
                // Не уведомляем инициатора события
                targetUsersIds.Remove(filledHistoryEvent.Doer);

                if (filledHistoryEvent != null && targetUsersIds.Count != 0) // если оповещать-то некого
                {
                    // вызов Notification Service и не ожидаем результата
                    _ = SendNotificationsAsync(ri, filledHistoryEvent, targetUsersIds);
                }                
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
                logger.Trace("CreateHistoryEvent leave.");
            }
        }

        /// <summary>
        /// Выполняет запрос к NotificationService для отправки оповещения по событию
        /// </summary>
        /// <param name="currentRequest">Запрос к этому серверу</param>
        /// <param name="currentEvent">Текущее событие, по которому отправляется оповещение</param>
        /// <returns>Task</returns>
        public async Task SendNotificationsAsync(FQRequestInfo currentRequest, HistoryEvent currentEvent, List<Guid> targetUsersIds)
        {
            try
            {
                FQRequestInfo ri_SendMessage = currentRequest.Clone();
                // заполняем запрос
                NotificationInfo notification = new NotificationInfo();
                notification.Title = currentEvent.EventTitle;
                notification.Body = currentEvent.TargetItemName;
                notification.TargetsIds = targetUsersIds;
                notification.Data.Add("HistoryEvent", currentEvent.Serialize());

                ri_SendMessage.RequestData.actionName = "NotifyUsers";
                ri_SendMessage.RequestData.postData = notification.Serialize();

                // TODO: в релиз отключаем логгер? оставить
                await RouteInfo.RouteToServiceAsync(ri_SendMessage, _httpContextAccessor)
                    .ContinueWith((result) => logger.Debug($"Result of call: {result}"));
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to call NotificationService");
                return;
            }
            finally
            {
                logger.Trace("SendNotificationsAsync leave");
            }
        }

        public List<HistoryEvent> GetHistoryEvents(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("GetHistoryEvents started.");

                HistoryEvent conditionHistoryEvent = new HistoryEvent(true);
                int count = 0;
                DateTime? toDate = null;

                var inputParams = JsonConvert.DeserializeObject<Dictionary<string, object>>(ri.RequestData.postData.ToString());

                if (inputParams.ContainsKey("conditionHistoryEvent"))
                {
                    conditionHistoryEvent = GetHistoryEventFromPostData(inputParams["conditionHistoryEvent"]);
                }

                //Если пользователь - не показываем события до его регистрации
                if (ri._User.Role != User.RoleTypes.Parent)
                {
                    conditionHistoryEvent.CreationDate = ri._Account.CreationDate;
                }

                if (inputParams.ContainsKey("count"))
                {
                    Int32.TryParse(inputParams["count"].ToString(), out count);
                }
                if (count < 0 || count > Settings.Current[Settings.Name.Event.BatchLimit, CommonData.eventBatchLimit])
                {
                    count = Settings.Current[Settings.Name.Event.BatchLimit, CommonData.eventBatchLimit];
                }

                if (inputParams.ContainsKey("toDate"))
                {
                    if (DateTime.TryParse(inputParams["toDate"].ToString(), out DateTime toDateValue))
                    {
                        toDate = toDateValue;
                    }
                }

                //Получение всех сущностей, данные которых могут понадобиться для формирования текста сообщений
                var selectedEvents = DBWorker.GetEvents(ri, conditionHistoryEvent, count, toDate);

                if (selectedEvents.Count > 0)
                {
                    return FillTemplates(ri, selectedEvents);
                }
                else
                {
                    return selectedEvents;
                }
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
                logger.Trace("GetHistoryEvents leave.");
            }
        }

        private List<Dictionary<string, object>> GetTasks(FQRequestInfo ri, List<Guid> selectingTasks)
        {
            try
            {
                FQRequestInfo ri_GetTasks = ri.Clone();
                ri_GetTasks.RequestData.actionName = "GetTasks";

                ri_GetTasks.RequestData.postData = JsonConvert.SerializeObject(selectingTasks);
                var response = RouteInfo.RouteToService(ri_GetTasks, _httpContextAccessor);

                var targetTasks = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(response);

                return targetTasks;
            }
            finally
            {

            }
        }

        private List<Reward> GetRewards(FQRequestInfo ri, List<Guid> selectingRewards)
        {
            try
            {
                FQRequestInfo ri_GetReward = ri.Clone();
                ri_GetReward.RequestData.actionName = "GetRewardsById";
                
                ri_GetReward.RequestData.postData = JsonConvert.SerializeObject(selectingRewards);
                var response = RouteInfo.RouteToService(ri_GetReward, _httpContextAccessor);

                var targetRewards = JsonConvert.DeserializeObject<List<Reward>>(response);

                return targetRewards;
            }
            finally
            {

            }
        }

        private List<User> GetUsers(FQRequestInfo ri, List<Guid> selectingUsers)
        {
            try
            {
                FQRequestInfo ri_GetUser = ri.Clone();
                ri_GetUser.RequestData.actionName = "GetUsersById";
                
                ri_GetUser.RequestData.postData = JsonConvert.SerializeObject(selectingUsers);
                var response = RouteInfo.RouteToService(ri_GetUser, _httpContextAccessor);

                var targetUsers = JsonConvert.DeserializeObject<List<User>>(response);

                return targetUsers;
            }
            finally
            {

            }
        }

        private List<User> GetAllUsers(FQRequestInfo ri)
        {
            try
            {
                FQRequestInfo ri_GetAllUsers = ri.Clone();
                ri_GetAllUsers.RequestData.actionName = "GetAllUsers";
                ri_GetAllUsers.RequestData.postData = string.Empty;

                var response = RouteInfo.RouteToService(ri_GetAllUsers, _httpContextAccessor);

                var targetUsers = JsonConvert.DeserializeObject<List<User>>(response);

                return targetUsers;
            }
            finally
            {

            }
        }



        private void GetTemplates()
        {
            try
            {
                MessagTypeText_Parent = new Dictionary<HistoryEvent.MessageTypeEnum, Dictionary<string, string>>();
                MessagTypeText_Children = new Dictionary<HistoryEvent.MessageTypeEnum, Dictionary<string, string>>();

                Dictionary<string, string> Msg = new Dictionary<string, string>();

                #region Task

                //Task_Created
                Msg.Add("Title", "Создан черновик задания");
                Msg.Add("Body", "<b><color=#418D86>{0}</color></b> cоздал новый черновик задания <b><color=#D9A100>{1}</color></b>.");
                MessagTypeText_Parent.Add(HistoryEvent.MessageTypeEnum.Task_Created, Msg); //+


                //Task_Removed
                Msg = new Dictionary<string, string>();
                Msg.Add("Title", "Удален черновик задания");
                Msg.Add("Body", "<b><color=#418D86>{0}</color></b> удалил черновик задания <b><color=#D9A100>{1}</color></b>.");
                MessagTypeText_Parent.Add(HistoryEvent.MessageTypeEnum.Task_Removed, Msg);//+


                //Task_Published
                Msg = new Dictionary<string, string>();
                Msg.Add("Title", "Объявлено задание");
                Msg.Add("Body", "<b><color=#418D86>{0}</color></b> объявил задание <b><color=#D9A100>{1}</color></b>.");
                MessagTypeText_Parent.Add(HistoryEvent.MessageTypeEnum.Task_Published, Msg); //+

                Msg = new Dictionary<string, string>();
                Msg.Add("Title", "Доступно новое задание");
                Msg.Add("Body", "Доступно новое задание <b><color=#D9A100>{0}</color></b>.");
                MessagTypeText_Children.Add(HistoryEvent.MessageTypeEnum.Task_Published, Msg); //+


                //Task_Canceled_Available
                Msg = new Dictionary<string, string>();
                Msg.Add("Title", "Задание отменено");
                Msg.Add("Body", "<b><color=#418D86>{0}</color></b> отменил задание <b><color=#D9A100>{1}</color></b>.");
                MessagTypeText_Parent.Add(HistoryEvent.MessageTypeEnum.Task_Canceled_Available, Msg); //+

                Msg = new Dictionary<string, string>();
                Msg.Add("Title", "Задание отменено");
                Msg.Add("Body", "Задание <b><color=#D9A100>{0}</color></b> более не доступно.");
                MessagTypeText_Children.Add(HistoryEvent.MessageTypeEnum.Task_Canceled_Available, Msg); //+


                //Task_Canceled_InProgress
                Msg = new Dictionary<string, string>();
                Msg.Add("Title", "Выполнение отменено");
                Msg.Add("Body", "<b><color=#418D86>{0}</color></b> отменил выполняемое задание <b><color=#D9A100>{1}</color></b>. Герою начислено вознаградение в размере <b><color=#D9A100>{2}</color></b>.");
                MessagTypeText_Parent.Add(HistoryEvent.MessageTypeEnum.Task_Canceled_InProgress, Msg); //+

                Msg = new Dictionary<string, string>();
                Msg.Add("Title", "Выполнение отменено");
                Msg.Add("Body", "<b><color=#418D86>{0}</color></b> отменил выполняемое задание <b><color=#D9A100>{1}</color></b>. Получено вознаградение в размере <b><color=#D9A100>{2}</color></b>.");
                MessagTypeText_Children.Add(HistoryEvent.MessageTypeEnum.Task_Canceled_InProgress, Msg); //+

                //Task_Canceled_Related
                Msg = new Dictionary<string, string>();
                Msg.Add("Title", "Задание удалено");
                Msg.Add("Body", "Задание <b><color=#D9A100>{0}</color></b> удалено: герой-исполнитель был удален.");
                MessagTypeText_Parent.Add(HistoryEvent.MessageTypeEnum.Task_Canceled_Related, Msg); //+

                Msg = new Dictionary<string, string>();
                Msg.Add("Title", "Задание удалено");
                Msg.Add("Body", "Задание <b><color=#D9A100>{0}</color></b> удалено: герой-исполнитель был удален.");
                MessagTypeText_Children.Add(HistoryEvent.MessageTypeEnum.Task_Canceled_Related, Msg); //+


                //Task_ChangedStatus_InProgress
                Msg = new Dictionary<string, string>();
                Msg.Add("Title", "Началось выполнение задания");
                Msg.Add("Body", "<b><color=#418D86>{0}</color></b> начал выполнение задания <b><color=#D9A100>{1}</color></b>.");
                MessagTypeText_Parent.Add(HistoryEvent.MessageTypeEnum.Task_ChangedStatus_InProgress, Msg); //+

                Msg = new Dictionary<string, string>();
                Msg.Add("Title", "Началось выполнение задания");
                Msg.Add("Body", "Началось выполнение задания <b><color=#D9A100>{0}</color></b>.");
                MessagTypeText_Children.Add(HistoryEvent.MessageTypeEnum.Task_ChangedStatus_InProgress, Msg); //+


                //Task_ChangedStatus_Verification
                Msg = new Dictionary<string, string>();
                Msg.Add("Title", "Задание ожидает проверки");
                Msg.Add("Body", "<b><color=#418D86>{0}</color></b> выполнил задание <b><color=#D9A100>{1}</color></b>. Требуется проверка результата.");
                MessagTypeText_Parent.Add(HistoryEvent.MessageTypeEnum.Task_ChangedStatus_Verification, Msg); //+

                Msg = new Dictionary<string, string>();
                Msg.Add("Title", "Задание ожидает проверки");
                Msg.Add("Body", "Задание <b><color=#D9A100>{0}</color></b> выполнено. Ожидается проверка результата.");
                MessagTypeText_Children.Add(HistoryEvent.MessageTypeEnum.Task_ChangedStatus_Verification, Msg); //+


                //Task_ChangedStatus_Declined
                Msg = new Dictionary<string, string>();
                Msg.Add("Title", "Задание отклонено");
                Msg.Add("Body", "<b><color=#418D86>{0}</color></b> отклонил выполняемое задание <b><color=#D9A100>{1}</color></b> и выплатил штраф в размере <b><color=#D9A100>{2}</color></b>.");
                MessagTypeText_Parent.Add(HistoryEvent.MessageTypeEnum.Task_ChangedStatus_Declined, Msg); //+

                Msg = new Dictionary<string, string>();
                Msg.Add("Title", "Задание отклонено");
                Msg.Add("Body", "Задание <b><color=#D9A100>{0}</color></b> отклонено. Произведена оплата штраф в размере <b><color=#D9A100>{1}</color></b>.");
                MessagTypeText_Children.Add(HistoryEvent.MessageTypeEnum.Task_ChangedStatus_Declined, Msg); //+


                //Task_ChangedStatus_Redo
                Msg = new Dictionary<string, string>();
                Msg.Add("Title", "Задание требует доработки");
                Msg.Add("Body", "<b><color=#418D86>{0}</color></b> проверил результат выполнения задания <b><color=#D9A100>{1}</color></b>. Задание требует доработки.");
                MessagTypeText_Parent.Add(HistoryEvent.MessageTypeEnum.Task_ChangedStatus_Redo, Msg); //+

                Msg = new Dictionary<string, string>();
                Msg.Add("Title", "Задание требует доработки");
                Msg.Add("Body", "<b><color=#418D86>{0}</color></b> проверил результат выполнения задания <b><color=#D9A100>{1}</color></b>. Задание требует доработки.");
                MessagTypeText_Children.Add(HistoryEvent.MessageTypeEnum.Task_ChangedStatus_Redo, Msg); //+


                //Task_ChangedStatus_Completed
                Msg = new Dictionary<string, string>();
                Msg.Add("Title", "Задание выполнено успешно");
                Msg.Add("Body", "<b><color=#418D86>{0}</color></b> подтвердил успешное выполнение задания <b><color=#D9A100>{1}</color></b>.  Герою начислено вознаградение в размере <b><color=#D9A100>{2}</color></b>.");
                MessagTypeText_Parent.Add(HistoryEvent.MessageTypeEnum.Task_ChangedStatus_Completed, Msg); //+

                Msg = new Dictionary<string, string>();
                Msg.Add("Title", "Задание выполнено успешно");
                Msg.Add("Body", "<b><color=#418D86>{0}</color></b> подтвердил успешное выполнение задания <b><color=#D9A100>{1}</color></b>. Получено вознаградение в размере <b><color=#D9A100>{2}</color></b>.");
                MessagTypeText_Children.Add(HistoryEvent.MessageTypeEnum.Task_ChangedStatus_Completed, Msg); //+


                //Task_ChangedStatus_Failed
                Msg = new Dictionary<string, string>();
                Msg.Add("Title", "Задание провалено");
                Msg.Add("Body", "<b><color=#418D86>{0}</color></b> проверил выполнение задания <b><color=#D9A100>{1}</color></b>. <b><color=#418D86>{2}</color></b> провалил выполнение. Герой оплатил штраф в размере <b><color=#D9A100>{3}</color></b>.");
                MessagTypeText_Parent.Add(HistoryEvent.MessageTypeEnum.Task_ChangedStatus_Failed, Msg); //+

                Msg = new Dictionary<string, string>();
                Msg.Add("Title", "Задание провалено");
                Msg.Add("Body", "Задание <b><color=#D9A100>{0}</color></b> провалено. Произведена оплата штрафа в размере <b><color=#D9A100>{1}</color></b>.");
                MessagTypeText_Children.Add(HistoryEvent.MessageTypeEnum.Task_ChangedStatus_Failed, Msg); //+


                //Task_ChangedStatus_SolutionTimeOver
                Msg = new Dictionary<string, string>();
                Msg.Add("Title", "Задание провалено");
                Msg.Add("Body", "<b><color=#418D86>{0}</color></b> провалил задание <b><color=#D9A100>{1}</color></b>: истекло время выполнения. Герой оплатил штраф в размере <b><color=#D9A100>{2}</color></b>.");
                MessagTypeText_Parent.Add(HistoryEvent.MessageTypeEnum.Task_ChangedStatus_SolutionTimeOver, Msg); //+

                Msg = new Dictionary<string, string>();
                Msg.Add("Title", "Задание провалено");
                Msg.Add("Body", "Задание <b><color=#D9A100>{0}</color></b> провалено: истекло время выполнения. Произведена оплата штрафа в размере <b><color=#D9A100>{1}</color></b>.");
                MessagTypeText_Children.Add(HistoryEvent.MessageTypeEnum.Task_ChangedStatus_SolutionTimeOver, Msg); //+


                //Task_ChangedStatus_AvailableUntilPassed
                Msg = new Dictionary<string, string>();
                Msg.Add("Title", "Задание истекло");
                Msg.Add("Body", "Герои не откликнулись: задание <b><color=#D9A100>{0}</color></b> истекло.");
                MessagTypeText_Parent.Add(HistoryEvent.MessageTypeEnum.Task_ChangedStatus_AvailableUntilPassed, Msg); //+

                Msg = new Dictionary<string, string>();
                Msg.Add("Title", "Задание истекло");
                Msg.Add("Body", "Истекло задание <b><color=#D9A100>{0}</color></b>.");
                MessagTypeText_Children.Add(HistoryEvent.MessageTypeEnum.Task_ChangedStatus_AvailableUntilPassed, Msg); //+
                #endregion

                #region Reward

                //Reward_Created
                Msg = new Dictionary<string, string>();
                Msg.Add("Title", "Новое сокровище");
                Msg.Add("Body", "<b><color=#418D86>{0}</color></b> объявил сокровище <b><color=#798E12>{1}</color></b> стоимостью <b><color=#798E12>{2}</color></b> для <b><color=#418D86>{3}</color></b>.");
                MessagTypeText_Parent.Add(HistoryEvent.MessageTypeEnum.Reward_Created, Msg); //+

                Msg = new Dictionary<string, string>();
                Msg.Add("Title", "Новое сокровище");
                Msg.Add("Body", "Доступно новое сокровище <b><color=#798E12>{0}</color></b> стоимостью <b><color=#798E12>{1}</color></b>.");
                MessagTypeText_Children.Add(HistoryEvent.MessageTypeEnum.Reward_Created, Msg); //+


                //Reward_Removed
                Msg = new Dictionary<string, string>();
                Msg.Add("Title", "Сокровище удалено");
                Msg.Add("Body", "<b><color=#418D86>{0}</color></b> удалил сокровище <b><color=#798E12>{1}</color></b>.");
                MessagTypeText_Parent.Add(HistoryEvent.MessageTypeEnum.Reward_Removed, Msg); //+

                Msg = new Dictionary<string, string>();
                Msg.Add("Title", "Сокровище удалено");
                Msg.Add("Body", "Сокровище <b><color=#798E12>{0}</color></b> удалено.");
                MessagTypeText_Children.Add(HistoryEvent.MessageTypeEnum.Reward_Removed, Msg); //+


                //Reward_Purchased
                Msg = new Dictionary<string, string>();
                Msg.Add("Title", "Сокровище добыто");
                Msg.Add("Body", "<b><color=#418D86>{0}</color></b> ожидает получение сокровища <b><color=#798E12>{1}</color></b>.");
                MessagTypeText_Parent.Add(HistoryEvent.MessageTypeEnum.Reward_Purchased, Msg); //+

                Msg = new Dictionary<string, string>();
                Msg.Add("Title", "Сокровище добыто");
                Msg.Add("Body", "Сокровище <b><color=#798E12>{0}</color></b> добыто. Ожидание вручения.");
                MessagTypeText_Children.Add(HistoryEvent.MessageTypeEnum.Reward_Purchased, Msg); //+


                //Reward_Handed
                Msg = new Dictionary<string, string>();
                Msg.Add("Title", "Сокровище вручено");
                Msg.Add("Body", "<b><color=#418D86>{0}</color></b> подтвердил вручение <b><color=#418D86>{1}</color></b> сокровища <b><color=#798E12>{2}</color></b>.");
                MessagTypeText_Parent.Add(HistoryEvent.MessageTypeEnum.Reward_Handed, Msg); //+

                Msg = new Dictionary<string, string>();
                Msg.Add("Title", "Сокровище получено");
                Msg.Add("Body", "Сокровище <b><color=#798E12>{0}</color></b> получено.");
                MessagTypeText_Children.Add(HistoryEvent.MessageTypeEnum.Reward_Handed, Msg); //+
                #endregion

                #region User

                //User_Created
                Msg = new Dictionary<string, string>();
                Msg.Add("Title", "Новый человек в Королевстве!");
                Msg.Add("Body", "<b><color=#418D86>{0}</color></b> добавил пользователя <b><color=#418D86>{1}</color></b>.");
                MessagTypeText_Parent.Add(HistoryEvent.MessageTypeEnum.User_Created, Msg);

                Msg = new Dictionary<string, string>();
                Msg.Add("Title", "Добро пожаловать!");
                Msg.Add("Body", "Добро пожаловать в королевство FamilialQuest, Герой! Выполняй <b><color=#D9A100>Задания</color></b>, получай вознаграждения и добывай все заветные <b><color=#798E12>Сокровища</color></b>.");
                MessagTypeText_Children.Add(HistoryEvent.MessageTypeEnum.User_Created, Msg);


                //User_PasswordChanged
                Msg = new Dictionary<string, string>();
                Msg.Add("Title", "Изменен пароль пользователя");
                Msg.Add("Body", "Изменен пароль для входа в аккаунт.");
                MessagTypeText_Parent.Add(HistoryEvent.MessageTypeEnum.User_PasswordChanged, Msg);

                Msg = new Dictionary<string, string>();
                Msg.Add("Title", "Изменен пароль пользователя");
                Msg.Add("Body", "Изменен пароль для входа в аккаунт.");
                MessagTypeText_Children.Add(HistoryEvent.MessageTypeEnum.User_PasswordChanged, Msg);


                //User_Removed
                Msg = new Dictionary<string, string>();
                Msg.Add("Title", "Пользователь покинул Королевство");
                Msg.Add("Body", "<b><color=#418D86>{0}</color></b> удалил пользователя <b><color=#418D86>{1}</color></b>.");
                MessagTypeText_Parent.Add(HistoryEvent.MessageTypeEnum.User_Removed, Msg);
                #endregion

                #region Group
                //Group_Created
                Msg = new Dictionary<string, string>();
                Msg.Add("Title", "Добро пожаловать!");
                Msg.Add("Body", "Добавьте в свое королевство FamilialQuest представителей <b><color=#418D86>Королевского двора</color></b>, и объявляйте грандиозные <b><color=#D9A100>Задания</color></b> и несметные <b><color=#798E12>Сокровища</color></b> для отважных <b><color=#418D86>Героев</color></b>!");
                MessagTypeText_Parent.Add(HistoryEvent.MessageTypeEnum.Group_Created, Msg); //+

                //Group_PremiumPurchased1M
                //Msg = new Dictionary<string, string>();
                //Msg.Add("Title", "Приобретен Премиум-доступ");
                //Msg.Add("Body", "Премиум-доступ приобретен на период <b>1 месяц</b>.");
                //MessagTypeText_Parent.Add(HistoryEvent.MessageTypeEnum.Group_PremiumPurchased1M, Msg); //+

                ////Group_PremiumPurchased1M
                //Msg = new Dictionary<string, string>();
                //Msg.Add("Title", "Приобретен Премиум-доступ");
                //Msg.Add("Body", "Премиум-доступ приобретен на период <b>3 месяца</b>.");
                //MessagTypeText_Parent.Add(HistoryEvent.MessageTypeEnum.Group_PremiumPurchased3M, Msg); //+

                ////Group_PremiumPurchased1M
                //Msg = new Dictionary<string, string>();
                //Msg.Add("Title", "Приобретен Премиум-доступ");
                //Msg.Add("Body", "Премиум-доступ приобретен на период <b>1 год</b>.");
                //MessagTypeText_Parent.Add(HistoryEvent.MessageTypeEnum.Group_PremiumPurchased12M, Msg); //+

                #endregion
            }
            finally
            {

            }
        }

        private List<HistoryEvent> FillTemplates (FQRequestInfo ri, List<HistoryEvent> selectedEvents)
        {
            List<HistoryEvent> selectedEvents_filled = new List<HistoryEvent>(selectedEvents);

            var selectedEvents_Doers = new List<Guid>(selectedEvents_filled.Select(x => x.Doer).Distinct());
            selectedEvents_Doers.Remove(Guid.Empty);

            var targetTasks_Users_Executor = new List<Guid>();
            var targetRewards_AvailableFor = new List<Guid>();

            List<Guid> targetTasksIds = new List<Guid>(selectedEvents_filled.Where(x => x.ItemType == HistoryEvent.ItemTypeEnum.Task).Select(param => param.TargetItem).Distinct());
            targetTasksIds.Remove(Guid.Empty);
            if (targetTasksIds.Count > 0)
            {
                targetTasks = GetTasks(ri, targetTasksIds);
                targetTasks_Users_Executor = new List<Guid>(targetTasks.Select(x => Guid.Parse(x["Executor"].ToString())));
            }


            List<Guid> targetRewardsIds = new List<Guid>(selectedEvents_filled.Where(x => x.ItemType == HistoryEvent.ItemTypeEnum.Reward).Select(param => param.TargetItem).Distinct());
            targetRewardsIds.Remove(Guid.Empty);
            if (targetRewardsIds.Count > 0)
            {
                targetRewards = GetRewards(ri, targetRewardsIds);
                targetRewards_AvailableFor = new List<Guid>(targetRewards.Select(x => x.availableFor).Distinct());
            }

            List<Guid> targetUsersIds = new List<Guid>(selectedEvents_filled.Where(x => x.ItemType == HistoryEvent.ItemTypeEnum.User).Select(param => param.TargetItem).Distinct());
            targetUsersIds.AddRange(targetTasks_Users_Executor);
            targetUsersIds.AddRange(targetRewards_AvailableFor);
            targetUsersIds.AddRange(selectedEvents_Doers);

            targetUsersIds = new List<Guid>(targetUsersIds.Distinct());
            targetUsersIds.Remove(Guid.Empty);
            if (targetUsersIds.Count > 0)
            {
                targetUsers = GetUsers(ri, targetUsersIds);
            }

            GetTemplates();

            foreach (var selectedEvent in selectedEvents_filled)
            {
                Dictionary<string, string> message = new Dictionary<string, string>();
                
                try
                {
                    switch (selectedEvent.ItemType)
                    {
                        case HistoryEvent.ItemTypeEnum.Task:
                            {
                                message = FillTemplate_TaskEvent(ri, selectedEvent);
                                break;
                            }
                        case HistoryEvent.ItemTypeEnum.Reward:
                            {
                                message = FillTemplate_RewardEvent(ri, selectedEvent);
                                break;
                            }
                        case HistoryEvent.ItemTypeEnum.User:
                            {
                                message = FillTemplate_UserEvent(ri, selectedEvent);
                                break;
                            }
                        case HistoryEvent.ItemTypeEnum.Group:
                            {
                                message = FillTemplate_GroupEvent(ri, selectedEvent);
                                break;
                            }
                    }

                    selectedEvent.EventTitle = message["Title"];
                    selectedEvent.EventText = message["Body"];
                    selectedEvent.TargetItemName = message["ItemName"];
                    selectedEvent.CreationDateMinAgo = Convert.ToInt32((DateTime.UtcNow - selectedEvent.CreationDate).TotalMinutes);
                }
                catch
                {
                    selectedEvent.EventTitle = "<Неизвестно>";
                    selectedEvent.EventText = "<Неизвестно>";
                    selectedEvent.TargetItemName = "<Неизвестно>";
                }
            }

            return selectedEvents_filled;
        }

        private Dictionary<string, string> FillTemplate_TaskEvent(FQRequestInfo ri, HistoryEvent historyEvent)
        {
            try
            {
                Dictionary<string, string> msgTitleAndBody = new Dictionary<string, string>();
                               
                Dictionary<string, object> targetTask = targetTasks.Where(x => Guid.Parse(x["Id"].ToString()) == historyEvent.TargetItem).FirstOrDefault();
                if (targetTask != null)
                {
                    User doer = targetUsers.Where(x => x.Id == historyEvent.Doer).FirstOrDefault();
                    if (doer == null)
                    {
                        doer = new User();
                        doer.Name = "<Неизвестно>";
                    }

                    User taskExecutor = targetUsers.Where(x => x.Id == Guid.Parse(targetTask["Executor"].ToString())).FirstOrDefault();
                    if (taskExecutor == null)
                    {
                        taskExecutor = new User();
                        taskExecutor.Name = "<Неизвестно>";
                    }

                    switch (ri._User.Role)
                    {
                        case User.RoleTypes.Children:
                            {
                                if (MessagTypeText_Children.TryGetValue(historyEvent.MessageType, out msgTitleAndBody))
                                {
                                    msgTitleAndBody = new Dictionary<string, string>(msgTitleAndBody); //чтобы если TryGetValue в out msgTitleAndBody вернул null, здесь был инициализирован пустой список

                                    switch (historyEvent.MessageType)
                                    {
                                        #region Task
                                        case HistoryEvent.MessageTypeEnum.Task_Published:
                                        case HistoryEvent.MessageTypeEnum.Task_Canceled_Available:
                                        case HistoryEvent.MessageTypeEnum.Task_ChangedStatus_Verification:
                                        case HistoryEvent.MessageTypeEnum.Task_ChangedStatus_AvailableUntilPassed:
                                        case HistoryEvent.MessageTypeEnum.Task_Canceled_Related:
                                            {
                                                msgTitleAndBody["Body"] = string.Format(msgTitleAndBody["Body"], targetTask["Name"].ToString());
                                                break;
                                            }
                                        case HistoryEvent.MessageTypeEnum.Task_ChangedStatus_InProgress:
                                            {
                                                //Если показываем сообщение для Executor-а, просто показываем
                                                if (taskExecutor.Id == ri._User.Id)
                                                {
                                                    msgTitleAndBody["Body"] = string.Format(msgTitleAndBody["Body"], targetTask["Name"].ToString());
                                                }
                                                else
                                                {
                                                    //А вот тут костыль - для пользаков из AvailableFor нужно показать сообщение (что задача уплыла), как для родителя
                                                    if (MessagTypeText_Parent.TryGetValue(HistoryEvent.MessageTypeEnum.Task_ChangedStatus_InProgress, out msgTitleAndBody))
                                                    {
                                                        msgTitleAndBody = new Dictionary<string, string>(msgTitleAndBody); //чтобы если TryGetValue в out msgTitleAndBody вернул null, здесь был инициализирован пустой список

                                                        msgTitleAndBody["Body"] = string.Format(msgTitleAndBody["Body"], taskExecutor.Name, targetTask["Name"].ToString());
                                                    }
                                                }

                                                break;
                                            }
                                        case HistoryEvent.MessageTypeEnum.Task_Canceled_InProgress:
                                        case HistoryEvent.MessageTypeEnum.Task_ChangedStatus_Completed:
                                            {
                                                msgTitleAndBody["Body"] = string.Format(msgTitleAndBody["Body"], doer.Name, targetTask["Name"].ToString(), targetTask["Cost"].ToString());
                                                break;
                                            }
                                        case HistoryEvent.MessageTypeEnum.Task_ChangedStatus_Declined:
                                        case HistoryEvent.MessageTypeEnum.Task_ChangedStatus_Failed:
                                        case HistoryEvent.MessageTypeEnum.Task_ChangedStatus_SolutionTimeOver:
                                            {
                                                msgTitleAndBody["Body"] = string.Format(msgTitleAndBody["Body"], targetTask["Name"].ToString(), targetTask["Penalty"].ToString());
                                                break;
                                            }
                                        case HistoryEvent.MessageTypeEnum.Task_ChangedStatus_Redo:
                                            {
                                                msgTitleAndBody["Body"] = string.Format(msgTitleAndBody["Body"], doer.Name, targetTask["Name"].ToString());
                                                break;
                                            }
                                            #endregion
                                    }
                                }
                                else
                                {
                                    msgTitleAndBody = new Dictionary<string, string>();
                                    msgTitleAndBody.Add("Title", "<Неизвестно>");
                                    msgTitleAndBody.Add("Body", "<Неизвестно>");
                                }

                                break;
                            }
                        case User.RoleTypes.Parent:
                            {
                                if (MessagTypeText_Parent.TryGetValue(historyEvent.MessageType, out msgTitleAndBody))
                                {
                                    msgTitleAndBody = new Dictionary<string, string>(msgTitleAndBody);

                                    switch (historyEvent.MessageType)
                                    {
                                        #region Task
                                        case HistoryEvent.MessageTypeEnum.Task_Created:
                                        case HistoryEvent.MessageTypeEnum.Task_Published:
                                        case HistoryEvent.MessageTypeEnum.Task_Canceled_Available:
                                            {
                                                msgTitleAndBody["Body"] = string.Format(msgTitleAndBody["Body"], doer.Name, targetTask["Name"].ToString());
                                                break;
                                            }
                                        case HistoryEvent.MessageTypeEnum.Task_Removed:
                                        case HistoryEvent.MessageTypeEnum.Task_ChangedStatus_Redo:
                                            {
                                                msgTitleAndBody["Body"] = string.Format(msgTitleAndBody["Body"], doer.Name, targetTask["Name"].ToString());
                                                break;
                                            }
                                        case HistoryEvent.MessageTypeEnum.Task_Canceled_InProgress:
                                        case HistoryEvent.MessageTypeEnum.Task_ChangedStatus_Completed:
                                            {
                                                msgTitleAndBody["Body"] = string.Format(msgTitleAndBody["Body"], doer.Name, targetTask["Name"].ToString(), targetTask["Cost"].ToString());
                                                break;
                                            }

                                        case HistoryEvent.MessageTypeEnum.Task_ChangedStatus_Failed:
                                            {
                                                msgTitleAndBody["Body"] = string.Format(msgTitleAndBody["Body"], doer.Name, targetTask["Name"].ToString(), taskExecutor.Name, targetTask["Penalty"].ToString());
                                                break;
                                            }

                                        case HistoryEvent.MessageTypeEnum.Task_ChangedStatus_InProgress:
                                        case HistoryEvent.MessageTypeEnum.Task_ChangedStatus_Verification:
                                            {
                                                msgTitleAndBody["Body"] = string.Format(msgTitleAndBody["Body"], doer.Name, targetTask["Name"].ToString());
                                                break;
                                            }
                                        case HistoryEvent.MessageTypeEnum.Task_ChangedStatus_AvailableUntilPassed:
                                        case HistoryEvent.MessageTypeEnum.Task_Canceled_Related:
                                            {
                                                msgTitleAndBody["Body"] = string.Format(msgTitleAndBody["Body"], targetTask["Name"].ToString());
                                                break;
                                            }
                                        case HistoryEvent.MessageTypeEnum.Task_ChangedStatus_Declined:
                                            {
                                                msgTitleAndBody["Body"] = string.Format(msgTitleAndBody["Body"], doer.Name, targetTask["Name"].ToString(), targetTask["Penalty"].ToString());
                                                break;
                                            }
                                        case HistoryEvent.MessageTypeEnum.Task_ChangedStatus_SolutionTimeOver:
                                            {
                                                msgTitleAndBody["Body"] = string.Format(msgTitleAndBody["Body"], taskExecutor.Name, targetTask["Name"].ToString(), targetTask["Penalty"].ToString());
                                                break;
                                            }
                                            #endregion
                                    }
                                }
                                else
                                {
                                    msgTitleAndBody = new Dictionary<string, string>();
                                    msgTitleAndBody.Add("Title", "<Неизвестно>");
                                    msgTitleAndBody.Add("Body", "<Неизвестно>");
                                }

                                break;
                            }
                    }

                    msgTitleAndBody.Add("ItemName", targetTask["Name"].ToString());
                }
                else
                {
                    msgTitleAndBody.Add("Title", "<Неизвестно>");
                    msgTitleAndBody.Add("Body", "<Неизвестно>");
                    msgTitleAndBody.Add("ItemName", "<Неизвестно>");
                }

                return msgTitleAndBody;
            }
            finally
            {

            }
        }

        private Dictionary<string, string> FillTemplate_RewardEvent(FQRequestInfo ri, HistoryEvent historyEvent)
        {
            try
            {
                Dictionary<string, string> msgTitleAndBody = new Dictionary<string, string>();                

                Reward targetReward = targetRewards.Where(x => x.id == historyEvent.TargetItem).FirstOrDefault();
                if (targetReward != null)
                {
                    User doer = targetUsers.Where(x => x.Id == historyEvent.Doer).FirstOrDefault();
                    if (doer == null)
                    {
                        doer = new User();
                        doer.Name = "<Неизвестно>";
                    }

                    User targetReward_AvailableFor = targetUsers.Where(x => x.Id == targetReward.availableFor).FirstOrDefault();
                    if (targetReward_AvailableFor == null)
                    {
                        targetReward_AvailableFor = new User();
                        targetReward_AvailableFor.Name = "<Неизвестно>";
                    }

                    switch (ri._User.Role)
                    {
                        case User.RoleTypes.Children:
                            {
                                if (MessagTypeText_Children.TryGetValue(historyEvent.MessageType, out msgTitleAndBody))
                                {
                                    msgTitleAndBody = new Dictionary<string, string>(msgTitleAndBody); //чтобы если TryGetValue в out msgTitleAndBody вернул null, здесь был инициализирован пустой список

                                    switch (historyEvent.MessageType)
                                    {
                                        #region Reward
                                        case HistoryEvent.MessageTypeEnum.Reward_Created:
                                            {
                                                msgTitleAndBody["Body"] = string.Format(msgTitleAndBody["Body"], targetReward.Title, targetReward.Cost);
                                                break;
                                            }

                                        case HistoryEvent.MessageTypeEnum.Reward_Removed:
                                        case HistoryEvent.MessageTypeEnum.Reward_Purchased:
                                        case HistoryEvent.MessageTypeEnum.Reward_Handed:
                                            {
                                                msgTitleAndBody["Body"] = string.Format(msgTitleAndBody["Body"], targetReward.Title);
                                                break;
                                            }
                                            #endregion
                                    }
                                }
                                else
                                {
                                    msgTitleAndBody = new Dictionary<string, string>();
                                    msgTitleAndBody.Add("Title", "<Неизвестно>");
                                    msgTitleAndBody.Add("Body", "<Неизвестно>");
                                }

                                break;
                            }
                        case User.RoleTypes.Parent:
                            {
                                if (MessagTypeText_Parent.TryGetValue(historyEvent.MessageType, out msgTitleAndBody))
                                {
                                    msgTitleAndBody = new Dictionary<string, string>(msgTitleAndBody);

                                    switch (historyEvent.MessageType)
                                    {
                                        #region Reward
                                        case HistoryEvent.MessageTypeEnum.Reward_Created:
                                            {
                                                msgTitleAndBody["Body"] = string.Format(msgTitleAndBody["Body"], doer.Name, targetReward.Title, targetReward.Cost, targetReward_AvailableFor.Name);
                                                break;
                                            }

                                        case HistoryEvent.MessageTypeEnum.Reward_Removed:
                                            {
                                                msgTitleAndBody["Body"] = string.Format(msgTitleAndBody["Body"], doer.Name, targetReward.Title);
                                                break;
                                            }

                                        case HistoryEvent.MessageTypeEnum.Reward_Purchased:
                                            {
                                                msgTitleAndBody["Body"] = string.Format(msgTitleAndBody["Body"], targetReward_AvailableFor.Name, targetReward.Title);
                                                break;
                                            }

                                        case HistoryEvent.MessageTypeEnum.Reward_Handed:
                                            {
                                                msgTitleAndBody["Body"] = string.Format(msgTitleAndBody["Body"], doer.Name, targetReward_AvailableFor.Name, targetReward.Title);
                                                break;
                                            }
                                            #endregion
                                    }

                                }
                                else
                                {
                                    msgTitleAndBody = new Dictionary<string, string>();
                                    msgTitleAndBody.Add("Title", "<Неизвестно>");
                                    msgTitleAndBody.Add("Body", "<Неизвестно>");
                                }

                                break;
                            }
                    }

                    msgTitleAndBody.Add("ItemName", targetReward.Title);
                }
                else
                {
                    msgTitleAndBody.Add("Title", "<Неизвестно>");
                    msgTitleAndBody.Add("Body", "<Неизвестно>");
                    msgTitleAndBody.Add("ItemName", "<Неизвестно>");
                }

                return msgTitleAndBody;
            }
            finally
            {

            }
        }

        private Dictionary<string, string> FillTemplate_UserEvent(FQRequestInfo ri, HistoryEvent historyEvent)
        {
            try
            {
                Dictionary<string, string> msgTitleAndBody = new Dictionary<string, string>();
                
                User targetUser = targetUsers.Where(x => x.Id == historyEvent.TargetItem).FirstOrDefault();
                if (targetUser != null)
                {
                    User doer = targetUsers.Where(x => x.Id == historyEvent.Doer).FirstOrDefault();
                    if (doer == null)
                    {
                        doer = new User();
                        doer.Name = "<Неизвестно>";
                    }

                    switch (ri._User.Role)
                    {
                        case User.RoleTypes.Children:
                            {
                                if (MessagTypeText_Children.TryGetValue(historyEvent.MessageType, out msgTitleAndBody))
                                {
                                    msgTitleAndBody = new Dictionary<string, string>(msgTitleAndBody); //чтобы если TryGetValue в out msgTitleAndBody вернул null, здесь был инициализирован пустой список

                                    switch (historyEvent.MessageType)
                                    {
                                        #region User
                                        case HistoryEvent.MessageTypeEnum.User_Removed:
                                            {
                                                msgTitleAndBody["Body"] = string.Format(msgTitleAndBody["Body"], doer.Name, targetUser.Name);
                                                break;
                                            }
                                            #endregion
                                    }
                                }
                                else
                                {
                                    msgTitleAndBody = new Dictionary<string, string>();
                                    msgTitleAndBody.Add("Title", "<Неизвестно>");
                                    msgTitleAndBody.Add("Body", "<Неизвестно>");
                                }

                                break;
                            }
                        case User.RoleTypes.Parent:
                            {
                                if (MessagTypeText_Parent.TryGetValue(historyEvent.MessageType, out msgTitleAndBody))
                                {
                                    msgTitleAndBody = new Dictionary<string, string>(msgTitleAndBody);

                                    switch (historyEvent.MessageType)
                                    {
                                        #region User
                                        case HistoryEvent.MessageTypeEnum.User_Created:
                                        case HistoryEvent.MessageTypeEnum.User_Removed:
                                            {
                                                msgTitleAndBody["Body"] = string.Format(msgTitleAndBody["Body"], doer.Name, targetUser.Name);
                                                break;
                                            }
                                            #endregion
                                    }

                                }
                                else
                                {
                                    msgTitleAndBody = new Dictionary<string, string>();
                                    msgTitleAndBody.Add("Title", "<Неизвестно>");
                                    msgTitleAndBody.Add("Body", "<Неизвестно>");
                                }

                                break;
                            }
                    }

                    msgTitleAndBody.Add("ItemName", targetUser.Name); //TODO: мб + Title?
                }
                else
                {
                    msgTitleAndBody.Add("Title", "<Неизвестно>");
                    msgTitleAndBody.Add("Body", "<Неизвестно>");
                    msgTitleAndBody.Add("ItemName", "<Неизвестно>");
                }

                return msgTitleAndBody;
            }
            finally
            {

            }
        }

        private Dictionary<string, string> FillTemplate_GroupEvent(FQRequestInfo ri, HistoryEvent historyEvent)
        {
            try
            {
                Dictionary<string, string> msgTitleAndBody = new Dictionary<string, string>();

                switch (ri._User.Role)
                {
                    case User.RoleTypes.Parent:
                        {
                            if (MessagTypeText_Parent.TryGetValue(historyEvent.MessageType, out msgTitleAndBody))
                            {
                                //Не параметризованный текст, достаточно дефолтного
                                msgTitleAndBody = new Dictionary<string, string>(msgTitleAndBody);
                            }
                            else
                            {
                                msgTitleAndBody = new Dictionary<string, string>();
                                msgTitleAndBody.Add("Title", "<Неизвестно>");
                                msgTitleAndBody.Add("Body", "<Неизвестно>");
                            }

                            break;
                        }
                }

                msgTitleAndBody.Add("ItemName", "FamilialQuest");

                return msgTitleAndBody;
            }
            finally
            {

            }
        }
    }
}
