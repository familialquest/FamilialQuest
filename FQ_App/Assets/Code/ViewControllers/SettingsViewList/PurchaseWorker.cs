using Assets.Code.Controllers;
using Assets.Code.Models.REST.CommonTypes;
using Code.Controllers.MessageBox;
using Code.Models;
using Code.Models.REST;
using Newtonsoft.Json;
using Ricimi;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;
using static Assets.Code.Models.REST.CommonTypes.FQServiceException;

// Placing the Purchaser class in the CompleteProject namespace allows it to interact with ScoreManager, 
// one of the existing Survival Shooter scripts.
namespace Code.ViewControllers
{
    // Deriving the Purchaser class from IStoreListener enables it to receive messages from Unity Purchasing.
    public class PurchaseWorker : MonoBehaviour, IStoreListener
    {
        [HideInInspector]
        public GameObject CircleProgressBar;

        [HideInInspector]
        public Popup PopupSubscriptionDetailsController;


        //TODO: для прокидывания вызова UpdateSubscriptionInfo после покупки
        public SettingsPageController settingsPageController;

        public Dictionary<string, string> products = new Dictionary<string, string>();

        //Флаг, для проверки, откуда пришел вызов колбеков обработки покупки:
        //по инициативе пользователя, или UnityIAP напоминает о необработанных покупках
        private bool calledByUser = false;


        private static IStoreController m_StoreController;          // The Unity Purchasing system.
        IGooglePlayStoreExtensions m_StoreExtensionProvider; //private static IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.

        // Product identifiers for all products capable of being purchased: 
        // "convenience" general identifiers for use with Purchasing, and their store-specific identifier 
        // counterparts for use with and outside of Unity Purchasing. Define store-specific identifiers 
        // also on each platform's publisher dashboard (iTunes Connect, Google Play Developer Console, etc.)

        // General product identifiers for the consumable, non-consumable, and subscription products.
        // Use these handles in the code to reference which product to purchase. Also use these values 
        // when defining the Product Identifiers on the store. Except, for illustration purposes, the 
        // kProductIDSubscription - it has custom Apple and Google identifiers. We declare their store-
        // specific mapping to Unity Purchasing's AddProduct, below.

        //public static string kProductIDConsumable = "consumable";
        //public static string kProductIDNonConsumable = "nonconsumable";
        //public static string kProductIDSubscription = "subscription";
        private static string kM12Consumable = "m12";
        private static string kM3Consumable = "m3";
        private static string kM1Consumable = "m1";

        private static string kM12Consumable_promo = "m12_promo";
        private static string kM1Consumable_promo = "m1_promo";



        //// Apple App Store-specific product identifier for the subscription product.
        //private static string kProductNameAppleSubscription = "com.unity3d.subscription.new";

        // Google Play Store-specific product identifier subscription product.
        //private static string kProductNameGooglePlaySubscription = "familialquest.subscription.1";
        public static string kM12GooglePlayProduct = "familialquest.pa.12";
        public static string kM3GooglePlayProduct = "familialquest.pa.3";
        public static string kM1GooglePlayProduct = "familialquest.pa.1";

        public static string kM12GooglePlayProduct_promo = "promo_03_21_familialquest.pa.12";
        public static string kM1GooglePlayProduct_promo = "promo_03_21_familialquest.pa.1";

        private void Awake()
        {
            // If we have already connected to Purchasing ...
            if (!IsInitialized())
            {
                // Begin to configure our connection to Purchasing
                InitializePurchasing();
            }
        }

        //TODO: проверить
        private void OnDestroy()
        {
            if (m_StoreExtensionProvider != null)
            {
                //m_StoreExtensionProvider.EndConnection();
                m_StoreExtensionProvider = null;
            }
        }

