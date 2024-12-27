﻿using Assets.Code.Models.REST.CommonTypes;
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
using static Code.Models.TaskModel;

namespace Code.ViewControllers
{
    public class AdminAvailableTaskListFilter : ListFilter
    {
        public TMP_Text StatusFilterLabel;
        public TMP_Text UsersFilterLabel;

        private static List<Guid> CurrentUsersActiveFilter;

        //Для контроля ситуаций, когда пользователи из активного фильтра удалены\добавлены из группы
        private static bool usersSelectedPrevAll;
        private static int userSelectedPrevCount;

        private static Dictionary<AdminAvailableTaskFilter, bool> CurrentStatusesActiveFilter;
        private static Dictionary<AdminAvailableTaskFilter, bool> CurrentStatusesActiveFilterDelegate;

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

            CurrentStatusesActiveFilter = new Dictionary<AdminAvailableTaskFilter, bool>();

            CurrentStatusesActiveFilter.Add(AdminAvailableTaskFilter.All, true);
            CurrentStatusesActiveFilter.Add(AdminAvailableTaskFilter.Draft, true);
            CurrentStatusesActiveFilter.Add(AdminAvailableTaskFilter.Announced, true);
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
                PopupTaskStatusSelectorAdminAvailableTaskPageController taskStatusSelectorPageController = popup.GetComponent<PopupTaskStatusSelectorAdminAvailableTaskPageController>();
                CurrentStatusesActiveFilterDelegate = new Dictionary<AdminAvailableTaskFilter, bool>(CurrentStatusesActiveFilter);
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
                    CurrentStatusesActiveFilter = new Dictionary<AdminAvailableTaskFilter, bool>(CurrentStatusesActiveFilterDelegate);
                    CurrentStatusesActiveFilterDelegate = null;
                }

                if (CurrentStatusesActiveFilter[AdminAvailableTaskFilter.All])
                {
                    StatusFilterLabel.text = TskListFilterToString(AdminAvailableTaskFilter.All);
                }
                else
                {
                    if (CurrentStatusesActiveFilter[AdminAvailableTaskFilter.Draft])
                    {
                        StatusFilterLabel.text = TskListFilterToString(AdminAvailableTaskFilter.Draft);
                    }

                    if (CurrentStatusesActiveFilter[AdminAvailableTaskFilter.Announced])
                    {
                        StatusFilterLabel.text = TskListFilterToString(AdminAvailableTaskFilter.Announced);
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

                if (CurrentStatusesActiveFilter[AdminAvailableTaskFilter.All])
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

                        if (CurrentStatusesActiveFilter[AdminAvailableTaskFilter.Draft])
                        {
                            if (FilterToAdminAvailableStatus[AdminAvailableTaskFilter.Draft].Contains(itemStatus))
                            {
                                resultStatus = true;
                            }
                        }

                        if (CurrentStatusesActiveFilter[AdminAvailableTaskFilter.Announced])
                        {
                            if (FilterToAdminAvailableStatus[AdminAvailableTaskFilter.Announced].Contains(itemStatus))
                            {
                                resultStatus = true;
                            }
                        }                        
                    }

                    if (!resultUser)
                    {
                        List<Guid> AvailableFor = JsonConvert.DeserializeObject<List<Guid>>(dict["AvailableFor"].ToString());

                        if (AvailableFor.Count == 0)
                        {
                            resultUser = true;
                        }
                        else
                        {
                            foreach (var selectedUser in CurrentUsersActiveFilter)
                            {
                                if (AvailableFor.Contains(selectedUser))
                                {
                                    resultUser = true;
                                    break;
                                }
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

        private string TskListFilterToString(AdminAvailableTaskFilter filterValue)
        {
            try
            {
                var filterValueLabel = string.Empty;

                switch (filterValue)
                {
                    case AdminAvailableTaskFilter.All:
                        {
                            filterValueLabel = "Все статусы";
                            break;
                        }
                    case AdminAvailableTaskFilter.Draft:
                        {
                            filterValueLabel = "Черновик";
                            break;
                        }
                    case AdminAvailableTaskFilter.Announced:
                        {
                            filterValueLabel = "Объявлено";
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