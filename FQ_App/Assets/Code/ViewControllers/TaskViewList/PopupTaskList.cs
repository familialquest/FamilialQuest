using Code.Models;
using System;
using TMPro;
using UnityEngine;
using Code.ViewControllers.TList;
using Ricimi;
using Code.Controllers.MessageBox;
using UnityEngine.SceneManagement;
using Code.Models.REST;
using Assets.Code.Models.REST.CommonTypes;
using System.Globalization;
using static Code.Models.TaskModel;

namespace Code.ViewControllers
{
    public class PopupTaskList : MonoBehaviour
    {
        TListViewController m_listViewController;

        public GameObject thisForm;

        public GameObject CircleProgressBar;
        //public TextMeshProUGUI AmountText;
        public TextMeshProUGUI CoinsText;
        public TListViewController ScrollRect_AvailableTasks;
        public TListViewController ScrollRect_ActiveTasks;
        public TListViewController ScrollRect_FinishedTasks;
        public TListViewController ScrollRect_AvailableTasks_Children;
        public TListViewController ScrollRect_ActiveTasks_Children;
        public TListViewController ScrollRect_FinishedTasks_Children;

        public BaseTaskListFilter TypeChooser_TaskListFilter;


        private TListViewController m_currentList
        {
            get
            {
                switch (TypeChooser_TaskListFilter.CurrentActiveFilter)
                {
                    case TaskModel.BaseTaskFilter.Available:
                        return ScrollRect_AvailableTasks;
                        
                    case TaskModel.BaseTaskFilter.Active:
                        return ScrollRect_ActiveTasks;

                    case TaskModel.BaseTaskFilter.Finished:
                        return ScrollRect_FinishedTasks;
                };
                return ScrollRect_AvailableTasks;
            }
        }

        // public GameObject SelectModeActions;

        //public GameObject BottomBar;
        //private TextMeshProUGUI m_bottomBarText;

        //public StatusWindowController StatusWindowController;

        // Start is called before the first frame update
        void Awake()
        {
            try
            {
                // подпишемся на изменение списка
                DataModel.Instance.Tasks.OnListChanged += Tasks_ListChanged;

                if (CredentialHandler.Instance.CurrentUser.Role == Models.RoleModel.RoleTypes.User)
                {
                    ScrollRect_AvailableTasks = ScrollRect_AvailableTasks_Children;
                    ScrollRect_ActiveTasks = ScrollRect_ActiveTasks_Children;
                    ScrollRect_FinishedTasks = ScrollRect_FinishedTasks_Children;
                }

                // подпишемся на событие требования обновления от листвью
                ScrollRect_AvailableTasks.OnNeedListRefresh += M_listViewController_NeedListRefresh;
                ScrollRect_ActiveTasks.OnNeedListRefresh += M_listViewController_NeedListRefresh;
                ScrollRect_FinishedTasks.OnNeedListRefresh += M_listViewController_NeedListRefresh;

                // если лист прежде не был получен, то сымитируем требование обновления от листа
                if (DataModel.Instance.Tasks.Tasks == null)
                    M_listViewController_NeedListRefresh(null, null);
                else
                {
                    //SetNewList(DataModel.Instance.Tasks.AvailableTasks);
                    SetNewList();
                }

                //m_listViewController.OnListChanged += M_listViewController_ListChanged;
                //m_bottomBarText = BottomBar.transform.Find("SelectedAmount").GetComponent<TextMeshProUGUI>();

                TypeChooser_TaskListFilter.OnFilterChanged += TypeChooser_TaskListFilter_OnFilterChanged;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }

            thisForm.SetActive(false);
        }

