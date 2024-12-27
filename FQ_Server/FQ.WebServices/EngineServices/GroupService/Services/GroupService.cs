using CommonLib;
using CommonRoutes;
using CommonTypes;
using Google.Apis.AndroidPublisher.v3;
using Google.Apis.AndroidPublisher.v3.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using GroupService.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static CommonLib.FQServiceException;
using static CommonTypes.Subscription;
using static CommonTypes.User;

namespace GroupService.Services
{
    /// <summary>
    /// Сервис управления группами
    /// </summary>
    public class GroupServices : IGroupServices
    {
        static NLog.Logger logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        private readonly IHttpContextAccessor _httpContextAccessor;

        //TODO: теоретически, коннект будет держаться и проверка статуса и переинициализация не требуются
        private static AndroidPublisherService service = null;

        private Timer timerManagePurchases = null;

        private void TimerCallback_ManageSubscriptions(object e)
        {
            try
            {
                ManageSubscriptions();
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
        public GroupServices(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            InitializeDevAPI();

            timerManagePurchases = new Timer(
                this.TimerCallback_ManageSubscriptions,
                null,
                CommonLib.Settings.Current[Settings.Name.Group.ManageSubscriptionsDelay, CommonData.manageSubscriptionsDelay] * 1000,
                CommonLib.Settings.Current[Settings.Name.Group.ManageSubscriptionsDelay, CommonData.manageSubscriptionsDelay] * 1000
            );
        }

        /// <summary>
        ///  Дессериализация Group 
        /// </summary>
        /// <param name="inputParams">Json Group</param>
        /// <returns></returns>
        private Group GetGroupFromPostData(object inputParams)
        {
            try
            {
                logger.Trace("GetGroupFromPostData started.");

                Group inputGroup = new Group(true);
                inputGroup = JsonConvert.DeserializeObject<Group>(inputParams.ToString());

                logger.Trace($"id: {inputGroup.Id.ToString()}");
                logger.Trace($"name: {inputGroup.Name}");
                logger.Trace($"Image: {inputGroup.Image}");

                return inputGroup;
            }
            finally
            {
                logger.Trace("GetGroupFromPostData leave.");
            }
        }

        /// <summary>
        /// Создание группы
        /// </summary>
        /// <returns></returns>
        public Guid CreateGroup()
        {
            try
            {
                logger.Trace("CreateGroup started.");

                Group inputGroup = new Group(true);
                inputGroup.Id = Guid.NewGuid();

                DBWorker.CreateGroup(inputGroup);

                return inputGroup.Id;
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
                logger.Trace("CreateGroup leave.");
            }
        }

        /// <summary>
        /// Обновление группы
        /// </summary>
        /// <param name="ri"></param>
        public void UpdateGroup(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("UpdateGroup started.");

                Group inputGroup = GetGroupFromPostData(ri.RequestData.postData);

                //При формировании клиентом запроса следует учитывать регистрозависимость имен параметров (далее)!
                Dictionary<string, string> inputParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(ri.RequestData.postData.ToString());

                logger.Trace($"userId: {ri._Account.userId.ToString()}");

                //Проверка статуса полльзователя
                if (ri._User.Role != RoleTypes.Parent)
                {
                    throw new FQServiceException(FQServiceExceptionType.NotEnoughRights);
                }

                var selectedGroup = DBWorker.GetGroup(ri._User.GroupId);

                if (inputParams.ContainsKey("name"))
                {
                    selectedGroup.Name = inputGroup.Name;
                }

                if (inputParams.ContainsKey("image"))
                {
                    selectedGroup.Image = inputGroup.Image;
                }

                DBWorker.UpdateGroup(selectedGroup);
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
                logger.Trace("UpdateGroup leave.");
            }
        }

        /// <summary>
        /// Обновление информации о группе
        /// </summary>
        /// <param name="ri"></param>
        public void RemoveGroup(FQRequestInfo ri)
        {
            try
            {
                var groupId = Guid.Parse(ri.RequestData.postData.ToString());//JsonConvert.DeserializeObject<Guid>(ri.RequestData.postData.ToString());

                if (groupId == Guid.Empty)
                {
                    throw new Exception("Ошика: groupId == Guid.Empty");
                }

                DBWorker.RemoveGroup(groupId);
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
                logger.Trace("RemoveGroup leave.");
            }
        }

        /// <summary>
        /// Получение информации о группе
        /// </summary>
        /// <param name="ri"></param>
        /// <returns></returns>
        public Group GetGroup(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("GetGroup started.");

                var selectedGroup = DBWorker.GetGroup(ri._User.GroupId);

                //Формирование статуса подписки
                List<Subscription> actualSubscriptions = DBWorker.GetActualSubscriptionsForGroup(ri._User.GroupId);

                //Если присутствуют актуальные подписки
                if (actualSubscriptions.Count > 0)
                {
                    //Если есть актуальные - значит есть и активная (или актуальная, которая ещё не переведена в статус активной)                    
                    selectedGroup.SubscriptionIsActive = true;
                    selectedGroup.SubscriptionExpiredDate = CommonTypes.Constants.POSTGRES_DATETIME_MINVALUE;

                    //Все ниже - для получения даты завершения подписки, что нужно лишь для случая запроса информации на странице подписки в аппке.
                    //Во всех иных случаях достаточно свойства SubscriptionIsActive.
                    //Не критично - даже если не удастся в эту попытку её получить из-за фоновых операций, не будем выкидывать ошибку.

                    //Получение активной
                    Subscription activeSubscription = actualSubscriptions.Where(x => x.State == InnerState.Acivated).FirstOrDefault();

                    //Иной треш, маловозможен - только если попали в середину фонового выполнения TimerCallback_ManagePurchases.
                    //Не критично - просто пользователь не получит SubscriptionExpiredDate и обновит страничку ещё раз.
                    if (activeSubscription != null)
                    {
                        //Подсчёт итоговой даты истечения всех актуальных
                        DateTime expiredDateTotal = DateTime.UtcNow;

                        //Для начала - для активной
                        DateTime expiredDateActive = activeSubscription.ModificationTime.AddMonths(activeSubscription.Months);

                        //Если ещё не истекла
                        if (expiredDateActive > DateTime.UtcNow)
                        {
                            //Высчитывание остатка
                            var activeSubscriptionTimeRest = (expiredDateActive - DateTime.UtcNow).TotalSeconds;

                            expiredDateTotal = expiredDateTotal.AddSeconds(activeSubscriptionTimeRest);
                        }

                        //Теперь для приобретеных (которые ещё не активны)
                        actualSubscriptions.Remove(activeSubscription);

                        int totalMonths = 0;

                        foreach (var p in actualSubscriptions)
                        {
                            totalMonths += p.Months;
                        }

                        expiredDateTotal = expiredDateTotal.AddMonths(totalMonths);   
                        
                        selectedGroup.SubscriptionExpiredDate = expiredDateTotal;                        
                    }
                }               

                return selectedGroup;
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
                logger.Trace("GetGroup leave.");
            }
        }

