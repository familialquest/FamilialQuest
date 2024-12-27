using Assets.Code.Models.REST.CommonTypes;
using Code.Models;
using Code.Models.REST;
using Code.Models.REST.CommonType.Tasks;
using Code.Models.REST.Users;
using Code.Models.RoleModel;
using Code.ViewControllers.TList;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static Code.Models.TaskModel;

namespace Code.ViewControllers
{
    public class ActiveTaskListFilter : ListFilter
    {
        public TMP_Text StatusFilterLabel;
        public TMP_Text UsersFilterLabel;

        private static List<Guid> CurrentUsersActiveFilter;

        //Для контроля ситуаций, когда пользователи из активного фильтра удалены\добавлены из группы
        private static bool usersSelectedPrevAll;
        private static int userSelectedPrevCount;

        private static Dictionary<ActiveTaskFilter, bool> CurrentStatusesActiveFilter;
        private static Dictionary<ActiveTaskFilter, bool> CurrentStatusesActiveFilterDelegate;

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

            CurrentStatusesActiveFilter = new Dictionary<ActiveTaskFilter, bool>();

            CurrentStatusesActiveFilter.Add(ActiveTaskFilter.All, true);
            CurrentStatusesActiveFilter.Add(ActiveTaskFilter.InProgress, true);
            CurrentStatusesActiveFilter.Add(ActiveTaskFilter.PendingReview, true);
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
                PopupTaskStatusSelectorActiveTaskPageController taskStatusSelectorPageController = popup.GetComponent<PopupTaskStatusSelectorActiveTaskPageController>();
                CurrentStatusesActiveFilterDelegate = new Dictionary<ActiveTaskFilter, bool>(CurrentStatusesActiveFilter);
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
                    CurrentStatusesActiveFilter = new Dictionary<ActiveTaskFilter, bool>(CurrentStatusesActiveFilterDelegate);
                    CurrentStatusesActiveFilterDelegate = null;
                }

                if (CurrentStatusesActiveFilter[ActiveTaskFilter.All])
                {
                    StatusFilterLabel.text = TskListFilterToString(ActiveTaskFilter.All);
                }
                else
                {
                    if (CurrentStatusesActiveFilter[ActiveTaskFilter.InProgress])
                    {
                        StatusFilterLabel.text = TskListFilterToString(ActiveTaskFilter.InProgress);
                    }

                    if (CurrentStatusesActiveFilter[ActiveTaskFilter.PendingReview])
                    {
                        StatusFilterLabel.text = TskListFilterToString(ActiveTaskFilter.PendingReview);
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

                if (CurrentStatusesActiveFilter[ActiveTaskFilter.All])
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
                        BaseTaskStatus itemStatus = Utils.StatusFromString(dict["Status"].ToString());

                        if (CurrentStatusesActiveFilter[ActiveTaskFilter.InProgress])
                        {
                            if (FilterToActiveStatus[ActiveTaskFilter.InProgress].Contains(itemStatus))
                            {
                                resultStatus = true;
                            }
                        }

                        if (CurrentStatusesActiveFilter[ActiveTaskFilter.PendingReview])
                        {
                            if (FilterToActiveStatus[ActiveTaskFilter.PendingReview].Contains(itemStatus))
                            {
                                resultStatus = true;
                            }
                        }
                    }

                    if (!resultUser)
                    {
                        if (Guid.TryParse(dict["Executor"].ToString(), out Guid executor) && executor != Guid.Empty)
                        {
                            if (CurrentUsersActiveFilter.Contains(executor))
                            {
                                resultUser = true;
                            }                            
                        }
                        else
                        {
                            throw new Exception();
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

        private string TskListFilterToString(ActiveTaskFilter filterValue)
        {
            try
            {
                var filterValueLabel = string.Empty;

                switch (filterValue)
                {
                    case ActiveTaskFilter.All:
                        {
                            filterValueLabel = "Все статусы";
                            break;
                        }
                    case ActiveTaskFilter.InProgress:
                        {
                            filterValueLabel = "Выполняется";
                            break;
                        }
                    case ActiveTaskFilter.PendingReview:
                        {
                            if (CredentialHandler.Instance.CurrentUser.Role == RoleTypes.User)
                            {
                                filterValueLabel = "Ожидает проверки";
                            }
                            else
                            {
                                if (CredentialHandler.Instance.CurrentUser.Role == RoleTypes.Administrator)
                                {
                                    filterValueLabel = "Ожидает проверки";
                                }
                                else
                                {
                                    throw new Exception();
                                }
                            }
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