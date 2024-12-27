using System;
using UnityEngine;
using Ricimi;
using Code.Controllers.MessageBox;
using Code.Models.REST.CommonType.Tasks;
using Code.Controllers;
using System.Collections.Generic;
using Code.Models.REST;
using Code.Models;
using Code.Models.REST.Users;
using Newtonsoft.Json;
using System.Linq;
using Code.Models.RoleModel;
using Assets.Code.Models.REST.CommonTypes;
using Assets.Code.Models.REST.CommonTypes.Common;

namespace Code.ViewControllers
{
    [RequireComponent(typeof(TextFieldsFiller))]
    public class TaskDetailsController : MonoBehaviour
    {
        public GroupElementController DescriptionValues;
        public TaskStatusIconController TaskStatusIcon;
        public GroupElementController ButtonGroup;
        public GameObject CircleProgressBar;

        private TextFieldsFiller m_textFieldsFiller;
        private Popup m_thisPopup;

        // Start is called before the first frame update
        void Start()
        {
            try
            {
                m_thisPopup = GetComponent<Popup>();
                m_textFieldsFiller = GetComponent<TextFieldsFiller>();

                DescriptionValuesVisiblity();

                var currentStatus = Code.Models.REST.CommonType.Tasks.Utils.StatusFromString(m_textFieldsFiller.TextData["Status"].ToString());
                TaskStatusIcon.SetStatus(currentStatus);

                ButtonGroupVisiblity(currentStatus);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        private void ButtonGroupVisiblity(BaseTaskStatus currentStatus)
        {
            try
            {
                switch (currentStatus)
                {
                    case BaseTaskStatus.Created:
                        ButtonGroup.ShowOnlyGroup("DeleteEditPublish");
                        break;
                    case BaseTaskStatus.Assigned:
                        switch (CredentialHandler.Instance.CurrentUser.Role)
                        {
                            case RoleTypes.User:
                                ButtonGroup.ShowOnlyGroup("DeclineAccept");
                                break;
                            case RoleTypes.Administrator:
                                ButtonGroup.ShowOnlyGroup("Cancel");
                                break;
                        }
                        break;
                    case BaseTaskStatus.Accepted:
                    case BaseTaskStatus.InProgress:
                        switch (CredentialHandler.Instance.CurrentUser.Role)
                        {
                            case RoleTypes.User:
                                ButtonGroup.ShowOnlyGroup("DeclineComplete");
                                break;
                            case RoleTypes.Administrator:
                                ButtonGroup.ShowOnlyGroup("Cancel");
                                break;
                        }
                        break;
                    case BaseTaskStatus.Completed:
                    case BaseTaskStatus.PendingReview:
                        switch (CredentialHandler.Instance.CurrentUser.Role)
                        {
                            case RoleTypes.User:
                                ButtonGroup.ShowOnlyGroup("NoButtons");
                                break;
                            case RoleTypes.Administrator:
                                ButtonGroup.ShowOnlyGroup("FailRedoSuccess");
                                break;
                        }
                        break;

                    case BaseTaskStatus.Declined:
                    case BaseTaskStatus.AvailableUntilPassed:
                    case BaseTaskStatus.SolutionTimeOver:
                    case BaseTaskStatus.Successed:
                    case BaseTaskStatus.Closed:
                    case BaseTaskStatus.Deleted:
                    case BaseTaskStatus.Canceled:
                    case BaseTaskStatus.Failed:
                    case BaseTaskStatus.None:
                        switch (CredentialHandler.Instance.CurrentUser.Role)
                        {
                            case RoleTypes.User:
                                ButtonGroup.ShowOnlyGroup("NoButtons");
                                break;
                            case RoleTypes.Administrator:
                                ButtonGroup.ShowOnlyGroup("Renew");
                                break;
                        }
                        break;

                    default:
                        ButtonGroup.ShowOnlyGroup("NoButtons");
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        private void DescriptionValuesVisiblity()
        {
            try
            {
                //DescriptionValues.SetVisible("Cost", m_textFieldsFiller.TextData.ContainsKey("Cost") && !string.IsNullOrEmpty(m_textFieldsFiller.TextData["Cost"]));
                DescriptionValues.SetVisible("PenaltyLabelFormal", m_textFieldsFiller.TextData.ContainsKey("PenaltyLabelFormal") && !string.IsNullOrEmpty(m_textFieldsFiller.TextData["PenaltyLabelFormal"]));
                //DescriptionValues.SetVisible("AvailableUntil", m_textFieldsFiller.TextData.ContainsKey("AvailableUntil") && !string.IsNullOrEmpty(m_textFieldsFiller.TextData["AvailableUntil"]));
                DescriptionValues.SetVisible("SolutionTime", m_textFieldsFiller.TextData.ContainsKey("SolutionTime") && !string.IsNullOrEmpty(m_textFieldsFiller.TextData["SolutionTime"]));

                DescriptionValues.SetVisible("ExecutorLabel", m_textFieldsFiller.TextData.ContainsKey("ExecutorLabel") && !string.IsNullOrEmpty(m_textFieldsFiller.TextData["ExecutorLabel"]));
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        //public void OnButton_Decline()
        //{
        //    if (Guid.TryParse(m_textFieldsFiller.TextData["Id"], out Guid taskId))
        //        ChangeTaskStatus(taskId, BaseTaskStatus.Declined);
        //    else
        //        Debug.LogError("Can't parse GUID");
        //}

        public void OnButton_Accept()
        {
            try
            {
                Global_MessageBoxHandlerController.ShowMessageBox("Выполнение задания", "Начнётся выполнение задания.\n\nПродолжить?", MessageBoxType.Information, MessageBoxButtonsType.OkCancel)
                        .Then((dialogRes) =>
                        {
                            if (dialogRes == MessageBoxResult.Ok)
                            {
                                CircleProgressBar.SetActive(true);

                                if (Guid.TryParse(m_textFieldsFiller.TextData["Id"], out Guid taskId))
                                {
                                    ChangeTaskStatus(taskId, BaseTaskStatus.Accepted);
                                }
                                else
                                {
                                    CircleProgressBar.SetActive(false);
                                    Debug.LogError("Can't parse GUID");
                                }
                            }
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

        public void OnButton_Decline()
        {
            try
            {
                Global_MessageBoxHandlerController.ShowMessageBox("Отказ от задания", "Задание будет отклонено\n(при наличии штрафа - выполнится списание).\n\nПродолжить?", MessageBoxType.Information, MessageBoxButtonsType.OkCancel)
                       .Then((dialogRes) =>
                       {
                           if (dialogRes == MessageBoxResult.Ok)
                           {
                               CircleProgressBar.SetActive(true);

                               if (Guid.TryParse(m_textFieldsFiller.TextData["Id"], out Guid taskId))
                               {
                                   ChangeTaskStatus(taskId, BaseTaskStatus.Declined);
                               }
                               else
                               {
                                   CircleProgressBar.SetActive(false);
                                   Debug.LogError("Can't parse GUID");
                               }
                           }
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

        public void OnButton_Cancel()
        {
            try
            {
                Global_MessageBoxHandlerController.ShowMessageBox("Отмена задания", "Задание будет отменено\n(при налчии исполнителя - будет перечислено полное вознаграждение).\n\nПродолжить?", MessageBoxType.Information, MessageBoxButtonsType.OkCancel)
                        .Then((dialogRes) =>
                        {
                            if (dialogRes == MessageBoxResult.Ok)
                            {
                                CircleProgressBar.SetActive(true);

                                if (Guid.TryParse(m_textFieldsFiller.TextData["Id"], out Guid taskId))
                                {
                                    ChangeTaskStatus(taskId, BaseTaskStatus.Canceled);
                                }
                                else
                                {
                                    CircleProgressBar.SetActive(false);
                                    Debug.LogError("Can't parse GUID");
                                }
                            }
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

        public void OnButton_Complete()
        {
            try
            {
                Global_MessageBoxHandlerController.ShowMessageBox("Завершение задания", "Задание будет отправлено на проверку.\n\nПродолжить?", MessageBoxType.Information, MessageBoxButtonsType.OkCancel)
                       .Then((dialogRes) =>
                       {
                           if (dialogRes == MessageBoxResult.Ok)
                           {
                               CircleProgressBar.SetActive(true);

                               if (Guid.TryParse(m_textFieldsFiller.TextData["Id"], out Guid taskId))
                               {
                                   ChangeTaskStatus(taskId, BaseTaskStatus.Completed);
                               }
                               else
                               {
                                   CircleProgressBar.SetActive(false);
                                   Debug.LogError("Can't parse GUID");
                               }
                           }
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

        public void OnButton_Delete()
        {
            try
            {
                Global_MessageBoxHandlerController.ShowMessageBox("Удаление задания", "Задание будет удалено.\n\nПродолжить?", MessageBoxType.Information, MessageBoxButtonsType.OkCancel)
                       .Then((dialogRes) =>
                       {
                           if (dialogRes == MessageBoxResult.Ok)
                           {
                               CircleProgressBar.SetActive(true);

                               if (Guid.TryParse(m_textFieldsFiller.TextData["Id"], out Guid taskId))
                               {
                                   ChangeTaskStatus(taskId, BaseTaskStatus.Deleted);
                               }
                               else
                               {
                                   CircleProgressBar.SetActive(false);
                                   Debug.LogError("Can't parse GUID");
                               }
                           }
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

        public void OnButton_Edit(PopupOpener editPopupOpener)
        {
            try
            {
                editPopupOpener.OpenPopup(out var popup);
                CreateEditTaskPageController editTaskPageController = popup.GetComponent<CreateEditTaskPageController>();
                editTaskPageController.SetEditMode(CreateEditTaskPageController.ModeType.Edit);
                editTaskPageController.SetData(m_textFieldsFiller.Data);
                editTaskPageController.AfterTaskChangedDelegate = AfterTaskEdited;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        public void OnButton_Renew(PopupOpener editPopupOpener)
        {
            try
            {
                editPopupOpener.OpenPopup(out var popup);
                CreateEditTaskPageController editTaskPageController = popup.GetComponent<CreateEditTaskPageController>();
                editTaskPageController.SetEditMode(CreateEditTaskPageController.ModeType.Renew);
                editTaskPageController.SetData(m_textFieldsFiller.Data);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        public void OnButton_Publish()
        {
            try
            {
                //TODO: darkmagic - заменить AvailableUntil на SolutionTime
                if (m_textFieldsFiller.Data.TryGetValue("AvailableUntil", out object value))
                {
                    if (DateTime.TryParse(value.ToString(), out DateTime dateTimeValue) && (dateTimeValue != CommonData.dateTime_FQDB_MinValue))
                    {
                        if ((dateTimeValue - DateTime.UtcNow).TotalMinutes < 10)
                        {
                            Global_MessageBoxHandlerController.ShowMessageBox("Ещё одна деталь", "Минимальный срок завершения задания: 10 минут.", MessageBoxType.Information);
                            return;
                        }
                    }
                }

                Global_MessageBoxHandlerController.ShowMessageBox("Новое задание", "Задание будет объявлено всем указанным Героям\n(первый вызвавшийся Герой получит задание).\n\nПродолжить?", MessageBoxType.Information, MessageBoxButtonsType.OkCancel)
                       .Then((dialogRes) =>
                       {
                           if (dialogRes == MessageBoxResult.Ok)
                           {
                               CircleProgressBar.SetActive(true);

                               if (Guid.TryParse(m_textFieldsFiller.TextData["Id"], out Guid taskId))
                               {
                                   ChangeTaskStatus(taskId, BaseTaskStatus.Assigned);
                               }
                               else
                               {
                                   CircleProgressBar.SetActive(false);
                                   Debug.LogError("Can't parse GUID");
                               }
                           }
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

        public void OnButton_Fail()
        {
            try
            {
                Global_MessageBoxHandlerController.ShowMessageBox("Завершение задания", "Задание будет провалено\n(при наличии штрафа - у исполнителя будет выполено списание).\n\nПродолжить?", MessageBoxType.Information, MessageBoxButtonsType.OkCancel)
                       .Then((dialogRes) =>
                       {
                           if (dialogRes == MessageBoxResult.Ok)
                           {
                               CircleProgressBar.SetActive(true);

                               if (Guid.TryParse(m_textFieldsFiller.TextData["Id"], out Guid taskId))
                               {
                                   ChangeTaskStatus(taskId, BaseTaskStatus.Failed);
                               }
                               else
                               {
                                   CircleProgressBar.SetActive(false);
                                   Debug.LogError("Can't parse GUID");
                               }
                           }
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

        public void OnButton_Redo(PopupOpener editPopupOpener)
        {
            try
            {
                Global_MessageBoxHandlerController.ShowMessageBox("Доработка задания", "Задание будет возвращено в работу исполнителю.\n\nПродолжить?", MessageBoxType.Information, MessageBoxButtonsType.OkCancel)
                    .Then((dialogRes_Redo) =>
                    {
                        if (dialogRes_Redo == MessageBoxResult.Ok)
                        {
                            //TODO: darkmagic - заменить AvailableUntil на SolutionTime
                            if (m_textFieldsFiller.Data.TryGetValue("AvailableUntil", out object value) &&
                                DateTime.TryParse(value.ToString(), out DateTime dateTimeValue) &&
                                (dateTimeValue != CommonData.dateTime_FQDB_MinValue) &&
                                (dateTimeValue - DateTime.UtcNow).TotalMinutes < 10)
                            {
                                Global_MessageBoxHandlerController.ShowMessageBox("Доработка задания", "Для продолжения необходимо установить новый срок завершения.", MessageBoxType.Information, MessageBoxButtonsType.Ok)
                                    .Then((dialogRes) =>
                                    {
                                        if (dialogRes == MessageBoxResult.Ok)
                                        {
                                            editPopupOpener.OpenPopup(out var popup);
                                            CreateEditTaskPageController editTaskPageController = popup.GetComponent<CreateEditTaskPageController>();
                                            editTaskPageController.SetEditMode(CreateEditTaskPageController.ModeType.RedoUpdate);
                                            editTaskPageController.SetData(m_textFieldsFiller.Data);
                                            editTaskPageController.AfterTaskChangedDelegate = AfterRedoTaskEdited;
                                        }
                                        else
                                        {
                                            return;
                                        }
                                    })
                                    .Catch((ex) =>
                                    {
                                        Debug.LogError(ex);

                                        CircleProgressBar.SetActive(false);

                                        FQServiceException.ShowExceptionMessage(ex);
                                    });
                            }
                            else
                            {
                                Global_MessageBoxHandlerController.ShowMessageBox("Доработка задания", "Установить новый срок завершения задания или внести правки в описание перед возвратом в работу?", MessageBoxType.Information, MessageBoxButtonsType.YesNo)
                                    .Then((dialogRes) =>
                                    {
                                        if (dialogRes == MessageBoxResult.Yes)
                                        {
                                            editPopupOpener.OpenPopup(out var popup);
                                            CreateEditTaskPageController editTaskPageController = popup.GetComponent<CreateEditTaskPageController>();
                                            editTaskPageController.SetEditMode(CreateEditTaskPageController.ModeType.RedoUpdate);
                                            editTaskPageController.SetData(m_textFieldsFiller.Data);
                                            editTaskPageController.AfterTaskChangedDelegate = AfterRedoTaskEdited;
                                        }
                                        else
                                        {
                                            DoRedo();
                                        }
                                    })
                                    .Catch((ex) =>
                                    {
                                        Debug.LogError(ex);

                                        CircleProgressBar.SetActive(false);

                                        FQServiceException.ShowExceptionMessage(ex);
                                    });
                            }

                        }
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

        private void DoRedo()
        {
            CircleProgressBar.SetActive(true);

            if (Guid.TryParse(m_textFieldsFiller.TextData["Id"], out Guid taskId))
            {
                ChangeTaskStatus(taskId, BaseTaskStatus.InProgress);
            }
            else
            {
                CircleProgressBar.SetActive(false);
                Debug.LogError("Can't parse GUID");
            }
        }

        public void OnButton_Success()
        {
            try
            {
                Global_MessageBoxHandlerController.ShowMessageBox("Завершение задания", "Задание будет успешно завершено\n(выполнится начисление вознаграждения исполнителю).\n\nПродолжить?", MessageBoxType.Information, MessageBoxButtonsType.OkCancel)
                       .Then((dialogRes) =>
                       {
                           if (dialogRes == MessageBoxResult.Ok)
                           {
                               CircleProgressBar.SetActive(true);

                               if (Guid.TryParse(m_textFieldsFiller.TextData["Id"], out Guid taskId))
                               {
                                   ChangeTaskStatus(taskId, BaseTaskStatus.Successed);
                               }
                               else
                               {
                                   CircleProgressBar.SetActive(false);
                                   Debug.LogError("Can't parse GUID");
                               }
                           }
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

        private void ChangeTaskStatus(Guid taskId, BaseTaskStatus status)
        {
            try
            {
                TaskLogicController.ChangeStatus(taskId, (int)status)
                        .Then((res) =>
                        {
                            if (res.result)
                            {
                                if (status == BaseTaskStatus.Successed ||
                                    status == BaseTaskStatus.Failed ||
                                    status == BaseTaskStatus.Declined)
                                {
                                    //Обновим в фоне чтобы актуализировались монетки
                                    DataModel.Instance.Credentials.UpdateAllCredentials();
                                }

                                // закрыть это окно по удалению
                                m_thisPopup.Close();
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

        private void AfterTaskEdited(Dictionary<string, object> newProps)
        {
            try
            {
                m_textFieldsFiller.SetData(newProps);

                DescriptionValuesVisiblity();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        private void AfterRedoTaskEdited(Dictionary<string, object> newProps)
        {
            try
            {
                m_textFieldsFiller.SetData(newProps);

                DescriptionValuesVisiblity();

                DoRedo();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        public void OnClick_ButtonSelectUser(Ricimi.PopupOpener editPopupOpener)
        {
            try
            {
                SetActualSelectedUsers();

                editPopupOpener.OpenPopup(out var popup);
                PopupUserSelectorPageController userSelectorPageController = popup.GetComponent<PopupUserSelectorPageController>();
                userSelectorPageController.readOnlyMode = true;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                FQServiceException.ShowExceptionMessage(ex);
            }
        }

        private void SetActualSelectedUsers()
        {
            try
            {
                foreach (var user in DataModel.Instance.Credentials.ChildrenUsers)
                {
                    user.Selected = false;
                }

                List<User> destinationUsers = new List<User>();

                List<Guid> destinationUsersIds = new List<Guid>();

                destinationUsersIds = JsonConvert.DeserializeObject<List<Guid>>(m_textFieldsFiller.TextData["AvailableFor"]);

                //Пустой AvailableFor = доступ для всех детей группы
                if (destinationUsersIds.Count > 0)
                {
                    destinationUsers = DataModel.Instance.Credentials.ChildrenUsers.Where(x => destinationUsersIds.Contains(x.Id)).ToList();
                }
                else
                {
                    destinationUsers = DataModel.Instance.Credentials.ChildrenUsers;
                }
                //---                      

                foreach (var user in destinationUsers)
                {
                    user.Selected = true;
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