        private static void InitializeDevAPI()
        {
            try
            {
                logger.Trace("InitializeDevAPI started.");

                //Инициализация Google Developer API
                
                var credential = GoogleCredential.FromFile(CommonLib.Settings.Current[Settings.Name.Group.KeyFilePath, CommonData.groupKeyFilePath]).CreateScoped("https://www.googleapis.com/auth/androidpublisher");

                var initializer = new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = CommonLib.Settings.Current[Settings.Name.Group.applicationName, CommonData.applicationName]
                };

                service = new AndroidPublisherService(initializer);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw;
            }
            finally
            {
                logger.Trace("GetGroup leave.");
            }
        }

        private static string GetObfuscatedAccountIdHash(string salt, string userId)
        {
            string resultHash = string.Empty;

            byte[] saltBytes = Encoding.UTF8.GetBytes(salt.ToLower());
            byte[] passBytes = Encoding.UTF8.GetBytes(userId);
            byte[] secondSaltBytes = Encoding.UTF8.GetBytes("hsbg");

            byte[] plainTextWithSaltBytes =
                new byte[passBytes.Length + saltBytes.Length + secondSaltBytes.Length];

            // Copy plain text bytes into resulting array.
            for (int i = 0; i < passBytes.Length; i++)
                plainTextWithSaltBytes[i] = passBytes[i];

            // Append salt bytes to the resulting array.
            for (int i = 0; i < saltBytes.Length; i++)
                plainTextWithSaltBytes[passBytes.Length + i] = saltBytes[i];

            // Append salt bytes to the resulting array.
            for (int i = 0; i < secondSaltBytes.Length; i++)
                plainTextWithSaltBytes[passBytes.Length + saltBytes.Length + i] = secondSaltBytes[i];

            var hash = new System.Security.Cryptography.SHA256Managed();

            // Compute hash value of our plain text with appended salt.
            byte[] hashBytes = hash.ComputeHash(plainTextWithSaltBytes);

            resultHash = Convert.ToBase64String(hashBytes);

            return resultHash;
        }

