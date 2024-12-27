using Assets.Code.Models.REST.CommonTypes;
using Code.Models;
using Code.Models.REST.CommonType.Tasks;
using Code.Models.REST.Users;
using Code.ViewControllers.TList;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static Assets.Code.Models.Reward.BaseReward;
using static Code.Models.CredentialsModel;
using static Code.Models.RewardModel;
using static Code.Models.TaskModel;

namespace Code.ViewControllers
{
    public class OnlineGroupParentsListFilter : ListFilter
    {
        public TMP_Text StatusFilterLabel;
        public TMP_Text UsersFilterLabel;

        public bool isParentTab;

        private static Dictionary<OnlineStatusFilter, bool> CurrentStatusesActiveFilter;
        private static Dictionary<OnlineStatusFilter, bool> CurrentStatusesActiveFilterDelegate;

        // Start is called before the first frame update
        void Awake()
        {
            try
            {
                ResetFilters();               
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        public static void ResetFilters()
        {
            CurrentStatusesActiveFilter = new Dictionary<OnlineStatusFilter, bool>();

            CurrentStatusesActiveFilter.Add(OnlineStatusFilter.All, true);
            CurrentStatusesActiveFilter.Add(OnlineStatusFilter.Online, true);
            CurrentStatusesActiveFilter.Add(OnlineStatusFilter.NotOnline, true);
        }

        public void OnClick_ButtonChangeStatusFilter(Ricimi.PopupOpener editPopupOpener)
        {
            try
            {
                editPopupOpener.OpenPopup(out var popup);
                PopupGroupStatusSelectorOnlineParentsPageController taskStatusSelectorPageController = popup.GetComponent<PopupGroupStatusSelectorOnlineParentsPageController>();
                CurrentStatusesActiveFilterDelegate = new Dictionary<OnlineStatusFilter, bool>(CurrentStatusesActiveFilter);
                taskStatusSelectorPageController.SetValues(CurrentStatusesActiveFilterDelegate);
                taskStatusSelectorPageController.AfterTaskStatusSelectedDelegate = AfterTaskStatusSelected;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                FQServiceException.ShowExceptionMessage(ex);
            }
        }

        public void AfterTaskStatusSelected()
        {
            try
            {
                if (CurrentStatusesActiveFilter == null)
                {
                    ResetFilters();
                }

                if (CurrentStatusesActiveFilterDelegate != null)
                {
                    CurrentStatusesActiveFilter = new Dictionary<OnlineStatusFilter, bool>(CurrentStatusesActiveFilterDelegate);
                    CurrentStatusesActiveFilterDelegate = null;
                }

                if (CurrentStatusesActiveFilter[OnlineStatusFilter.All])
                {
                    StatusFilterLabel.text = TskListFilterToString(OnlineStatusFilter.All);
                }
                else
                {
                    if (CurrentStatusesActiveFilter[OnlineStatusFilter.Online])
                    {
                        StatusFilterLabel.text = TskListFilterToString(OnlineStatusFilter.Online);
                    }

                    if (CurrentStatusesActiveFilter[OnlineStatusFilter.NotOnline])
                    {
                        StatusFilterLabel.text = TskListFilterToString(OnlineStatusFilter.NotOnline);
                    }
                }

                FilterChanged(EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        public override bool FilterItem(object item)
        {
            try
            {
                if (CurrentStatusesActiveFilter == null)
                {
                    ResetFilters();
                }

                bool resultStatus = false;

                if (!(item is Dictionary<string, object>))
                    return false;

                if (CurrentStatusesActiveFilter[OnlineStatusFilter.All])
                {
                    return true;
                }
                else
                {
                    Dictionary<string, object> dict = (Dictionary<string, object>)item;

                    if (!resultStatus)
                    {
                        bool isOnline = false;

                        if (dict["LastAction"] != null && 
                            Int32.TryParse(dict["LastAction"].ToString(), out int lastAction))
                        {
                            if (lastAction >= 0)
                            {
                                if (lastAction > 15)
                                {
                                    isOnline = false;
                                }
                                else
                                {
                                    isOnline = true;
                                }
                            }
                        }

                        if (CurrentStatusesActiveFilter[OnlineStatusFilter.Online])
                        {
                            if (isOnline)
                            {
                                return true;
                            }
                        }

                        if (CurrentStatusesActiveFilter[OnlineStatusFilter.NotOnline])
                        {
                            if (!isOnline)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        private string TskListFilterToString(OnlineStatusFilter filterValue)
        {
            try
            {
                var filterValueLabel = string.Empty;

                switch (filterValue)
                {
                    case OnlineStatusFilter.All:
                        {
                            filterValueLabel = "Все статусы";
                            break;
                        }
                    case OnlineStatusFilter.Online:
                        {
                            filterValueLabel = "В сети";
                            break;
                        }
                    case OnlineStatusFilter.NotOnline:
                        {
                            filterValueLabel = "Не в сети";
                            break;
                        }
                }

                return filterValueLabel;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }
    }

}

//using Code.Models;
//using Code.Models.REST.Rewards;
//using Code.ViewControllers.TList;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using TMPro;
//using UnityEngine;
//using static Code.Models.CredentialsModel;

//namespace Code.ViewControllers
//{
//    public class OnlineGroupListFilter : ListFilter
//    {
//        public OnlineStatusFilter DefaultActiveFilter;

//        [HideInInspector]
//        public OnlineStatusFilter CurrentActiveFilter;

//        public TMP_Text FilterLabel;
//        public GameObject[] FilterIcons;
//        private int currentNumber;

//        // Start is called before the first frame update
//        void Awake()
//        {
//            try
//            {
//                CurrentActiveFilter = DefaultActiveFilter;
//            }
//            catch (Exception ex)
//            {
//                Debug.LogError(ex);
//                throw;
//            }
//        }

//        public void OnClick_ButtonChangeFilter()
//        {
//            try
//            {
//                currentNumber++;
//                int current = currentNumber;


//                if (current >= Enum.GetNames(typeof(OnlineStatusFilter)).Length)
//                {
//                    currentNumber = 0;
//                    current = currentNumber;
//                }

//                if (CurrentActiveFilter == (OnlineStatusFilter)current)
//                    return;

//                CurrentActiveFilter = (OnlineStatusFilter)current;

//                foreach (var filterIcon in FilterIcons)
//                {
//                    filterIcon.SetActive(false);
//                }
//                FilterIcons[current].SetActive(true);

//                FilterLabel.text = TskListFilterToString(CurrentActiveFilter);

//                FilterChanged(EventArgs.Empty);
//            }
//            catch (Exception ex)
//            {
//                Debug.LogError(ex);
//                CurrentActiveFilter = DefaultActiveFilter;
//            }
//        }

//        public override bool FilterItem(object item)
//        {
//            try
//            {
//                if (!(item is Dictionary<string, object>))
//                    return false;

//                if (CurrentActiveFilter == OnlineStatusFilter.All)
//                    return true;

//                bool isOnline = false;

//                Dictionary<string, object> dict = (Dictionary<string, object>)item;

//                if (dict["LastAction"] != null && DateTime.TryParse(dict["LastAction"].ToString(), out DateTime lastAction))
//                {
//                    var diffTime = DateTime.UtcNow - lastAction;

//                    if (diffTime.TotalMinutes >= 0)
//                    {
//                        if (diffTime.TotalMinutes > 15)
//                        {
//                            isOnline = false;
//                        }
//                        else
//                        {
//                            isOnline = true;
//                        }
//                    }
//                }

//                if (CurrentActiveFilter == OnlineStatusFilter.Online)
//                {
//                    if (isOnline)
//                    {
//                        return true;
//                    }
//                }

//                if (CurrentActiveFilter == OnlineStatusFilter.NotOnline)
//                {
//                    if (!isOnline)
//                    {
//                        return true;
//                    }
//                }

//                return false;
//            }
//            catch (Exception ex)
//            {
//                Debug.LogError(ex);
//                throw;
//            }
//        }

//        private string TskListFilterToString(OnlineStatusFilter filterValue)
//        {
//            try
//            {
//                var filterValueLabel = string.Empty;

//                switch (filterValue)
//                {
//                    case OnlineStatusFilter.All:
//                        {
//                            filterValueLabel = "Все";
//                            break;
//                        }
//                    case OnlineStatusFilter.Online:
//                        {
//                            filterValueLabel = "В сети";
//                            break;
//                        }
//                    case OnlineStatusFilter.NotOnline:
//                        {
//                            filterValueLabel = "Не в сети";
//                            break;
//                        }
//                }

//                return filterValueLabel;
//            }
//            catch (Exception ex)
//            {
//                Debug.LogError(ex);
//                throw;
//            }
//        }
//    }

//}
