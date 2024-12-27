using Code.Models;
using System;
using TMPro;
using UnityEngine;
using Code.ViewControllers.TList;
using Code.Models.REST;
using UnityEngine.UI;
using Assets.Code.Models.REST.CommonTypes;
using Code.Controllers.MessageBox;
using System.Globalization;

namespace Code.ViewControllers
{
    public class RewardPageController : MonoBehaviour
    {
        public GameObject thisForm;

        public GameObject CircleProgressBar;
        //public TextMeshProUGUI AmountText;
        public TextMeshProUGUI CoinsText;
        public TListViewController ScrollRect_AvailableRewards;
        public TListViewController ScrollRect_ObtainedRewards;
        public TListViewController ScrollRect_AvailableRewards_Children;
        public TListViewController ScrollRect_ObtainedRewards_Children;

        public BaseRewardListFilter TypeChooser_RewardListFilter;

        private TListViewController m_currentList
        {
            get
            {
                switch (TypeChooser_RewardListFilter.CurrentActiveFilter)
                {
                    case RewardModel.BaseRewardFilter.Available:
                        return ScrollRect_AvailableRewards;

                    case RewardModel.BaseRewardFilter.Obtained:
                        return ScrollRect_ObtainedRewards;
                };
                return ScrollRect_AvailableRewards;
            }
        }

        // Start is called before the first frame update
        void Awake()
        {

            try
            {
                // подпишемся на изменение списка
                DataModel.Instance.Rewards.OnListChanged += Rewards_ListChanged; ;

                if (CredentialHandler.Instance.CurrentUser.Role == Models.RoleModel.RoleTypes.User)
                {
                    ScrollRect_AvailableRewards = ScrollRect_AvailableRewards_Children;
                    ScrollRect_ObtainedRewards = ScrollRect_ObtainedRewards_Children;
                }

                // подпишемся на событие требования обновления от листвью
                ScrollRect_AvailableRewards.OnNeedListRefresh += M_listViewController_NeedListRefresh;
                ScrollRect_ObtainedRewards.OnNeedListRefresh += M_listViewController_NeedListRefresh;

                // если лист прежде не был получен, то сымитируем требование обновления от листа
                if (DataModel.Instance.Rewards.Rewards == null)
                    M_listViewController_NeedListRefresh(null, null);
                else
                {
                    SetNewList();
                }

                TypeChooser_RewardListFilter.OnFilterChanged += TypeChooser_RewardListFilter_OnFilterChanged;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }

            thisForm.SetActive(false);
        }

        private void Rewards_ListChanged(object sender, EventArgs e)
        {
            SetNewList();
        }

        private void TypeChooser_RewardListFilter_OnFilterChanged(object sender, EventArgs e)
        {
            ScrollRect_AvailableRewards.gameObject.SetActive(TypeChooser_RewardListFilter.CurrentActiveFilter == RewardModel.BaseRewardFilter.Available);
            ScrollRect_ObtainedRewards.gameObject.SetActive(TypeChooser_RewardListFilter.CurrentActiveFilter == RewardModel.BaseRewardFilter.Obtained);

            //if (m_currentList.Items?.Count > 0)
            //{
            //    AmountText.text = $"{m_currentList.Items?.Count}";
            //}
            //else
            //{
            //    AmountText.text = "-";
            //}
        }

        private void M_listViewController_ListChanged(object sender, EventArgs e)
        {
            //SetSelectedAmountText();
        }

        private void OnDestroy()
        {
            try
            {
                // отпишемся от изменение списка
                DataModel.Instance.Rewards.OnListChanged -= Rewards_ListChanged;
                // отпишемся от события требования обновления от листвью
                ScrollRect_AvailableRewards.OnNeedListRefresh -= M_listViewController_NeedListRefresh;
                ScrollRect_ObtainedRewards.OnNeedListRefresh -= M_listViewController_NeedListRefresh;
                // отпишемся от обновлений фильтра
                TypeChooser_RewardListFilter.OnFilterChanged -= TypeChooser_RewardListFilter_OnFilterChanged;

                AvailableRewardListFilter.ResetFilters();
                ObtainedRewardListFilter.ResetFilters();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        private void M_listViewController_NeedListRefresh(object sender, System.EventArgs e)
        {
            try
            {
                CircleProgressBar.SetActive(true);

                DataModel.Instance.Credentials.UpdateAllCredentials(true, false, true) //чтобы корректно показывать статус доступности наград
                    .Then((result_UpdateAllCredentials) =>
                    {
                        if (result_UpdateAllCredentials.result)
                        {
                            DataModel.Instance.Rewards.UpdateAllRewardsItems()
                                .Then((res) =>
                                {
                                    CircleProgressBar.SetActive(false);
                                })
                                .Catch((ex) =>
                                {
                                    //Если сюда попали, то ошибку уже обработали в DataModel и показали уведомление
                                    //Нужно только убрать индикацию загрузки
                                    CircleProgressBar.SetActive(false);
                                });
                        }
                        else
                        {
                            CircleProgressBar.SetActive(false);
                        }
                    })
                    .Catch((ex) =>
                    {
                        //Если сюда попали, то ошибку уже обработали в DataModel и показали уведомление
                        //Нужно только убрать индикацию загрузки
                        CircleProgressBar.SetActive(false);
                    });
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);

                CircleProgressBar.SetActive(false);

                FQServiceException.ShowExceptionMessage(ex);
            }
        }

        private void SetNewList(/*List<Dictionary<string, object>> newList*/)
        {
            ScrollRect_AvailableRewards.SetListItems(RewardModel.ToListOfDictionary(DataModel.Instance.Rewards.AvailableRewards));
            ScrollRect_ObtainedRewards.SetListItems(RewardModel.ToListOfDictionary(DataModel.Instance.Rewards.ObtainedRewards));

            //if (m_currentList.Items?.Count > 0)
            //{
            //    AmountText.text = $"{m_currentList.Items?.Count}";
            //}
            //else
            //{
            //    AmountText.text = "-";
            //}

            //Актуализация отображаемых фильтров
            var availableFilters = GetComponentsInChildren<AvailableRewardListFilter>(true);
            foreach (var filter in availableFilters)
            {
                try
                {
                    filter.SetActualSelectedUsers();                    
                }
                catch
                {

                }
            }

            //Актуализация отображаемых фильтров
            var obtainedeFilters = GetComponentsInChildren<ObtainedRewardListFilter>(true);
            foreach (var filter in obtainedeFilters)
            {
                try
                {
                    filter.SetActualSelectedUsers();                    
                }
                catch
                {

                }
            }


            if (CredentialHandler.Instance.CurrentUser.Role == Models.RoleModel.RoleTypes.User)
            {
                if (CoinsText != null)
                {
                    if (CredentialHandler.Instance.CurrentUser.Coins >= 1000)
                    {
                        CoinsText.text = CredentialHandler.Instance.CurrentUser.Coins.ToString("0,0", CultureInfo.CreateSpecificCulture("el-GR"));
                    }
                    else
                    {
                        CoinsText.text = $"{CredentialHandler.Instance.CurrentUser.Coins}";
                    }
                }
            }
        }    
    }

}