        private static int GetMonthsCountFromProductId(string productId)
        {

            //Обязательно перед проверкой subscriptionProductId1M. Т.к. Contains
            if (productId.Contains(CommonLib.Settings.Current[Settings.Name.Group.subscriptionProductId12M, CommonData.subscriptionProductId12M]))
            {
                return 12;
            }

            if (productId.Contains(CommonLib.Settings.Current[Settings.Name.Group.subscriptionProductId3M, CommonData.subscriptionProductId3M]))
            {
                return 3;
            }

            if (productId.Contains(CommonLib.Settings.Current[Settings.Name.Group.subscriptionProductId1M, CommonData.subscriptionProductId1M]))
            {
                return 1;
            }
            throw new Exception("GetMonthsCountFromProductId: неизвестный productId");
        }

        public void ProcessPurchase(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("ProcessPurchase started.");
                logger.Trace($"GroupId: {ri._User.GroupId.ToString()}");

                var postData = JsonConvert.DeserializeObject<Dictionary<string, string>>(ri.RequestData.postData.ToString());

                logger.Trace($"purchaseToken: {postData["purchaseToken"]}");
                logger.Trace($"productId: {postData["productId"]}");             
                
                if (string.IsNullOrEmpty(postData["purchaseToken"]) ||
                    string.IsNullOrEmpty(postData["productId"]))
                {
                    throw new Exception("Empty system fields");
                }

                //Проверка статуса полльзователя
                if (ri._User.Role != RoleTypes.Parent)
                {
                    throw new FQServiceException(FQServiceExceptionType.NotEnoughRights);
                }

                int months = 0;

                //Запрос инфы покупки по токену 
                var purchaseGetRequest = service.Purchases.Products.Get(CommonLib.Settings.Current[Settings.Name.Group.packageName, CommonData.packageName], postData["productId"], postData["purchaseToken"]);
                var purchaseGetResponse = purchaseGetRequest.Execute();

                logger.Trace($"ObfuscatedExternalAccountId: {purchaseGetResponse.ObfuscatedExternalAccountId}");

                //Проверка статуса покупки
                if (purchaseGetResponse.PurchaseState == 0)
                {
                    //Проверка дублирования
                    if (string.IsNullOrEmpty(purchaseGetResponse.ObfuscatedExternalAccountId) ||    //в случае промо-кода информация отсутстует -  далее выполнится внутренняя проверка, избежим дублирования
                        purchaseGetResponse.AcknowledgementState == null ||
                        purchaseGetResponse.AcknowledgementState == 0)
                    {
                        //Проверка идентификатора пользователя (текущего и оплатившего покупку)
                        var obfuscatedAccountIdHash = GetObfuscatedAccountIdHash("fqpurchasesalt", ri._User.Id.ToString());

                        logger.Trace($"obfuscatedAccountIdHash: {obfuscatedAccountIdHash}");

                        if (string.IsNullOrEmpty(purchaseGetResponse.ObfuscatedExternalAccountId) ||    //в случае промо-кода информация отсутстует - на предъявителя
                            purchaseGetResponse.ObfuscatedExternalAccountId == obfuscatedAccountIdHash)
                        {
                            //Проверка: не дубляж ли?
                            //Это возможно в случае, если в предыдущую итерацию был облом с подтверждением покупки ProductPurchasesAcknowledgeRequest,
                            //а затем и в приложении (независимо) не отработало подтверждение обработки (например,в случае, если отвалилась сеть и ответ не был получен).
                            //
                            ////GetActualSubscriptionsForGroup подойдет для этой цели, 
                            ////т.к. у нас в хранилище не может располагаться отмененная покупка, не будучи таковой при проверке выше purchaseGetResponse.PurchaseState == 0
                            //List<Subscription> actualSubscriptions = DBWorker.GetActualSubscriptionsForGroup(ri._User.GroupId);
                            //Subscription existedEqualsSubscription = actualSubscriptions.Where(x => x.PurchaseToken == postData["purchaseToken"]).FirstOrDefault();

                            var purchaseExistingCount = DBWorker.GetThisSubscriptionCountForGroup(ri._User.GroupId, postData["purchaseToken"]);


                            if (purchaseExistingCount == 0)
                            {
                                months = GetMonthsCountFromProductId(postData["productId"]);

                                //в случае промо-кода информация отсутстует
                                if (string.IsNullOrEmpty(purchaseGetResponse.OrderId))
                                {
                                    purchaseGetResponse.OrderId = string.Empty;
                                }

                                //Сохранение покупки
                                DBWorker.SaveSubscription(ri._User.GroupId, postData["purchaseToken"], purchaseGetResponse.OrderId, months);

                                //Подтверждение покупки
                                try
                                {
                                    var purchaseAcknowledgeRequestBody = new Google.Apis.AndroidPublisher.v3.Data.ProductPurchasesAcknowledgeRequest();
                                    var purchaseAcknowledgeRequest = service.Purchases.Products.Acknowledge(purchaseAcknowledgeRequestBody, CommonLib.Settings.Current[Settings.Name.Group.packageName, CommonData.packageName], postData["productId"], postData["purchaseToken"]);
                                    var purchaseAcknowledgeResponse = purchaseAcknowledgeRequest.Execute();

                                    if (purchaseAcknowledgeResponse != String.Empty)
                                    {
                                        throw new Exception();
                                    }
                                }
                                catch
                                {
                                    //Не будем кидать ошибку, т.к. это не критично.
                                    //Подтверждение выполнится на стороне клиента: или после получения ответа на этот запрос, 
                                    //или повторно получим этот запрос после релога, и будет выкинут PurchaseIsAlreadyExists.
                                    //В противном случае (при долговременном отсутствии подтверждения) покупка будет возвращена Google-ом,
                                    //что учтётся в ManageSunscription.

                                    //throw new Exception("purchaseAcknowledgeRequest error");
                                }
                            }
                            else
                            {
                                throw new FQServiceException(FQServiceExceptionType.PurchaseIsAlreadyExists);
                            }                            
                        }
                        else
                        {
                            throw new Exception("ObfuscatedExternalAccountId Not Equals");
                        } 
                    }
                    else
                    {
                        if (purchaseGetResponse.AcknowledgementState == 1)
                        {
                            throw new FQServiceException(FQServiceExceptionType.AcknowledgementStateIsAcknowledged);
                        }
                        else
                        {
                            throw new Exception("AcknowledgementState is not correct");
                        }                        
                    }
                }
                else
                {
                    if (purchaseGetResponse.PurchaseState == 1)
                    {
                        throw new FQServiceException(FQServiceExceptionType.PurchaseStateIsCanceled);
                    }
                    else
                    {
                        //TODO: purchaseGetResponse.PurchaseState == 2 - отложенные покупки пока не поддерживаем
                        throw new Exception("PurchaseState is not correct");
                    }
                }

                //TODO: для фиксации событий покупки на будущее
                //if (months != 0)
                //{
                //    //Запись HistoryEvent-а
                //    try
                //    {
                //        HistoryEvent.MessageTypeEnum msgType = HistoryEvent.MessageTypeEnum.Default;

                //        switch (months)
                //        {
                //            case 1:
                //                {
                //                    msgType = HistoryEvent.MessageTypeEnum.Group_PremiumPurchased1M;
                //                    break;
                //                }
                //            case 3:
                //                {
                //                    msgType = HistoryEvent.MessageTypeEnum.Group_PremiumPurchased3M;
                //                    break;
                //                }
                //            case 12:
                //                {
                //                    msgType = HistoryEvent.MessageTypeEnum.Group_PremiumPurchased12M;
                //                    break;
                //                }
                //        }

                //        if (msgType != HistoryEvent.MessageTypeEnum.Default)
                //        {
                //            FQRequestInfo ri_CreateHistoryEvent = ri.Clone();

                //            List<Guid> availableFor = new List<Guid>();

                //            HistoryEvent he = new HistoryEvent(HistoryEvent.ItemTypeEnum.Group, msgType, HistoryEvent.VisabilityEnum.Parents, ri._User.GroupId, availableFor, ri._User.Id);

                //            ri_CreateHistoryEvent.RequestData.actionName = "CreateHistoryEvent";
                //            ri_CreateHistoryEvent.RequestData.postData = JsonConvert.SerializeObject(he);

                //            RouteInfo.RouteToService(ri_CreateHistoryEvent, _httpContextAccessor);
                //        }
                //    }
                //    catch (Exception)
                //    {
                //        //Если по какой-то причине не удалось записать HistoryEvent - не повод  прерывать операцию и кидать пользователю Error.
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
                logger.Trace("ProcessPurchase leave.");
            }
        }

        private void ManageSubscriptions()
        {
            try
            {
                //TODO: убрать лог
                logger.Trace("ManageSubscriptions started.");

                //Получение из БД всех приобретенных или активных
                List<Subscription> actualSubscriptions = DBWorker.GetAllActualSubscriptions();

                logger.Trace($"actualSubscriptions: {actualSubscriptions.Count}");

                #region Обнуление покупок
                //Запрос всех отозваных покупок
                List<VoidedPurchase> voidedPurchases = new List<VoidedPurchase>();

                var voidedProductsRequest = service.Purchases.Voidedpurchases.List(CommonLib.Settings.Current[Settings.Name.Group.packageName, CommonData.packageName]);
                var voidedProductResponse = voidedProductsRequest.Execute();

                if (voidedProductResponse != null && 
                    voidedProductResponse.VoidedPurchases != null)
                {
                    voidedPurchases.AddRange(voidedProductResponse.VoidedPurchases);
                }

                while (voidedProductResponse.TokenPagination != null &&
                       !String.IsNullOrEmpty(voidedProductResponse.TokenPagination.NextPageToken))
                {
                    voidedProductsRequest.Token = voidedProductResponse.TokenPagination.NextPageToken;
                    voidedProductResponse = voidedProductsRequest.Execute();

                    if (voidedProductResponse != null &&
                    voidedProductResponse.VoidedPurchases != null)
                    {
                        voidedPurchases.AddRange(voidedProductResponse.VoidedPurchases);
                    }
                }

                logger.Trace($"voidedPurchases: {voidedPurchases.Count}");

                //Обнуление покупок, о которых сообщил DEV API и которые ещё не обнулены в БД
                List<string> toVoidSubscriptions = new List<string>(voidedPurchases.Where(x => actualSubscriptions.Select(y => y.PurchaseToken).Contains(x.PurchaseToken)).Select(x => x.PurchaseToken));

                logger.Trace($"toVoidSubscriptions: {toVoidSubscriptions.Count}");

                foreach (var purchaseToken in toVoidSubscriptions)
                {
                    try
                    {
                        DBWorker.UpdateSubscriptionState(purchaseToken, InnerState.Voided);
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }

                //Удаление из списка актуальных всех обнуленных
                actualSubscriptions = new List<Subscription>(actualSubscriptions.Where(x => !toVoidSubscriptions.Contains(x.PurchaseToken)));

                logger.Trace($"actualSubscriptions: {actualSubscriptions.Count}");
                #endregion

                #region Управление статусами актуальных покупок

                //Получение списка групп, у которых в данный момент имеются актуальные покупки
                List<Guid> groups = new List<Guid>(actualSubscriptions.Select(x => x.GroupId).Distinct());

                logger.Trace($"groups: {groups.Count}");

                foreach (var group in groups)
                {
                    //Получение списка актуальных покупок для группы
                    List<Subscription> groupSubscriptions = new List<Subscription>(actualSubscriptions.Where(x => x.GroupId == group));

                    logger.Trace($"groupPurchases: {groupSubscriptions.Count}");

                    //Получение активной покупки
                    Subscription activeSubscription = groupSubscriptions.Where(x => x.State == InnerState.Acivated).FirstOrDefault();

                    if (activeSubscription != null)
                    {
                        logger.Trace($"activeSubscription: {activeSubscription.PurchaseToken}");
                        logger.Trace($"activeSubscription.ModificationTime: {activeSubscription.ModificationTime}");


                        DateTime expiredDate = activeSubscription.ModificationTime.AddMonths(activeSubscription.Months);

                        logger.Trace($"expiredDate: {expiredDate}");

                        //Если срок действия покупки истек
                        if (expiredDate <= DateTime.UtcNow)
                        {
                            logger.Trace("Expired");
                            DBWorker.UpdateSubscriptionState(activeSubscription.PurchaseToken, InnerState.Expired);
                        }
                        else
                        {
                            logger.Trace("Not expired");
                            continue;
                        }
                    }

                    logger.Trace($"activeSubscription: null or expired");

                    //Если активной не было или истекла - поиск покупки, которая приобретена раньше остальных
                    Subscription subscriptionToActivate = groupSubscriptions.Where(x => x.State == InnerState.Purchased).OrderBy(x => x.ModificationTime).FirstOrDefault();

                    //Если таковая имеется - установка статуса Acivated
                    if (subscriptionToActivate != null)
                    {
                        logger.Trace($"purchaseToActivate: {subscriptionToActivate.PurchaseToken}");

                        DBWorker.UpdateSubscriptionState(subscriptionToActivate.PurchaseToken, InnerState.Acivated);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                logger.Trace("ManageSubscriptions leave.");
            }

            #endregion
        }
    }
}