        public void InitializePurchasing()
        {
            try
            {
                if (CredentialHandler.Instance.CurrentUser.Role == Models.RoleModel.RoleTypes.Administrator &&
                    Application.platform == RuntimePlatform.Android)
                {
                    Debug.Log("InitializePurchasing");

                    // If we have already connected to Purchasing ...
                    if (IsInitialized())
                    {
                        Debug.Log("IsInitialized");

                        // ... we are done here.
                        return;
                    }

                    // Create a builder, first passing in a suite of Unity provided stores.
                    var builder = ConfigurationBuilder.Instance(UnityEngine.Purchasing.StandardPurchasingModule.Instance(UnityEngine.Purchasing.AndroidStore.GooglePlay));// (StandardPurchasingModule.Instance());

                    //// Add a product to sell / restore by way of its identifier, associating the general identifier
                    //// with its store-specific identifiers.
                    //builder.AddProduct(kProductIDConsumable, ProductType.Consumable);
                    //// Continue adding the non-consumable product.
                    //builder.AddProduct(kProductIDNonConsumable, ProductType.NonConsumable);

                    // And finish adding the subscription product. Notice this uses store-specific IDs, illustrating
                    // if the Product ID was configured differently between Apple and Google stores. Also note that
                    // one uses the general kProductIDSubscription handle inside the game - the store-specific IDs 
                    // must only be referenced here. 

                    builder.AddProduct(kM12Consumable, ProductType.Consumable, new IDs(){
                        //{ kProductNameAppleSubscription, AppleAppStore.Name },
                        { kM12GooglePlayProduct, UnityEngine.Purchasing.GooglePlay.Name}
                    });
                    builder.AddProduct(kM3Consumable, ProductType.Consumable, new IDs(){
                        //{ kProductNameAppleSubscription, AppleAppStore.Name },
                        { kM3GooglePlayProduct, UnityEngine.Purchasing.GooglePlay.Name }
                    });
                    builder.AddProduct(kM1Consumable, ProductType.Consumable, new IDs(){
                        //{ kProductNameAppleSubscription, AppleAppStore.Name },
                        { kM1GooglePlayProduct, UnityEngine.Purchasing.GooglePlay.Name }
                    });

                    builder.AddProduct(kM12Consumable_promo, ProductType.Consumable, new IDs(){
                        //{ kProductNameAppleSubscription, AppleAppStore.Name },
                        { kM12GooglePlayProduct_promo, UnityEngine.Purchasing.GooglePlay.Name }
                    });
                    builder.AddProduct(kM1Consumable_promo, ProductType.Consumable, new IDs(){
                        //{ kProductNameAppleSubscription, AppleAppStore.Name },
                        { kM1GooglePlayProduct_promo, UnityEngine.Purchasing.GooglePlay.Name }
                    });

                    //builder.AddProduct(kProductIDNonConsumable, ProductType.NonConsumable, new IDs(){
                    //    //{ kProductNameAppleSubscription, AppleAppStore.Name },
                    //    { kProductNameGooglePlayProduct, Google.Play.Billing.GooglePlayStoreModule.StoreName },
                    //});

                    // Kick off the remainder of the set-up with an asynchrounous call, passing the configuration 
                    // and this class' instance. Expect a response either in OnInitialized or OnInitializeFailed.
                    UnityPurchasing.Initialize(this, builder);
                }
            }
            catch (Exception ex)
            {
                Debug.Log(String.Format("InitializePurchasing Error: {0}", ex.Message));
            }
        }


        private bool IsInitialized()
        {
            // Only say we are initialized if both the Purchasing references are set.
            return m_StoreController != null && m_StoreExtensionProvider != null;
        }