        private void TypeChooser_TaskListFilter_OnFilterChanged(object sender, EventArgs e)
        {
            try
            {
                ScrollRect_AvailableTasks.gameObject.SetActive(TypeChooser_TaskListFilter.CurrentActiveFilter == TaskModel.BaseTaskFilter.Available);
                ScrollRect_ActiveTasks.gameObject.SetActive(TypeChooser_TaskListFilter.CurrentActiveFilter == TaskModel.BaseTaskFilter.Active);
                ScrollRect_FinishedTasks.gameObject.SetActive(TypeChooser_TaskListFilter.CurrentActiveFilter == TaskModel.BaseTaskFilter.Finished);

                //if (m_currentList.Items?.Count > 0)
                //{
                //    AmountText.text = $"{m_currentList.Items?.Count}";
                //}
                //else
                //{
                //    AmountText.text = "-";
                //}
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        private void M_listViewController_ListChanged(object sender, EventArgs e)
        {
            try
            {
                //SetSelectedAmountText();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        //private void SetSelectedAmountText()
        //{
        //    //m_bottomBarText.text = m_listViewController.GetSelectedItems().Count.ToString();
        //}

        private void OnDestroy()
        {
            try
            {
                // отпишемся от изменение списка
                DataModel.Instance.Tasks.OnListChanged -= Tasks_ListChanged;
                // отпишемся от события требования обновления от листвью
                ScrollRect_AvailableTasks.OnNeedListRefresh -= M_listViewController_NeedListRefresh;
                ScrollRect_ActiveTasks.OnNeedListRefresh -= M_listViewController_NeedListRefresh;
                ScrollRect_FinishedTasks.OnNeedListRefresh -= M_listViewController_NeedListRefresh;
                // отпишемся от обновлений фильтра
                TypeChooser_TaskListFilter.OnFilterChanged -= TypeChooser_TaskListFilter_OnFilterChanged;

                AdminAvailableTaskListFilter.ResetFilters();
                ActiveTaskListFilter.ResetFilters();
                FinishedTaskListFilter.ResetFilters();

            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        private void OnEnable()
        {

        }

        private void Tasks_ListChanged(object sender, ListChangedEventArgs e)
        {
            try
            {
                var newList = e.List;
                //SetNewList(newList);
                SetNewList();
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
                DataModel.Instance.Tasks.UpdateTasks()
                    .Then((res) =>
                    {                        
                        CircleProgressBar.SetActive(false);
                    })
                    .Catch((ex) =>
                    {
                        //Если сюда попали, то ошибку уже обработали в DataModel и показали уведомление
                        //Нужно только убрать индикацию загрузки
                        CircleProgressBar.SetActive(false);
                    }); ;
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
                //m_listViewController.SetListItems(newList);
                //AmountText.text = $"{m_listViewController.Items.Count}";
                ScrollRect_AvailableTasks.SetListItems(DataModel.Instance.Tasks.AvailableTasks);
                ScrollRect_ActiveTasks.SetListItems(DataModel.Instance.Tasks.ActiveTasks);
                ScrollRect_FinishedTasks.SetListItems(DataModel.Instance.Tasks.FinishedTasks);

                //Актуализация отображаемых фильтров
                var availableFilters = GetComponentsInChildren<AdminAvailableTaskListFilter>(true);
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
                var activeFilters = GetComponentsInChildren<ActiveTaskListFilter>(true);
                foreach (var filter in activeFilters)
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
                var finishedFilters = GetComponentsInChildren<FinishedTaskListFilter>(true);
                foreach (var filter in finishedFilters)
                {                   
                    try
                    {
                        filter.SetActualSelectedUsers();
                    }
                    catch
                    {

                    }                    
                }

                //if (m_currentList.Items?.Count > 0)
                //{
                //    AmountText.text = $"{m_currentList.Items?.Count}";
                //}
                //else
                //{
                //    AmountText.text = "-";
                //}

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

        public void OnButton_CreateNewTask(PopupOpener popupCreateEditTask)
        {
            try
            {
                popupCreateEditTask.OpenPopup(out var popup);
                CreateEditTaskPageController createEditTaskPageController = popup.GetComponent<CreateEditTaskPageController>();
                createEditTaskPageController.SetEditMode(CreateEditTaskPageController.ModeType.Create);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        //public void OnClickButton_SelectMode()
        //{
        //    //SelectModeActions.GetComponent<SelectModeActionsController>().Switch();
        //    // change button icon

        //    // 
        //    m_listViewController.SwitchSelectMode();

        //    // включим показ количества выбранных элементов
        //    //BottomBar.SetActive(!BottomBar.activeInHierarchy);
        //    SetSelectedAmountText();
        //}

        //public void OnClickButton_DeleteSelected()
        //{
        //    // получение списка выделенных listItem
        //    var list = m_listViewController.GetSelectedItems();
        //    // если нет выделенных, то ничего не делаем
        //    if (list.Count == 0)
        //        return;

        //    // спросить подтверждение
        //    Global_MessageBoxHandlerController.ShowMessageBox("Confirmation", $"Delete {list.Count} selected tasks?", MessageBoxType.Information, MessageBoxButtonsType.YesNo)
        //        .Then((messageBoxRes) =>
        //        {
        //            if (messageBoxRes == MessageBoxResult.Yes)
        //            {
        //                CircleProgressBar.SetActive(true);

        //                var guidList = new List<Guid>();
        //                foreach (var item in list)
        //                {
        //                    if (Guid.TryParse(item.Data["Id"].ToString(), out Guid itemGuid))
        //                        guidList.Add(itemGuid);
        //                    else
        //                        Debug.LogError($"Can't parse guid from item.Data[id] {item.Data["id"]}");
        //                }

        //                // вызов тасклогики
        //                TaskLogicController.DeleteTask(guidList)
        //                    .Then((res) =>
        //                    {
        //                        if (res.result)
        //                        {
        //                            Global_MessageBoxHandlerController.ShowMessageBox("", $"Tasks were deleted successfully", MessageBoxType.Information);
        //                        }
        //                        else
        //                        {
        //                            Global_MessageBoxHandlerController.ShowMessageBox("Error", $"Failed to delete selected tasks {res.description} ({res.code})", MessageBoxType.Error);
        //                        }
        //                        CircleProgressBar.SetActive(false);
        //                    });
        //            }
        //        }); ;
        //}

        //public void OnClickButton_ChangeStatusSelected(int newStatus)
        //{
        //    // получение списка выделенных listItem
        //    var list = m_listViewController.GetSelectedItems();
        //    // если нет выделенных, то ничего не делаем
        //    if (list.Count == 0)
        //        return;        

        //    // спросить подтверждение
        //    Global_MessageBoxHandlerController.ShowMessageBox("Confirmation", $"Change status for {list.Count} selected tasks on {newStatus}?", MessageBoxType.Information, MessageBoxButtonsType.YesNo)
        //        .Then((messageBoxRes) =>
        //        {
        //            StatusWindowController.Close();
        //            if (messageBoxRes == MessageBoxResult.Yes)
        //            {
        //                CircleProgressBar.SetActive(true);

        //                var guidList = new List<Guid>();
        //                foreach (var item in list)
        //                {
        //                    if (Guid.TryParse(item.Data["Id"].ToString(), out Guid itemGuid))
        //                        guidList.Add(itemGuid);
        //                    else
        //                        Debug.LogError($"Can't parse guid from item.Data[id] {item.Data["id"]}");
        //                }

        //                // вызов тасклогики
        //                TaskLogicController.ChangeStatus(guidList, newStatus)
        //                    .Then((res) =>
        //                    {
        //                        if (res.result)
        //                        {
        //                            Global_MessageBoxHandlerController.ShowMessageBox("", $"Status for tasks was changed successfully", MessageBoxType.Information);
        //                        }
        //                        else
        //                        {
        //                            Global_MessageBoxHandlerController.ShowMessageBox("Error", $"Failed to change status for selected tasks. {res.description} ({res.code})", MessageBoxType.Error);
        //                        }
        //                        CircleProgressBar.SetActive(false);
        //                    });
        //            }
        //        }); ;
        //}

        //public void OnClickButton_Status()
        //{
        //    Debug.Log("OnClickButton_Status");

        //    // получение списка выделенных listItem
        //    var selected_list = m_listViewController.GetSelectedItems();
        //    // если нет выделенных, то ничего не делаем
        //    if (selected_list.Count == 0)
        //        return;

        //    // проверить, что у всех выбранных задач - один статус
        //    Code.Models.REST.Tasks.BaseTaskStatus baseTaskStatus = Code.Models.REST.Tasks.Utils.StatusFromString(selected_list[0].Data["Status"].ToString());
        //    Code.Models.REST.Tasks.BaseTaskStatus prevBaseTaskStatus = baseTaskStatus;
        //    foreach (var item in selected_list)
        //    {
        //        baseTaskStatus = Code.Models.REST.Tasks.Utils.StatusFromString(item.Data["Status"].ToString());
        //        if (prevBaseTaskStatus != baseTaskStatus)
        //            return;
        //    }

        //    // открыть/закрыть быстрое меню установки статуса
        //    if (StatusWindowController.IsOpened)
        //        StatusWindowController.Close();
        //    else
        //        StatusWindowController.Open(baseTaskStatus);
        //}


        //public void OnClickButton_SelectAll()
        //{
        //    m_listViewController.SwitchSelection();
        //    SetSelectedAmountText();
        //}
    }

}