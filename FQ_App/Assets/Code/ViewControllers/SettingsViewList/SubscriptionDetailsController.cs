using Code.Models;
using System;
using TMPro;
using UnityEngine;
using Code.ViewControllers.TList;
using Code.Models.REST;
using UnityEngine.UI;
using Assets.Code.Models.REST.CommonTypes;
using Code.Controllers.MessageBox;
using Ricimi;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Globalization;

namespace Code.ViewControllers
{
    [RequireComponent(typeof(Popup))]
    public class SubscriptionDetailsController : MonoBehaviour
    {
        public GameObject CircleProgressBar;

        public GameObject ScrollRect_m12;
        public TextMeshProUGUI Toggle_m12;
        public TextMeshProUGUI PriceActual_m12;
        public TextMeshProUGUI Price_m12;

        public GameObject ScrollRect_m3;
        public TextMeshProUGUI Toggle_m3;
        public TextMeshProUGUI PriceActual_m3;
        public TextMeshProUGUI Price_m3;

        public GameObject ScrollRect_m1;
        public TextMeshProUGUI Toggle_m1;
        public TextMeshProUGUI Price_m1;

        public BaseSubscriptionsListFilter TypeChooser_SubscriptionListFilter;

        public PurchaseWorker purchaseWorker;

        void Awake()
        {
            try
            {
                TypeChooser_SubscriptionListFilter.OnFilterChanged += TypeChooser_SubscriptionListFilter_OnFilterChanged;                
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        //TODO: динамическая установка цен в следующий апдейт
        public void SetData()
        {
            try
            {
                purchaseWorker.CircleProgressBar = CircleProgressBar;
                purchaseWorker.PopupSubscriptionDetailsController = this.gameObject.GetComponent<Popup>();

                PriceActual_m12.text = purchaseWorker.GetPriceForProduct(PurchaseWorker.kM12GooglePlayProduct, out decimal priceM12, out string curencyStringM12);
                PriceActual_m3.text = purchaseWorker.GetPriceForProduct(PurchaseWorker.kM3GooglePlayProduct, out decimal priceM3, out string curencyStringM3);
                Price_m1.text = purchaseWorker.GetPriceForProduct(PurchaseWorker.kM1GooglePlayProduct, out decimal priceM1, out string curencyStringM1);

                Price_m12.text = string.Format("{0} {1}", (priceM1 * 12).ToString("G29"), curencyStringM12);
                Price_m3.text = string.Format("{0} {1}", (priceM1 * 3).ToString("G29"), curencyStringM3);

                bool showDecimalPart = false;

                if (Decimal.Round(priceM12) < 99 ||
                    Decimal.Round(priceM3) < 99 ||
                    Decimal.Round(priceM1) < 99)
                {
                    showDecimalPart = true;
                }

                //TODO: хитрость, чтобы не показывать копейки
                if (showDecimalPart)
                {
                    Toggle_m12.text += string.Format("<size=80%>{0} {1}/мес.</size>", (priceM12 / 12).ToString("#.##"), curencyStringM12);
                }
                else
                {
                    Toggle_m12.text += string.Format("<size=80%>{0} {1}/мес.</size>", Decimal.Round(priceM12 / 12).ToString(), curencyStringM12);
                }

                if (showDecimalPart)
                {
                    Toggle_m3.text += string.Format("<size=80%>{0} {1}/мес.</size>", (priceM3 / 3).ToString("#.##"), curencyStringM3);
                }
                else
                {
                    Toggle_m3.text += string.Format("<size=80%>{0} {1}/мес.</size>", Decimal.Round(priceM3 / 3).ToString(), curencyStringM3);
                }

                if (showDecimalPart)
                {
                    Toggle_m1.text += string.Format("<size=80%>{0} {1}/мес.</size>", priceM1.ToString("#.##"), curencyStringM1);                    
                }
                else
                {
                    Toggle_m1.text += string.Format("<size=80%>{0} {1}/мес.</size>", Decimal.Round(priceM1).ToString(), curencyStringM1);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        private void TypeChooser_SubscriptionListFilter_OnFilterChanged(object sender, EventArgs e)
        {
            ScrollRect_m12.SetActive(TypeChooser_SubscriptionListFilter.CurrentActiveFilter == BaseSubscriptionFilter.m12);
            ScrollRect_m3.SetActive(TypeChooser_SubscriptionListFilter.CurrentActiveFilter == BaseSubscriptionFilter.m3);
            ScrollRect_m1.SetActive(TypeChooser_SubscriptionListFilter.CurrentActiveFilter == BaseSubscriptionFilter.m1);
        }

        public void OnButton_Purchase()
        {
            try
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    switch (TypeChooser_SubscriptionListFilter.CurrentActiveFilter)
                    {
                        case BaseSubscriptionFilter.m12:
                            {
                                purchaseWorker.BuyConsumable(12);
                                break;
                            }
                        case BaseSubscriptionFilter.m3:
                            {
                                purchaseWorker.BuyConsumable(3);
                                break;
                            }
                        case BaseSubscriptionFilter.m1:
                            {
                                purchaseWorker.BuyConsumable(1);
                                break;
                            }
                        default:
                            {
                                throw new Exception("Incorrect product to purchase");
                            }
                    }                    
                }
                else
                {
                    Global_MessageBoxHandlerController.ShowMessageBox("Премиум-доступ", "К сожалению, в данный момент покупки возможны только с устройства Android.", MessageBoxType.Information);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);                

                FQServiceException.ShowExceptionMessage(ex);
            }
        }

        private void OnDestroy()
        {
            try
            {
                // отпишемся от обновлений фильтра
                TypeChooser_SubscriptionListFilter.OnFilterChanged -= TypeChooser_SubscriptionListFilter_OnFilterChanged;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }
    }
}