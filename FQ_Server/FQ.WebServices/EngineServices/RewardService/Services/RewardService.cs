using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using CommonLib;
using CommonRoutes;
using CommonTypes;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using RewardService.Models;
using static CommonLib.FQServiceException;
using static CommonTypes.Reward;
using static CommonTypes.User;

namespace RewardService.Services
{
    /// <summary>
    /// Сервис управления группами
    /// </summary>
    public class RewardServices : IRewardServices
    {
        static NLog.Logger logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Default constructor with HTTPContext
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        public RewardServices(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        
        /// <summary>
        /// Дессериализация Reward
        /// </summary>
        /// <param name="inputParams"></param>
        /// <returns></returns>
        public Reward GetRewardFromPostData(object inputParams)
        {
            try
            {
                logger.Trace("GetRewardFromPostData started.");

                Reward inputReward = new Reward(true);
                inputReward = JsonConvert.DeserializeObject<Reward>(inputParams.ToString());

                logger.Trace($"id: {inputReward.id.ToString()}");
                logger.Trace($"groupId: {inputReward.groupId.ToString()}");
                logger.Trace($"title: {inputReward.Title}");
                logger.Trace($"description: {inputReward.Description}");
                logger.Trace($"cost: {inputReward.Cost.ToString()}");
                logger.Trace($"Image: {inputReward.Image}");
                logger.Trace($"creator: {inputReward.creator}");
                logger.Trace($"availableFor: {inputReward.availableFor}");
                logger.Trace($"Status: {inputReward.Status.ToString()}");
                logger.Trace($"creationDate: {inputReward.CreationDate.ToString()}");
                logger.Trace($"purchaseDate: {inputReward.PurchaseDate.ToString()}");
                logger.Trace($"handedDate: {inputReward.HandedDate.ToString()}");

                return inputReward;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }
            finally
            {
                logger.Trace("GetRewardFromPostData leave.");
            }
        }


        /// <summary>
        /// Создание новой награды
        /// </summary>
        /// <param name="ri"></param>
        /// <param name="inputReward"></param>
        /// <param name="availableFor"></param>
        /// <returns></returns>
        public Guid AddReward(FQRequestInfo ri, Reward inputReward, List<Guid> availableFor, bool isStartingItem = false)
        {
            try
            {
                logger.Trace("AddReward started.");

                logger.Trace($"userId: {ri._Account.userId.ToString()}");
                logger.Trace($"id: {inputReward.id.ToString()}");
                logger.Trace($"groupId: {inputReward.groupId.ToString()}");
                logger.Trace($"title: {inputReward.Title}");
                logger.Trace($"description: {inputReward.Description}");
                logger.Trace($"cost: {inputReward.Cost.ToString()}");
                logger.Trace($"Image: {inputReward.Image}");
                logger.Trace($"creator: {inputReward.creator}");
                logger.Trace($"availableFor: {inputReward.availableFor}");
                logger.Trace($"Status: {inputReward.Status.ToString()}");
                logger.Trace($"creationDate: {inputReward.CreationDate.ToString()}");
                logger.Trace($"purchaseDate: {inputReward.PurchaseDate.ToString()}");
                logger.Trace($"handedDate: {inputReward.HandedDate.ToString()}");

                foreach (var destUserId in availableFor)
                {
                    logger.Trace($"destUserId: {destUserId}");
                }

                if (string.IsNullOrEmpty(inputReward.Title) || 
                    inputReward.Cost == null || 
                    inputReward.Cost.Value == 0)
                {
                    throw new FQServiceException(FQServiceExceptionType.EmptyRequiredField);
                }

                if (inputReward.Cost < 1)
                {
                    throw new FQServiceException(FQServiceExceptionType.DefaultError);
                }

                //Проверка статуса пользователя
                if (ri._User.Role != RoleTypes.Parent)
                {
                    throw new FQServiceException(FQServiceExceptionType.NotEnoughRights);
                }

                //Получение всех пользователей группы
                FQRequestInfo riToCredential = ri.Clone();
                riToCredential.RequestData.actionName = "GetUsersById";
                riToCredential.RequestData.postData = JsonConvert.SerializeObject(availableFor);

                //Если запрос выполнен успешно, значит все пользователи валидны и существуют в группе.
                var response = RouteInfo.RouteToService(riToCredential, _httpContextAccessor);

                //Теперь необходимо проверить, что назначаем задачу только детям.
                var destUsers = JsonConvert.DeserializeObject<List<User>>(response);
                if (destUsers.Where(x => x.Role != RoleTypes.Children).Count() > 0)
                {                    
                    throw new Exception("Ошибка: награда может быть назначена только детям.");
                }

                //Проверка превышения лимита
                var availableRewardsCount = DBWorker.GetAvailableRewardsCount(ri._User.GroupId);

                if (ri._Group.SubscriptionIsActive)
                {
                    if (availableRewardsCount >= CommonLib.Settings.Current[Settings.Name.Reward.maxRewards_Extension, CommonData.maxRewards_Extension])
                    {
                        throw new FQServiceException(FQServiceExceptionType.LimitAchieved);
                    }
                }
                else
                {
                    if (availableRewardsCount >= CommonLib.Settings.Current[Settings.Name.Reward.maxRewards_NotExtension, CommonData.maxRewards_NotExtension])
                    {
                        throw new FQServiceException(FQServiceExceptionType.LimitAchieved);
                    }
                }

                var allRewardsCount = DBWorker.GetRewardsCount(ri._User.GroupId);
                if (availableRewardsCount >= CommonLib.Settings.Current[Settings.Name.Reward.maxRewards_Total, CommonData.maxRewards_Total])
                {
                    throw new FQServiceException(FQServiceExceptionType.TotalItemsLimitAchieved);
                }

                foreach (var destUser in destUsers)
                {
                    inputReward.id = Guid.NewGuid();
                    inputReward.groupId = ri._User.GroupId;
                    inputReward.creator = ri._Account.userId;
                    inputReward.availableFor = destUser.Id;
                    inputReward.Status = RewardStatuses.Available;
                    inputReward.CreationDate = DateTime.UtcNow;
                    inputReward.PurchaseDate = CommonData.dateTime_FQDB_MinValue;
                    inputReward.HandedDate = CommonData.dateTime_FQDB_MinValue;

                    DBWorker.AddReward(inputReward);

                    if (!isStartingItem)
                    {
                        //Запись HistoryEvent-а
                        try
                        {
                            FQRequestInfo ri_CreateHistoryEvent = ri.Clone();

                            List<Guid> event_availableFor = new List<Guid>();
                            event_availableFor.Add(inputReward.availableFor);

                            HistoryEvent he = new HistoryEvent(HistoryEvent.ItemTypeEnum.Reward, HistoryEvent.MessageTypeEnum.Reward_Created, HistoryEvent.VisabilityEnum.Children, inputReward.id, event_availableFor, ri._User.Id);

                            ri_CreateHistoryEvent.RequestData.actionName = "CreateHistoryEvent";
                            ri_CreateHistoryEvent.RequestData.postData = JsonConvert.SerializeObject(he);

                            RouteInfo.RouteToService(ri_CreateHistoryEvent, _httpContextAccessor);
                        }
                        catch (Exception)
                        {
                            //Если по какой-то причине не улаось записать HistoryEvent - не повод  прерывать операцию и кидать пользователю Error.
                        }
                    }
                }

                //Рудимент, но пока оставлю
                return inputReward.id;
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
                logger.Trace("AddReward leave.");
            }
        }

        //TODO: зарезервировано
        ///// <summary>
        ///// Обновление параметров существующей награды
        ///// </summary>
        ///// <param name="uc"></param>
        ///// <param name="inputReward"></param>
        ///// <returns></returns>
        //public void UpdateReward(FQRequestInfo ri)
        //{
        //    try
        //    {
        //        logger.Trace("UpdateGroup started.");

        //        logger.Trace("GetRewardFromPostData.");
        //        Reward inputReward = GetRewardFromPostData(ri.RequestData.postData);

        //        logger.Trace("Get updated params names.");
        //        //При формировании клиентом запроса следует учитывать регистрозависимость имен параметров (далее)!
        //        Dictionary<string, string> inputParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(ri.RequestData.postData.ToString());

        //        logger.Trace("Проверка входных параметров.");

        //        logger.Trace($"userId: {ri._Account.ToString()}");
        //        logger.Trace($"id: {inputReward.id.ToString()}");                

        //        if (ri._Account.userId == Guid.Empty || inputReward.id == Guid.Empty)
        //        {
        //            throw new Exception("uc.userId == Guid.Empty || inputReward.id == Guid.Empty");
        //        }                

        //        logger.Trace("Формирование системных параметров.");
        //        List<Guid> rewardsId = new List<Guid>();
        //        rewardsId.Add(inputReward.id);
        //        var selectedReward = DBWorker.GetRewardsById(ri._User.GroupId, rewardsId).First();

        //        if (selectedReward == null)
        //        {
        //            throw new Exception("Ошибка: selectedReward == null.");
        //        }

        //        if (inputParams.ContainsKey("title"))
        //        {
        //            if (!string.IsNullOrEmpty(inputReward.Title))
        //            {
        //                selectedReward.Title = inputReward.Title;
        //            }
        //        }

        //        if (inputParams.ContainsKey("description"))
        //        {
        //            selectedReward.Description = inputReward.Description;
        //        }

        //        if (inputParams.ContainsKey("cost"))
        //        {
        //            selectedReward.Cost = inputReward.Cost;
        //        }

        //        if (inputParams.ContainsKey("image"))
        //        {
        //            selectedReward.Image = inputReward.Image;
        //        }

        //        if (inputParams.ContainsKey("status"))
        //        {
        //            if (inputReward.Status != null && (inputReward.Status == 0 || inputReward.Status == 1 || inputReward.Status == 2))
        //            {
        //                selectedReward.Status = inputReward.Status;
        //            }
        //        }

        //        if (inputParams.ContainsKey("creationDate"))
        //        {
        //            selectedReward.creationDate = inputReward.creationDate;
        //        }

        //        if (inputParams.ContainsKey("purchaseDate"))
        //        {
        //            selectedReward.purchaseDate = inputReward.purchaseDate;
        //        }

        //        if (inputParams.ContainsKey("handedDate"))
        //        {
        //            selectedReward.handedDate = inputReward.handedDate;
        //        }

        //        logger.Trace("UpdateReward.");
        //        DBWorker.UpdateReward(selectedReward);
        //    }
        //    catch (FQServiceException)
        //    {
        //        throw;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex);
        //        throw new Exception(CommonData.defaultError);
        //    }
        //    finally
        //    {
        //        logger.Trace("UpdateGroup leave.");
        //    }
        //}

        /// <summary>
        /// Приобретение награды
        /// </summary>
        /// <param name="ri"></param>
        public void PurchaseReward(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("PurchaseReward started.");

                Guid inputRewardId = JsonConvert.DeserializeObject<Guid>(ri.RequestData.postData.ToString());

                logger.Trace($"id: {inputRewardId}");

                if (inputRewardId == Guid.Empty)
                {
                    throw new Exception("Ошибка: inputRewardId == Guid.Empty.");
                }                
                
                //Получение награды
                List<Guid> rewardsId = new List<Guid>();
                rewardsId.Add(inputRewardId);
                var selectedReward = DBWorker.GetRewardsById(ri._User.GroupId, rewardsId).FirstOrDefault();
                if (selectedReward == null || selectedReward.Status == RewardStatuses.Deleted)
                {
                    throw new FQServiceException(FQServiceExceptionType.ItemNotFound);
                }

                //Проверка доступности награды для пользователя.");
                if (selectedReward.availableFor != ri._Account.userId)
                {
                    throw new Exception("Ошибка: награда недоступна.");
                }

                //Проверка статуса награды
                if (selectedReward.Status != RewardStatuses.Available)
                {
                    throw new FQServiceException(FQServiceExceptionType.UnsupportedStatusChanging);
                }

                //Списание стоимости награды
                FQRequestInfo riToCredential = ri.Clone();
                riToCredential.RequestData.actionName = "WriteOffCost";
                riToCredential.RequestData.postData = JsonConvert.SerializeObject(selectedReward.Cost);
                RouteInfo.RouteToService(riToCredential, _httpContextAccessor);

                //Изменение статуса награды
                selectedReward.Status = RewardStatuses.Purchased;
                selectedReward.PurchaseDate = DateTime.UtcNow;
                DBWorker.UpdateReward(selectedReward);

                //Запись HistoryEvent-а
                try
                {
                    FQRequestInfo ri_CreateHistoryEvent = ri.Clone();

                    List<Guid> availableFor = new List<Guid>();
                    availableFor.Add(selectedReward.availableFor);

                    HistoryEvent he = new HistoryEvent(HistoryEvent.ItemTypeEnum.Reward, HistoryEvent.MessageTypeEnum.Reward_Purchased, HistoryEvent.VisabilityEnum.Children, selectedReward.id, availableFor, ri._User.Id);

                    ri_CreateHistoryEvent.RequestData.actionName = "CreateHistoryEvent";
                    ri_CreateHistoryEvent.RequestData.postData = JsonConvert.SerializeObject(he);

                    RouteInfo.RouteToService(ri_CreateHistoryEvent, _httpContextAccessor);
                }
                catch (Exception)
                {
                    //Если по какой-то причине не улаось записать HistoryEvent - не повод  прерывать операцию и кидать пользователю Error.
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
                logger.Trace("PurchaseReward leave.");
            }
        }

        /// <summary>
        /// Подтверждение вручения награды
        /// </summary>
        /// <param name="ri"></param>
        public void GiveReward(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("GiveReward started.");

                Guid inputRewardId = JsonConvert.DeserializeObject<Guid>(ri.RequestData.postData.ToString());

                logger.Trace($"id: {inputRewardId}");

                if (inputRewardId == Guid.Empty)
                {
                    throw new Exception("Ошибка: inputRewardId == Guid.Empty.");
                }

                //Получение награды
                List<Guid> rewardsId = new List<Guid>();
                rewardsId.Add(inputRewardId);
                var selectedReward = DBWorker.GetRewardsById(ri._User.GroupId, rewardsId).FirstOrDefault();
                if (selectedReward == null || selectedReward.Status == RewardStatuses.Deleted)
                {
                    throw new FQServiceException(FQServiceExceptionType.ItemNotFound);
                }
                
                //Проверка статуса награды
                if (selectedReward.Status != RewardStatuses.Purchased)
                {
                    throw new FQServiceException(FQServiceExceptionType.UnsupportedStatusChanging);
                }

                //Изменение статуса награды
                selectedReward.Status = RewardStatuses.Handed;
                selectedReward.HandedDate = DateTime.UtcNow;
                DBWorker.UpdateReward(selectedReward);

                //Запись HistoryEvent-а
                try
                {
                    FQRequestInfo ri_CreateHistoryEvent = ri.Clone();

                    List<Guid> availableFor = new List<Guid>();
                    availableFor.Add(selectedReward.availableFor);

                    HistoryEvent he = new HistoryEvent(HistoryEvent.ItemTypeEnum.Reward, HistoryEvent.MessageTypeEnum.Reward_Handed, HistoryEvent.VisabilityEnum.Children, selectedReward.id, availableFor, ri._User.Id);
                    
                    ri_CreateHistoryEvent.RequestData.actionName = "CreateHistoryEvent";
                    ri_CreateHistoryEvent.RequestData.postData = JsonConvert.SerializeObject(he);

                    RouteInfo.RouteToService(ri_CreateHistoryEvent, _httpContextAccessor);
                }
                catch (Exception)
                {
                    //Если по какой-то причине не улаось записать HistoryEvent - не повод  прерывать операцию и кидать пользователю Error.
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
                logger.Trace("GiveReward leave.");
            }
        }

        /// <summary>
        /// Удаление награды
        /// </summary>
        /// <param name="ri"></param>
        /// <param name="inputReward"></param>
        public void RemoveReward(FQRequestInfo ri, Reward inputReward)
        {
            try
            {
                logger.Trace("RemoveReward started.");

                logger.Trace($"inputReward.id: {inputReward.id}");

                if (inputReward.id == Guid.Empty)
                {
                    throw new Exception("Ошибка: inputReward.id == Guid.Empty.");
                }

                //Проверка статуса пользователя
                if (ri._User.Role != RoleTypes.Parent)
                {
                    throw new FQServiceException(FQServiceExceptionType.NotEnoughRights);
                }

                //Получим награду, чтобы проверить её статус, т.к. можно удалить только объявленную 
                //(не приобретенную и не полученную) награду
                List<Guid> rewardsId = new List<Guid>();
                rewardsId.Add(inputReward.id);
                var selectedReward = DBWorker.GetRewardsById(ri._User.GroupId, rewardsId).FirstOrDefault();
                if (selectedReward == null || selectedReward.Status == RewardStatuses.Deleted)
                {
                    throw new FQServiceException(FQServiceExceptionType.ItemNotFound);
                }

                //Проверка статуса награды: Purchased - промежуточный статус, не позволим удалять.
                if (selectedReward.Status == RewardStatuses.Purchased)
                {
                    throw new FQServiceException(FQServiceExceptionType.UnsupportedStatusChanging);
                }

                //Удаление награды
                DBWorker.RemoveReward(ri._User.GroupId, selectedReward.id);

                //Запись HistoryEvent-а
                try
                {
                    FQRequestInfo ri_CreateHistoryEvent = ri.Clone();

                    List<Guid> availableFor = new List<Guid>();
                    availableFor.Add(selectedReward.availableFor);

                    HistoryEvent he = new HistoryEvent(HistoryEvent.ItemTypeEnum.Reward, HistoryEvent.MessageTypeEnum.Reward_Removed, HistoryEvent.VisabilityEnum.Children, selectedReward.id, availableFor, ri._User.Id);

                    ri_CreateHistoryEvent.RequestData.actionName = "CreateHistoryEvent";
                    ri_CreateHistoryEvent.RequestData.postData = JsonConvert.SerializeObject(he);

                    RouteInfo.RouteToService(ri_CreateHistoryEvent, _httpContextAccessor);
                }
                catch (Exception)
                {
                    //Если по какой-то причине не улаось записать HistoryEvent - не повод  прерывать операцию и кидать пользователю Error.
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
                logger.Trace("RemoveReward leave.");
            }
        }

        /// <summary>
        /// Удаление связанных с пользователем наград
        /// </summary>
        /// <param name="ri"></param>
        public void RemoveRelatedRewards(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("RemoveReward started.");

                Guid removingUserId = JsonConvert.DeserializeObject<Guid>(ri.RequestData.postData.ToString());

                logger.Trace($"userId: {ri._Account.userId}");
                logger.Trace($"removingUserId: {removingUserId}");

                if (removingUserId == Guid.Empty)
                {
                    throw new Exception("Ошибка: removingUserId == Guid.Empty.");
                }

                //Проверка статуса пользователя
                if (ri._User.Role != RoleTypes.Parent)
                {
                    throw new FQServiceException(FQServiceExceptionType.NotEnoughRights);
                }
                
                //Удаление наград
                List<Guid> relatedRewards = DBWorker.RemoveRelatedRewards(ri._User.GroupId, removingUserId);
                
                //TODO: пока отключим чтобы не заваливать ленту юзера
                //foreach (var relatedReward in relatedRewards)
                //{
                //    //Запись HistoryEvent-а
                //    try
                //    {
                //        FQRequestInfo ri_CreateHistoryEvent = ri.Clone();

                //        List<Guid> availableFor = new List<Guid>();
                //        availableFor.Add(removingUserId);

                //        HistoryEvent he = new HistoryEvent(HistoryEvent.ItemTypeEnum.Reward, HistoryEvent.MessageTypeEnum.Reward_Removed, HistoryEvent.VisabilityEnum.Children, relatedReward, availableFor, ri._User.Id);

                //        ri_CreateHistoryEvent.RequestData.actionName = "CreateHistoryEvent";
                //        ri_CreateHistoryEvent.RequestData.postData = JsonConvert.SerializeObject(he);

                //        RouteInfo.RouteToService(ri_CreateHistoryEvent, _httpContextAccessor);
                //    }
                //    catch (Exception)
                //    {
                //        //Если по какой-то причине не улаось записать HistoryEvent - не повод  прерывать операцию и кидать пользователю Error.
                //    }
                //}                
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
                logger.Trace("RemoveReward leave.");
            }
        }

        public List<Reward> GetAllRewards(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("GetAllRewards started.");
               
                logger.Trace("Получение наград.");
                var allRewards = DBWorker.GetAllRewards(ri._User.GroupId);

                if (ri._User.Role == RoleTypes.Parent)
                {
                    return allRewards;
                }
                else
                {
                    List<Reward> userAvailableRewards = new List<Reward>(allRewards.Where(x => x.availableFor == ri._Account.userId));
                    return userAvailableRewards;
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
                logger.Trace("GetAllRewards leave.");
            }
        }

        /// <summary>
        /// Получение списка наград по идентификаторам
        /// </summary>
        /// <param name="ri"></param>
        /// <param name="requestedRewards"></param>
        /// <returns></returns>
        public List<Reward> GetRewardsById(FQRequestInfo ri, List<Guid> requestedRewards)
        {
            try
            {
                logger.Trace("GetRewardsById started.");
                                
                logger.Trace($"selectedRewards.Count: {requestedRewards.Count}");

                //Проверка, сколько валидных гуидов
                var emptyIds = requestedRewards.Where(x => x == Guid.Empty).Count();
                logger.Trace($"rewardIds: {emptyIds}");

                if (requestedRewards.Count == 0 || emptyIds > 0)
                {
                    throw new Exception("Ошибка: rewardIds.Count == 0 || emptyIds > 0");
                }

                //Получение наград
                var responsedRewards = DBWorker.GetRewardsById(ri._User.GroupId, requestedRewards);

                if (responsedRewards.Count != requestedRewards.Count)
                {
                    throw new FQServiceException(FQServiceExceptionType.ItemNotFound);
                }

                if (ri._User.Role == RoleTypes.Parent)
                {
                    return responsedRewards;
                }
                else
                {
                    List<Reward> userAvailableRewards = new List<Reward>(responsedRewards.Where(x => x.availableFor == ri._Account.userId));
                    return userAvailableRewards;
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
                logger.Trace("GetRewardsById leave.");
            }
        }
    }
}