        public string GetPriceForProduct(string kGooglePlayProductID, out decimal price, out string curencyString)
        {
            try
            {
                if (IsInitialized())
                {
                    curencyString = string.Empty;

                    var priceStringSource = JsonConvert.DeserializeObject<Dictionary<string, object>>(products[kGooglePlayProductID])["price_amount_micros"].ToString();
                    var curency = JsonConvert.DeserializeObject<Dictionary<string, object>>(products[kGooglePlayProductID])["price_currency_code"].ToString();

                    switch (curency)
                    {
                        case "RUB":
                            {
                                curencyString = "руб.";
                                break;
                            }
                        case "USD":
                        case "USN":
                            {
                                curencyString = "$";
                                break;
                            }
                        default:
                            {
                                curencyString = curency;
                                break;
                            }
                    }

                    price = decimal.Parse(priceStringSource) / 1000000;

                    var priceStringNew = string.Format("{0} {1}", price.ToString("G29"), curencyString);

                    return priceStringNew;
                }
                else
                {
                    throw new Exception("Not Initialized");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("GetPriceForProduct error: " + ex.Message);
                throw new Exception("Not Initialized");
            }
        }
        
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            try
            {
                // Purchasing has succeeded initializing. Collect our Purchasing references.
                Debug.Log("OnInitialized: PASS");

                // Overall Purchasing system, configured with products for this application.
                m_StoreController = controller;

                // Store specific subsystem, for accessing device-specific store features.
                //m_StoreExtensionProvider = extensions;

                m_StoreExtensionProvider = extensions.GetExtension<IGooglePlayStoreExtensions>();

                foreach (var product in controller.products.all)
                {
                    products.Add(product.definition.storeSpecificId, product.metadata.GetGoogleProductMetadata().originalJson);
                    Debug.Log(String.Format("Product {0}: {1}", product.definition.storeSpecificId, product.metadata.GetGoogleProductMetadata().originalJson));
                }

                #region Subscription
                //Debug.Log("Available items:");
                //foreach (var item in controller.products.all)
                //{
                //    if (item.availableToPurchase)
                //    {
                //        Debug.Log(string.Join(" - ",
                //            new[]
                //            {
                //                item.metadata.localizedTitle,
                //                item.metadata.localizedDescription,
                //                item.metadata.isoCurrencyCode,
                //                item.metadata.localizedPrice.ToString(),
                //                item.metadata.localizedPriceString,
                //                item.transactionID,
                //                item.receipt
                //            }));

                //        // this is the usage of SubscriptionManager class
                //        if (item.receipt != null)
                //        {
                //            if (item.definition.type == ProductType.Subscription)
                //            {
                //                if (checkIfProductIsAvailableForSubscriptionManager(item.receipt))
                //                {
                //                    string intro_json = null; //(introductory_info_dict == null || !introductory_info_dict.ContainsKey(item.definition.storeSpecificId)) ? null : introductory_info_dict[item.definition.storeSpecificId];
                //                    SubscriptionManager p = new SubscriptionManager(item, intro_json);
                //                    SubscriptionInfo info = p.getSubscriptionInfo();
                //                    Debug.Log("product id is: " + info.getProductId());
                //                    Debug.Log("purchase date is: " + info.getPurchaseDate());
                //                    Debug.Log("subscription next billing date is: " + info.getExpireDate());
                //                    Debug.Log("is subscribed? " + info.isSubscribed().ToString());
                //                    Debug.Log("is expired? " + info.isExpired().ToString());
                //                    Debug.Log("is cancelled? " + info.isCancelled());
                //                    Debug.Log("product is in free trial peroid? " + info.isFreeTrial());
                //                    Debug.Log("product is auto renewing? " + info.isAutoRenewing());
                //                    Debug.Log("subscription remaining valid time until next billing date is: " + info.getRemainingTime());
                //                    Debug.Log("is this product in introductory price period? " + info.isIntroductoryPricePeriod());
                //                    Debug.Log("the product introductory localized price is: " + info.getIntroductoryPrice());
                //                    Debug.Log("the product introductory price period is: " + info.getIntroductoryPricePeriod());
                //                    Debug.Log("the number of product introductory price period cycles is: " + info.getIntroductoryPricePeriodCycles());
                //                }
                //                else
                //                {
                //                    Debug.Log("This product is not available for SubscriptionManager class, only products that are purchase by 1.19+ SDK can use this class.");
                //                }
                //            }
                //            else
                //            {
                //                Debug.Log("the product is not a subscription product");
                //            }
                //        }
                //        else
                //        {
                //            Debug.Log("the product should have a valid receipt");
                //        }

                //    }
                //} 
                #endregion

            }
            catch (Exception ex)
            {
                Debug.Log(String.Format("OnInitialized Error: {0}", ex.Message));
            }
        }


        public void OnInitializeFailed(InitializationFailureReason error)
        {
            // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
            Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
        }


