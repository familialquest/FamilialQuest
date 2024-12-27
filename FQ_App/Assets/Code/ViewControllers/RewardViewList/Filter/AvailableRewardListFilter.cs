
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
using static Code.Models.RewardModel;
using static Code.Models.TaskModel;

namespace Code.ViewControllers
{
    public class AvailableRewardListFilter : ListFilter
    {
        public TMP_Text StatusFilterLabel;
        public TMP_Text UsersFilterLabel;

        private static List<Guid> CurrentUsersActiveFilter;

        //Для контроля ситуаций, когда пользователи из активного фильтра удалены\добавлены из группы
        private static bool usersSelectedPrevAll;
        private static int userSelectedPrevCount;

        private static Dictionary<AvailableRewardFilter, bool> CurrentStatusesActiveFilter;
        private static Dictionary<AvailableRewardFilter, bool> CurrentStatusesActiveFilterDelegate;

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
            CurrentUsersActiveFilter = new List<Guid>(DataModel.Instance.Credentials.ChildrenUsers.Select(x => x.Id));
            usersSelectedPrevAll = false;
            userSelectedPrevCount = CurrentUsersActiveFilter.Count;
            
            CurrentStatusesActiveFilter = new Dictionary<AvailableRewardFilter, bool>();

            CurrentStatusesActiveFilter.Add(AvailableRewardFilter.All, true);
            CurrentStatusesActiveFilter.Add(AvailableRewardFilter.CanBeBought, true);
            CurrentStatusesActiveFilter.Add(AvailableRewardFilter.CanNotBeBought, true);
        }

        public void OnClick_ButtonChangeUserFilter(Ricimi.PopupOpener editPopupOpener)
        {
            try
            {
                SetActualSelectedUsers();

                editPopupOpener.OpenPopup(out var popup);
                PopupUserSelectorPageController userSelectorPageController = popup.GetComponent<PopupUserSelectorPageController>();
                userSelectorPageController.AfterCredentialsSelectedDelegate = AfterCredentialsSelected;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                FQServiceException.ShowExceptionMessage(ex);
            }
        }

        public void OnClick_ButtonChangeStatusFilter(Ricimi.PopupOpener editPopupOpener)
        {
            try
            {
                editPopupOpener.OpenPopup(out var popup);
                PopupRewardStatusSelectorAvailableRewardTaskPageController taskStatusSelectorPageController = popup.GetComponent<PopupRewardStatusSelectorAvailableRewardTaskPageController>();
                CurrentStatusesActiveFilterDelegate = new Dictionary<AvailableRewardFilter, bool>(CurrentStatusesActiveFilter);
                taskStatusSelectorPageController.SetValues(CurrentStatusesActiveFilterDelegate);
                taskStatusSelectorPageController.AfterTaskStatusSelectedDelegate = AfterTaskStatusSelected;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                FQServiceException.ShowExceptionMessage(ex);
            }
        }

