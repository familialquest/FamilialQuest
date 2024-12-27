using Code.Models;
using System;
using TMPro;
using UnityEngine;
using Code.ViewControllers.TList;
using Code.Models.REST;
using UnityEngine.UI;
using Assets.Code.Models.REST.CommonTypes;
using Code.Controllers.MessageBox;
using Assets.Code.Controllers;
using UnityEngine.SceneManagement;
using System.Globalization;
using Code.Models.REST.Users;

namespace Code.ViewControllers
{
    public class HistoryEventPageController : MonoBehaviour
    {
        public GameObject CircleProgressBar;

        //public TextMeshProUGUI AmountText;
        public TextMeshProUGUI CoinsText;
        public TListViewController ScrollRect_AllHistoryEvents;
        public TListViewController ScrollRect_TaskHistoryEvents;
        public TListViewController ScrollRect_RewardHistoryEvents;
        public TListViewController ScrollRect_UserHistoryEvents;
        public TListViewController ScrollRect_AllHistoryEvents_Children;
        public TListViewController ScrollRect_TaskHistoryEvents_Children;
        public TListViewController ScrollRect_RewardHistoryEvents_Children;
        public TListViewController ScrollRect_UserHistoryEvents_Children;


        public BaseHistoryEventListFilter TypeChooser_HistoryEventListFilter;

        private TListViewController m_currentList
        {
            get
            {
                switch (TypeChooser_HistoryEventListFilter.CurrentActiveFilter)
                {
                    case HistoryEventModel.BaseHistoryEventFilter.All:
                        return ScrollRect_AllHistoryEvents;
                    case HistoryEventModel.BaseHistoryEventFilter.Task:
                        return ScrollRect_TaskHistoryEvents;
                    case HistoryEventModel.BaseHistoryEventFilter.Reward:
                        return ScrollRect_RewardHistoryEvents;
                    case HistoryEventModel.BaseHistoryEventFilter.User:
                        return ScrollRect_UserHistoryEvents;


                };
                return ScrollRect_AllHistoryEvents;
            }
        }

        // Start is called before the first frame update
        void Awake()
        {
            try
            {

                // подпишемся на изменение списка
                DataModel.Instance.HistoryEvents.OnListChanged += HistoryEvent_ListChanged;

                if (CredentialHandler.Instance.CurrentUser.Role == Models.RoleModel.RoleTypes.User)
                {
                    ScrollRect_AllHistoryEvents = ScrollRect_AllHistoryEvents_Children;
                    ScrollRect_TaskHistoryEvents = ScrollRect_TaskHistoryEvents_Children;
                    ScrollRect_RewardHistoryEvents = ScrollRect_RewardHistoryEvents_Children;
                    ScrollRect_UserHistoryEvents = ScrollRect_UserHistoryEvents_Children;
                }

                // подпишемся на событие требования обновления от листвью
                ScrollRect_AllHistoryEvents.OnNeedListRefresh += M_listViewController_NeedListRefresh;
                ScrollRect_TaskHistoryEvents.OnNeedListRefresh += M_listViewController_NeedListRefresh;
                ScrollRect_RewardHistoryEvents.OnNeedListRefresh += M_listViewController_NeedListRefresh;
                ScrollRect_UserHistoryEvents.OnNeedListRefresh += M_listViewController_NeedListRefresh;

                // если лист прежде не был получен, то сымитируем требование обновления от листа
                if (DataModel.Instance.HistoryEvents.HistoryEvents == null)
                    M_listViewController_NeedListRefresh(null, null);
                else
                {
                    SetNewList();
                }

                TypeChooser_HistoryEventListFilter.OnFilterChanged += TypeChooser_HistoryEventListFilter_OnFilterChanged;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        private void HistoryEvent_ListChanged(object sender, EventArgs e)
        {
            try
            {
                SetNewList();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        private void TypeChooser_HistoryEventListFilter_OnFilterChanged(object sender, EventArgs e)
        {
            try
            {
                ScrollRect_AllHistoryEvents.gameObject.SetActive(TypeChooser_HistoryEventListFilter.CurrentActiveFilter == HistoryEventModel.BaseHistoryEventFilter.All);
                ScrollRect_TaskHistoryEvents.gameObject.SetActive(TypeChooser_HistoryEventListFilter.CurrentActiveFilter == HistoryEventModel.BaseHistoryEventFilter.Task);
                ScrollRect_RewardHistoryEvents.gameObject.SetActive(TypeChooser_HistoryEventListFilter.CurrentActiveFilter == HistoryEventModel.BaseHistoryEventFilter.Reward);
                ScrollRect_UserHistoryEvents.gameObject.SetActive(TypeChooser_HistoryEventListFilter.CurrentActiveFilter == HistoryEventModel.BaseHistoryEventFilter.User);

                //AmountText.text = $"{m_currentList.Items.Count}";
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
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
                DataModel.Instance.HistoryEvents.OnListChanged -= HistoryEvent_ListChanged;

                // отпишемся от события требования обновления от листвью
                ScrollRect_AllHistoryEvents.OnNeedListRefresh -= M_listViewController_NeedListRefresh;
                ScrollRect_TaskHistoryEvents.OnNeedListRefresh -= M_listViewController_NeedListRefresh;
                ScrollRect_RewardHistoryEvents.OnNeedListRefresh -= M_listViewController_NeedListRefresh;
                ScrollRect_UserHistoryEvents.OnNeedListRefresh -= M_listViewController_NeedListRefresh;

                // отпишемся от обновлений фильтра
                TypeChooser_HistoryEventListFilter.OnFilterChanged -= TypeChooser_HistoryEventListFilter_OnFilterChanged;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        public void OnClickShowNewItems()
        {
            M_listViewController_NeedListRefresh(null, null);
        }

        private void M_listViewController_NeedListRefresh(object sender, System.EventArgs e)
        {
            try
            {
                CircleProgressBar.SetActive(true);
                DataModel.Instance.HistoryEvents.UpdateHistoryEventsItems()
                    .Then((res) =>
                    {
                        CircleProgressBar.SetActive(false);
                    })
                    .Catch((ex) =>
                    {
                        Debug.LogError(ex);

                        CircleProgressBar.SetActive(false);

                        FQServiceException.ShowExceptionMessage(ex);
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
            try
            {
                ScrollRect_AllHistoryEvents.SetListItems(HistoryEventModel.ToListOfDictionary(DataModel.Instance.HistoryEvents.HistoryEvents));
                ScrollRect_TaskHistoryEvents.SetListItems(HistoryEventModel.ToListOfDictionary(DataModel.Instance.HistoryEvents.TaskHistoryEvents));
                ScrollRect_RewardHistoryEvents.SetListItems(HistoryEventModel.ToListOfDictionary(DataModel.Instance.HistoryEvents.RewardHistoryEvents));
                ScrollRect_UserHistoryEvents.SetListItems(HistoryEventModel.ToListOfDictionary(DataModel.Instance.HistoryEvents.UserHistoryEvents));

                //AmountText.text = $"{m_currentList.Items?.Count}";

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
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }        
    }
}