        public void BuyConsumable(int months)
        {
            try
            {
                // Buy the consumable product using its general identifier. Expect a response either 
                // through ProcessPurchase or OnPurchaseFailed asynchronously.
                //BuyProductID(kProductIDConsumable);

                switch (months)
                {
                    case 12:
                        {
                            BuyProductID(kM12Consumable);
                            break;
                        }
                    case 3:
                        {
                            BuyProductID(kM3Consumable);
                            break;
                        }
                    case 1:
                        {
                            BuyProductID(kM1Consumable);
                            break;
                        }
                    default:
                        {
                            throw new Exception("Incorrect product to purchase");
                        }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("BuyConsumable error: " + ex.Message);
                throw new FQServiceException(FQServiceException.FQServiceExceptionType.DefaultError);
            }
        }

        void BuyProductID(string productId)
        {
            Debug.Log(string.Format("BuyProductID: {0}", productId));

            // If Purchasing has been initialized ...
            if (IsInitialized())
            {
                // ... look up the Product reference with the general product identifier and the Purchasing 
                // system's products collection.
                Product product = m_StoreController.products.WithID(productId);

                // If the look up found a product for this device's store and that product is ready to be sold ... 
                if (product != null && product.availableToPurchase)
                {
                    Debug.Log(string.Format("Purchasing product asychronously: '{0} ({1})|{2}'", product.definition.id, product.availableToPurchase, product.definition.storeSpecificId));

                    var obfuscatedAccountIdHash = GetObfuscatedAccountIdHash("fqpurchasesalt", CredentialHandler.Instance.CurrentUser.Id.ToString());

                    Debug.Log(string.Format("obfuscatedAccountIdHash: {0}", obfuscatedAccountIdHash));

                    m_StoreExtensionProvider.SetObfuscatedAccountId(obfuscatedAccountIdHash);

                    if (CircleProgressBar != null)
                    {
                        CircleProgressBar.SetActive(true);
                    }

                    calledByUser = true;

                    // ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed 
                    // asynchronously.
                    m_StoreController.InitiatePurchase(product);
                }
                // Otherwise ...
                else
                {
                    // ... report the product look-up failure situation  
                    Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");

                    throw new FQServiceException(FQServiceException.FQServiceExceptionType.DefaultError);
                }
            }
            // Otherwise ...
            else
            {
                // ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or 
                // retrying initiailization.
                Debug.Log("BuyProductID FAIL. Not initialized.");

                throw new FQServiceException(FQServiceException.FQServiceExceptionType.DefaultError);
            }
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            try
            {
                Debug.Log("ProcessPurchase");               

                if (String.Equals(args.purchasedProduct.definition.id, kM12Consumable, StringComparison.Ordinal) ||
                    String.Equals(args.purchasedProduct.definition.id, kM3Consumable, StringComparison.Ordinal) ||
                    String.Equals(args.purchasedProduct.definition.id, kM1Consumable, StringComparison.Ordinal) ||
                    String.Equals(args.purchasedProduct.definition.id, kM12Consumable_promo, StringComparison.Ordinal) ||
                    String.Equals(args.purchasedProduct.definition.id, kM1Consumable_promo, StringComparison.Ordinal))
                {
                                       
                    Debug.Log(string.Format("PurchaseReceipt: {0}", args.purchasedProduct.receipt));

                    Dictionary<string, object> receiptDetails = GetReceiptDetails(args.purchasedProduct.receipt);

                    Debug.Log(string.Format("PurchaseToken: {0}", receiptDetails["purchaseToken"].ToString()));

                    //В случае промокода информация отсутствует
                    if (receiptDetails.ContainsKey("acknowledged"))
                    {
                        Debug.Log(string.Format("Acknowledged: {0}", receiptDetails["acknowledged"].ToString()));
                    }

                    //В случае промокода информация отсутствует
                    if (receiptDetails.ContainsKey("obfuscatedAccountId"))
                    {
                        Debug.Log(string.Format("ObfuscatedAccountId: {0}", receiptDetails["obfuscatedAccountId"].ToString()));
                    }

                    //Этот параметр для конкретной покупки выставляет наш сервер.
                    //Если покупка ранее уже была корректно обработана и товар предоставлен пользователю - будет true.
                    if (!receiptDetails.ContainsKey("acknowledged") || 
                        (bool)receiptDetails["acknowledged"] == false)
                    {
                        //Незавершенная транзакция, требующая нашего внимания

                        //Сохраним в локальное значение и сбросим к дефолтному
                        bool calledByUserLocal = calledByUser;
                        calledByUser = false;

                        var obfuscatedAccountIdHash = GetObfuscatedAccountIdHash("fqpurchasesalt", CredentialHandler.Instance.CurrentUser.Id.ToString());

                        if (!receiptDetails.ContainsKey("obfuscatedAccountId") ||
                            obfuscatedAccountIdHash == receiptDetails["obfuscatedAccountId"].ToString())
                        {
                            //Всё норм - ID купившего и текущего юзера совпадают

                            //Отправка обработки покупки на сервер
                            GroupController.ProccessPurchase(receiptDetails["productId"].ToString(), receiptDetails["purchaseToken"].ToString())
                                .Then((res) =>
                                {
                                    Debug.Log($"status: {res.status}");

                                    if (res.result)
                                    {
                                        //Подтвердим корректную обработку транзакции, чтобы её обработка более не выстреливала 
                                        try
                                        {
                                            Debug.Log("ConfirmPendingPurchase start0");
                                            m_StoreController.ConfirmPendingPurchase(args.purchasedProduct);
                                        }
                                        catch (Exception exConfirmTransaction)
                                        {
                                            Debug.LogError("exConfirmTransaction error0: " + exConfirmTransaction.Message);
                                        }

                                        //Обновим в фоне инфу о премиум-доступе
                                        if (settingsPageController != null)
                                        {
                                            settingsPageController.UpdateSubscriptionInfo();
                                        }

                                        if (calledByUserLocal)
                                        {
                                            if (PopupSubscriptionDetailsController != null)
                                            {
                                                PopupSubscriptionDetailsController.Close();
                                            }

                                            if (CircleProgressBar != null)
                                            {
                                                CircleProgressBar.SetActive(false);
                                            }

                                            //В случае "покупки руками"
                                            Global_MessageBoxHandlerController.ShowMessageBox("Премиум-доступ", "Покупка успешно выполнена!");
                                        }
                                        else
                                        {
                                            //В случае автоматического вызова при инициализации
                                            Global_MessageBoxHandlerController.ShowMessageBox("Премиум-доступ", "Не завершенная ранее покупка успешно выполнена!");
                                        }
                                    }
                                    else
                                    {
                                        //Такого быть не должно
                                        //На всякий
                                        if (CircleProgressBar != null)
                                        {
                                            CircleProgressBar.SetActive(false);
                                        }
                                    }
                                })
                                .Catch((ex) =>
                                {
                                    Debug.LogError("ProccessPurchase error: " + ex.Message);

                                    if (Enum.TryParse(ex.Message, out FQServiceExceptionType _exType))
                                    {
                                        if (_exType == FQServiceExceptionType.PurchaseStateIsCanceled)
                                        {
                                            //Ситуация возможна (легко воспроизводится на практике), когда операция не была подтверждена на сервере. 
                                            //А затем (до попаданию сюда) покупка была отменена (о чём знает GooglePlay, но не знает сервис UnityIAP, настойчиво требующий обработать транзакцию)

                                            //Подтвердим, что для нас с ней всё ясно, чтобы обработка более не выстреливала

                                            //TODO: нареканий по падению функции нет, но результат не всегда достигается (довольно редко и незакономерно),
                                            //что приводит к ошибке DuplicateTransaction и необходимости перелогина для возможности совершить новую покупку.
                                            //Критичность только в удобстве пользователя.
                                            try
                                            {
                                                m_StoreController.ConfirmPendingPurchase(args.purchasedProduct);
                                            }
                                            catch (Exception exConfirmTransaction)
                                            {
                                                Debug.LogError("exConfirmTransaction error: " + exConfirmTransaction.Message);
                                            }

                                        }

                                        if (_exType == FQServiceExceptionType.AcknowledgementStateIsAcknowledged)
                                        {
                                            //Теоретически возможная ситуация, когда операция была подтверждена на сервере (о чём знает GooglePlay, но не знает сервис UnityIAP, настойчиво требующий обработать транзакцию)
                                            //Подтвердим, что всё корректно обработано

                                            try
                                            {
                                                m_StoreController.ConfirmPendingPurchase(args.purchasedProduct);
                                            }
                                            catch (Exception exConfirmTransaction)
                                            {
                                                Debug.LogError("exConfirmTransaction error: " + exConfirmTransaction.Message);
                                            }
                                        }

                                        if (_exType == FQServiceExceptionType.PurchaseIsAlreadyExists)
                                        {
                                            //Теоретически возможная ситуация, когда покупка была добавлена группе, но Acknowledeg не подтверждена Гуглу на сервере (по какой-то причине) - 
                                            //тогда при следующей инициализации встрелила повторная обработка, и попали сюда.
                                            //Подтвердим, что всё корректно обработано

                                            try
                                            {
                                                m_StoreController.ConfirmPendingPurchase(args.purchasedProduct);
                                            }
                                            catch (Exception exConfirmTransaction)
                                            {
                                                Debug.LogError("exConfirmTransaction error: " + exConfirmTransaction.Message);
                                            }
                                        }
                                    }

                                    //Если сюда попали, то ошибку уже обработали в DataModel и показали уведомление
                                    //Нужно только убрать индикацию загрузки

                                    if (CircleProgressBar != null)
                                    {
                                        CircleProgressBar.SetActive(false);
                                    }
                                });
                        }
                        else
                        {
                            //Расклад возможен лишь при незавершенной операции покупки и перезаходе в аккаунт другой группы
                            if (!calledByUserLocal)
                            {
                                //Произошел автоматический вызов при инициализации
                                Global_MessageBoxHandlerController.ShowMessageBox("Премиум-доступ", "Обнаружена незавершенная покупка, не соответствующая текущей группе.\n\nВыполните вход в соответствующий аккаунт или отмените покупку в истории заказов Google Play.", MessageBoxType.Warning);
                            }
                            else
                            {
                                //А вот тут стрём. Такого быть не должно при вызове из UI
                                //но всё же обработаем.
                                FQServiceException.ShowExceptionMessage(FQServiceExceptionType.DefaultError);
                            }
                        }

                        //Вернём "В ожидании", т.к. об остальном позаботится сервер и обработка ответа (then\catch промиса).
                        Debug.Log("Return Pending");
                        return PurchaseProcessingResult.Pending;

                    }
                    else
                    {
                        //Здесь какая-то подвисшая транзакция, которая была обработана ранее (теоретически такой вариант невозможен)
                        //Нас не интересует - вернём complete.

                        Debug.Log("Return Complete");
                        return PurchaseProcessingResult.Complete;
                    }
                }
                else
                {
                    throw new Exception(string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'", args.purchasedProduct.definition.id));
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);

                FQServiceException.ShowExceptionMessage(ex);

                return PurchaseProcessingResult.Pending;
            }
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            if (CircleProgressBar != null)
            {
                CircleProgressBar.SetActive(false);
            }

            // A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
            // this reason with the user to guide their troubleshooting actions.
            Debug.LogError(string.Format("OnPurchaseFailed: '{0}' ({1}), PurchaseFailureReason: {2}", product.definition.storeSpecificId, product.availableToPurchase, failureReason));

            Debug.Log(string.Format("PurchaseFailedReceipt: {0}", product.receipt));

            if (failureReason != PurchaseFailureReason.UserCancelled)
            {
                if (failureReason == PurchaseFailureReason.DuplicateTransaction)
                {
                    Global_MessageBoxHandlerController.ShowMessageBox("Премиум-доступ", "Обнаружена незавершенная покупка.\n\nПерезайдите в аккаунт для завершения.", MessageBoxType.Warning);
                }
                else
                {
                    FQServiceException.ShowExceptionMessage(FQServiceException.FQServiceExceptionType.DefaultError);
                }
            }
        }

        private Dictionary<string, object> GetReceiptDetails(string receipt)
        {
            try
            {
                Dictionary<string, object> details = new Dictionary<string, object>();

                var receipt_wrapper = JsonConvert.DeserializeObject<Dictionary<string, object>>(receipt);
                var payload_wrapper = JsonConvert.DeserializeObject<Dictionary<string, object>>(receipt_wrapper["Payload"].ToString());
                var original_json_payload_wrapper = JsonConvert.DeserializeObject<Dictionary<string, object>>(payload_wrapper["json"].ToString());
                details.Add("purchaseToken", original_json_payload_wrapper["purchaseToken"].ToString());
                details.Add("productId", original_json_payload_wrapper["productId"].ToString());

                //В случае промокода информация отсутствует
                if (original_json_payload_wrapper.ContainsKey("obfuscatedAccountId"))
                {
                    details.Add("obfuscatedAccountId", original_json_payload_wrapper["obfuscatedAccountId"].ToString());
                }

                //В случае промокода информация отсутствует
                if (original_json_payload_wrapper.ContainsKey("acknowledged"))
                {
                    details.Add("acknowledged", original_json_payload_wrapper["acknowledged"]);
                }

                return details;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                throw;
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

        //public void BuyNonConsumable()
        //{
        //    // Buy the non-consumable product using its general identifier. Expect a response either 
        //    // through ProcessPurchase or OnPurchaseFailed asynchronously.
        //    BuyProductID(kProductIDNonConsumable);
        //}


        //public void BuySubscription()
        //{
        //    // Buy the subscription product using its the general identifier. Expect a response either 
        //    // through ProcessPurchase or OnPurchaseFailed asynchronously.
        //    // Notice how we use the general product identifier in spite of this ID being mapped to
        //    // custom store-specific identifiers above.
        //    BuyProductID(kProductIDSubscription);
        //}

        //// Restore purchases previously made by this customer. Some platforms automatically restore purchases, like Google. 
        //// Apple currently requires explicit purchase restoration for IAP, conditionally displaying a password prompt.
        //public void RestorePurchases()
        //{
        //    // If Purchasing has not yet been set up ...
        //    if (!IsInitialized())
        //    {
        //        // ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
        //        Debug.Log("RestorePurchases FAIL. Not initialized.");
        //        return;
        //    }

        //    // If we are running on an Apple device ... 
        //    if (Application.platform == RuntimePlatform.IPhonePlayer ||
        //        Application.platform == RuntimePlatform.OSXPlayer)
        //    {
        //        // ... begin restoring purchases
        //        Debug.Log("RestorePurchases started ...");

        //        //// Fetch the Apple store-specific subsystem.
        //        //var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
        //        //// Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
        //        //// the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
        //        //apple.RestoreTransactions((result) =>
        //        //{
        //        //    // The first phase of restoration. If no more responses are received on ProcessPurchase then 
        //        //    // no purchases are available to be restored.
        //        //    Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
        //        //});
        //    }
        //    // Otherwise ...
        //    else
        //    {
        //        // We are not running on an Apple device. No work is necessary to restore purchases.
        //        Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
        //    }
        //}


        //  
        // --- IStoreListener
        //



        //public void onPurchasesUpdated(BillingResult billingResult, List<Purchase> purchases)
        //{

        //}

        //private string GetPurchaseToken(string receipt)
        //{
        //    try
        //    {
        //        string result = "";

        //        var receipt_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode(receipt);

        //        var payload = (string)receipt_wrapper["Payload"];
        //        var payload_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode(payload);

        //        var original_json_payload_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode((string)payload_wrapper["json"]);
        //        result = (string)original_json_payload_wrapper["purchaseToken"];

        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.LogError(ex.Message);
        //        return String.Empty;
        //    }
        //}


        //private bool checkIfProductIsAvailableForSubscriptionManager(string receipt)
        //{
        //    var receipt_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode(receipt);
        //    if (!receipt_wrapper.ContainsKey("Store") || !receipt_wrapper.ContainsKey("Payload"))
        //    {
        //        Debug.Log("The product receipt does not contain enough information");
        //        return false;
        //    }
        //    var store = (string)receipt_wrapper["Store"];
        //    var payload = (string)receipt_wrapper["Payload"];

        //    if (payload != null)
        //    {
        //        switch (store)
        //        {
        //            case GooglePlay.Name:
        //                {
        //                    var payload_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode(payload);
        //                    if (!payload_wrapper.ContainsKey("json"))
        //                    {
        //                        Debug.Log("The product receipt does not contain enough information, the 'json' field is missing");
        //                        return false;
        //                    }
        //                    var original_json_payload_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode((string)payload_wrapper["json"]);
        //                    if (original_json_payload_wrapper == null || !original_json_payload_wrapper.ContainsKey("developerPayload"))
        //                    {
        //                        Debug.Log("The product receipt does not contain enough information, the 'developerPayload' field is missing");
        //                        return false;
        //                    }
        //                    var developerPayloadJSON = (string)original_json_payload_wrapper["developerPayload"];
        //                    var developerPayload_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode(developerPayloadJSON);
        //                    if (developerPayload_wrapper == null || !developerPayload_wrapper.ContainsKey("is_free_trial") || !developerPayload_wrapper.ContainsKey("has_introductory_price_trial"))
        //                    {
        //                        Debug.Log("The product receipt does not contain enough information, the product is not purchased using 1.19 or later");
        //                        return false;
        //                    }
        //                    return true;
        //                }
        //            //case AppleAppStore.Name:
        //            //case AmazonApps.Name:
        //            //case MacAppStore.Name:
        //            //    {
        //            //        return true;
        //            //    }
        //            default:
        //                {
        //                    return false;
        //                }
        //        }
        //    }
        //    return false;
        //}
    }
}