        public void SetActualSelectedUsers()
        {
            try
            {
                if (CurrentStatusesActiveFilter == null || CurrentUsersActiveFilter == null)
                {
                    ResetFilters();
                }

                if (usersSelectedPrevAll)
                {
                    //Если были выбраны "Все герои", не важно, кого добавили или удалили в группу - здесь селект всех

                    foreach (var user in DataModel.Instance.Credentials.ChildrenUsers)
                    {
                        user.Selected = true;
                    }

                    List<User> destinationUsers = new List<User>(DataModel.Instance.Credentials.ChildrenUsers); 
                }
                else
                {
                    //Нужно чекнуть, не был ли удален из группы, выбранный в фильтре пользователь

                    foreach (var user in DataModel.Instance.Credentials.ChildrenUsers)
                    {
                        user.Selected = true;
                    }

                    List<User> destinationUsers = new List<User>();

                    destinationUsers = DataModel.Instance.Credentials.ChildrenUsers.Where(x => CurrentUsersActiveFilter.Contains(x.Id)).ToList();

                    foreach (var user in DataModel.Instance.Credentials.ChildrenUsers)
                    {
                        if (!destinationUsers.Contains(user))
                        {
                            user.Selected = false;
                        }
                    }
                }

                AfterCredentialsSelected();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        private void AfterCredentialsSelected()
        {
            try
            {
                if (CurrentStatusesActiveFilter == null || CurrentUsersActiveFilter == null)
                {
                    ResetFilters();
                }

                UsersFilterLabel.text = "Все герои";
                CurrentUsersActiveFilter.Clear();

                var selectedUsers = DataModel.Instance.Credentials.ChildrenUsers.Where(x => x.Selected);

                var destinationUsersNames = new List<string>();

                if (selectedUsers.Count() > 0)
                {
                    foreach (var selectedUser in selectedUsers)
                    {
                        CurrentUsersActiveFilter.Add(selectedUser.Id);
                    }

                    //Отображение
                    destinationUsersNames = new List<string>(selectedUsers.Select(x1 => x1.Name).OrderBy(x2 => x2));

                    if (destinationUsersNames.Count() == 1 &&
                        DataModel.Instance.Credentials.ChildrenUsers.Count != 1)
                    {
                        UsersFilterLabel.text = destinationUsersNames.First();

                        TextFieldsFiller.TruncateAvailableUsers(UsersFilterLabel, null, destinationUsersNames.Count());
                    }
                    else
                    {
                        if (destinationUsersNames.Count() == DataModel.Instance.Credentials.ChildrenUsers.Count)
                        {
                            UsersFilterLabel.text = "Все герои";
                        }
                        else
                        {
                            UsersFilterLabel.text = string.Join(", ", destinationUsersNames);

                            TextFieldsFiller.TruncateAvailableUsers(UsersFilterLabel, null, destinationUsersNames.Count());
                        }
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

        public void AfterTaskStatusSelected()
        {
            try
            {
                if (CurrentStatusesActiveFilter == null || CurrentUsersActiveFilter == null)
                {
                    ResetFilters();
                }

                if (CurrentStatusesActiveFilterDelegate != null)
                {
                    CurrentStatusesActiveFilter = new Dictionary<AvailableRewardFilter, bool>(CurrentStatusesActiveFilterDelegate);
                    CurrentStatusesActiveFilterDelegate = null;
                }

                if (CurrentStatusesActiveFilter[AvailableRewardFilter.All])
                {
                    StatusFilterLabel.text = TskListFilterToString(AvailableRewardFilter.All);
                }
                else
                {
                    if (CurrentStatusesActiveFilter[AvailableRewardFilter.CanBeBought])
                    {
                        StatusFilterLabel.text = TskListFilterToString(AvailableRewardFilter.CanBeBought);
                    }

                    if (CurrentStatusesActiveFilter[AvailableRewardFilter.CanNotBeBought])
                    {
                        StatusFilterLabel.text = TskListFilterToString(AvailableRewardFilter.CanNotBeBought);
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
                if (CurrentStatusesActiveFilter == null || CurrentUsersActiveFilter == null)
                {
                    ResetFilters();
                }

                bool resultStatus = false;
                bool resultUser = false;

                if (!(item is Dictionary<string, object>))
                    return false;

                if (CurrentStatusesActiveFilter[AvailableRewardFilter.All])
                {
                    resultStatus = true;
                }

                if (CurrentUsersActiveFilter.Count == 0)
                {
                    resultUser = true;
                }

                if (resultStatus && resultUser)
                {
                    return true;
                }
                else
                {
                    Dictionary<string, object> dict = (Dictionary<string, object>)item;

                    //TODO: зарезервировано под НОВЫЕ
                    //if (CurrentActiveFilter == AdminAvailableTaskFilter.Fresh)
                    //{
                    //    DateTime itemDate = DateTime.Parse(dict["ModificationTime"].ToString());

                    //    if (DateTime.Now - itemDate < TimeSpan.FromHours(1))
                    //        return true;
                    //}
                    //else

                    if (!resultStatus)
                    {
                        BaseRewardStatus itemStatus = (BaseRewardStatus)Enum.Parse(typeof(BaseRewardStatus), dict["Status"].ToString());

                        if (itemStatus == BaseRewardStatus.Registered)
                        {
                            var destinationUser = DataModel.Instance.Credentials.Users.Where(x => x.Id == Guid.Parse(dict["AvailableFor"].ToString())).FirstOrDefault();
                            var cost = Convert.ToInt32(dict["Cost"]);

                            if (CurrentStatusesActiveFilter[AvailableRewardFilter.CanBeBought])
                            {
                                if (destinationUser.Coins >= cost)
                                {
                                    resultStatus = true;
                                }
                            }

                            if (CurrentStatusesActiveFilter[AvailableRewardFilter.CanNotBeBought])
                            {
                                if (destinationUser.Coins < cost)
                                {
                                    resultStatus = true;
                                }
                            }
                        }
                    }

                    if (!resultUser)
                    {
                        if (Guid.TryParse(dict["AvailableFor"].ToString(), out Guid availableFor) && availableFor != Guid.Empty)
                        {
                            if (CurrentUsersActiveFilter.Contains(availableFor))
                            {
                                resultUser = true;
                            }
                        }                      
                    }

                    if (resultStatus && resultUser)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        private string TskListFilterToString(AvailableRewardFilter filterValue)
        {
            try
            {
                var filterValueLabel = string.Empty;

                switch (filterValue)
                {
                    case AvailableRewardFilter.All:
                        {
                            filterValueLabel = "Все статусы";
                            break;
                        }
                    case AvailableRewardFilter.CanBeBought:
                        {
                            filterValueLabel = "Доступно";
                            break;
                        }
                    case AvailableRewardFilter.CanNotBeBought:
                        {
                            filterValueLabel = "Недостаточно монет";
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