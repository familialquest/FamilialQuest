using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ricimi;

using Code.Controllers.MessageBox;
using Code.Controllers;
using Code.Models.REST.CommonType.Tasks;
using System.Linq;
using Code.Models;
using Newtonsoft.Json;
using Code.Models.REST.Users;
using Assets.Code.Models.REST.CommonTypes;
using UnityEngine.UI;
using Assets.Code.Models.REST.CommonTypes.Common;

namespace Code.ViewControllers
{
    [RequireComponent(typeof(Popup))]
    [RequireComponent(typeof(TooltipController))]
    [RequireComponent(typeof(TextFieldsFiller))]
    [RequireComponent(typeof(TaskPresenter))]
    public class CreateEditTaskPageController : MonoBehaviour
    {
        public GroupElementController ButtonGroup;
        public GroupElementController DescriptionValues;
        public GroupToggleController Toogles;
        public GameObject Placeholder_AvailableUsers;
        public GameObject Placeholder_SolutionTime;

        public GameObject CircleProgressBar;

        private Popup m_thisPopup;
        private TooltipController m_tooltipController;
        private TextFieldsFiller m_textFieldsFiller;
        private ModeType m_editMode = ModeType.Create;

        private DateTime selectedSolutionTime = DateTime.MinValue;
        private DateTime selectedAvailableUntil = DateTime.MinValue;

        public enum ModeType
        {
            Create = 0,
            Edit = 1,
            Renew = 2,
            RedoUpdate = 3
        }

        private List<Guid> availableFor = new List<Guid>();

        public delegate void AfterTaskChanged(Dictionary<string, object> newTaskProps);
        private AfterTaskChanged m_afterTaskChangedDelegate = null;
        public AfterTaskChanged AfterTaskChangedDelegate { get => m_afterTaskChangedDelegate; set => m_afterTaskChangedDelegate = value; }

        void Awake()
        {
            try
            {
                m_thisPopup = GetComponent<Popup>();
                m_tooltipController = GetComponent<TooltipController>();
                m_textFieldsFiller = GetComponent<TextFieldsFiller>();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        private void Start()
        {
            try
            {
                SetActualSelectedUsers(true);

                switch (m_editMode)
                {
                    case ModeType.Create:
                    case ModeType.Renew:
                        ButtonGroup.ShowOnlyGroup("DraftPublish");
                        break;
                    case ModeType.Edit:
                        ButtonGroup.ShowOnlyGroup("ApplyCancel");
                        break;
                    case ModeType.RedoUpdate:

                        //Для сохранения "целостности" задания разрешим установить\менять только описание и срок завершения
                        var inputFields = transform.GetComponentsInChildren<TMP_InputField>();
                        foreach (var inputField in inputFields)
                        {
                            if (!inputField.name.Contains("Description") &&
                                !inputField.name.Contains("SolutionTime"))
                            {
                                inputField.interactable = false;
                            }
                        }

                        var toggles = transform.GetComponentsInChildren<Toggle>();
                        foreach (var toggle in toggles)
                        {
                            if (!toggle.name.Contains("SolutionTime"))
                            {
                                toggle.interactable = false;
                            }
                        }

                        ButtonGroup.ShowOnlyGroup("ApplyCancel");
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        public void ValidateNotEmpty(TMP_InputField inputField)
        {
            try
            {
                string text = inputField.text;

                if (string.IsNullOrWhiteSpace(text))
                {
                    ShowExclamation(inputField, true);
                }
                else
                {
                    ShowExclamation(inputField, false);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        public void SetData(Dictionary<string, object> data)
        {
            try
            {
                m_textFieldsFiller.SetData(data);                

                TogglesState();

                //Если указан SolutionTime - нужно перегнать в корректный текстовый вид
                //TODO: darkmagic - заменить AvailableUntil на SolutionTime
                if (m_textFieldsFiller.Data.TryGetValue("AvailableUntil", out object value))
                {
                    if (DateTime.TryParse(value.ToString(), out DateTime dateTimeValue) && (dateTimeValue != CommonData.dateTime_FQDB_MinValue))
                    {
                        AfterSolutionTimeChangedDelegate(dateTimeValue.ToLocalTime(), DateTimePickerController.GetTextFromDate(dateTimeValue.ToLocalTime()));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        public void SetEditMode(ModeType mode)
        {
            try
            {
                m_editMode = mode;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        public void OnClick_ButtonCreate()
        {
            try
            {
                if (!ReadForm(out var taskProps))
                {
                    throw new FQServiceException(FQServiceException.FQServiceExceptionType.EmptyRequiredField);
                }

                if (taskProps.ContainsKey("Cost"))
                {
                    if (!VerifyCost(taskProps["Cost"]))
                    {
                        Global_MessageBoxHandlerController.ShowMessageBox("Ещё одна деталь", "Минимальный размер вознаграждения: 1 монета.", MessageBoxType.Information);
                        return;
                    }
                }

                if (taskProps.ContainsKey("Penalty"))
                {
                    if (!VerifyPenalty(taskProps["Penalty"]))
                    {
                        Global_MessageBoxHandlerController.ShowMessageBox("Ещё одна деталь", "Минимальный размер штрафа:\n1 монета.", MessageBoxType.Information);
                        return;
                    }
                }

                //SolutionTimeOver
                var solutionTimeToggle = Toogles.Toggles.Where(x => x.name.Contains("SolutionTime")).FirstOrDefault();

                if (selectedSolutionTime != DateTime.MinValue &&
                    taskProps.ContainsKey("SolutionTimeOver") &&
                    solutionTimeToggle != null &&
                    solutionTimeToggle.isOn)                
                {
                    if ((selectedSolutionTime.ToUniversalTime() - DateTime.UtcNow).TotalMinutes < 10)
                    {
                        Global_MessageBoxHandlerController.ShowMessageBox("Ещё одна деталь", "Минимальный срок завершения задания: 10 минут.", MessageBoxType.Information);
                        return;
                    }

                    //TODO: darkmagic
                    taskProps.Remove("SolutionTimeOver");
                    taskProps["AvailableUntil"] = JsonConvert.SerializeObject(selectedSolutionTime.ToUniversalTime());
                }

                Global_MessageBoxHandlerController.ShowMessageBox("Новое задание", "Задание будет объявлено всем указанным Героям\n(первый вызвавшийся Герой получит это задание).\n\nПродолжить?", MessageBoxType.Information, MessageBoxButtonsType.OkCancel)
                    .Then((dialogRes) =>
                    {
                        if (dialogRes == MessageBoxResult.Ok)
                        {
                            CircleProgressBar.SetActive(true);


                            //AvailableFor
                            var availableFor_serialized = JsonConvert.SerializeObject(new List<Guid>());

                            //Пустой AvailableFor = доступ для всех детей группы
                            if (availableFor.Count != DataModel.Instance.Credentials.ChildrenUsers.Count ||
                                DataModel.Instance.Credentials.ChildrenUsers.Count == 1)
                            {
                                availableFor_serialized = JsonConvert.SerializeObject(availableFor);
                            }

                            taskProps["AvailableFor"] = availableFor_serialized;


                            TaskLogicController.CreateTask(taskProps)
                                .Then((res) =>
                                {
                                    Debug.Log($"status: {res.status}");

                                    if (res.result)
                                    {
                                        TaskLogicController.ChangeStatus(((CreateTaskResponse)res.ParsedResponse).CreatedTask.Id, BaseTaskStatus.Assigned)
                                            .Then((resultChangeStatus) =>
                                            {

                                                //Сброс селекта с пользаков AvailableFor
                                                foreach (var user in DataModel.Instance.Credentials.ChildrenUsers)
                                                {
                                                    user.Selected = false;
                                                }

                                                Global_MessageBoxHandlerController.ShowMessageBox("Новое задание", "Задание объявлено!");

                                                m_thisPopup.Close();

                                            })
                                            .Catch((ex) =>
                                            {
                                                //Сброс селекта с пользаков AvailableFor
                                                foreach (var user in DataModel.Instance.Credentials.ChildrenUsers)
                                                {
                                                    user.Selected = false;
                                                }

                                                Global_MessageBoxHandlerController.ShowMessageBox("Упс..", "Не удалось объявить задание.", MessageBoxType.Warning)
                                                    .Then((dialogResult) =>
                                                    {
                                                        Global_MessageBoxHandlerController.ShowMessageBox("Новый черновик", "Черновик задания сохранен!");
                                                    });
                                                

                                                m_thisPopup.Close();
                                            });
                                    }
                                    else
                                    {
                                        CircleProgressBar.SetActive(false);
                                    }
                                }).Catch((ex) =>
                                {
                                    //Если сюда попали, то ошибку уже обработали в DataModel и показали уведомление
                                    //Нужно только убрать индикацию загрузки
                                    CircleProgressBar.SetActive(false);
                                });
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

        public void OnClick_ButtonCreateDraft()
        {
            try
            {
                if (!ReadForm(out var taskProps))
                {
                    throw new FQServiceException(FQServiceException.FQServiceExceptionType.EmptyRequiredField);
                }

                if (taskProps.ContainsKey("Cost"))
                {
                    if (!VerifyCost(taskProps["Cost"]))
                    {
                        Global_MessageBoxHandlerController.ShowMessageBox("Ещё одна деталь", "Минимальный размер вознаграждения: 1 монета.", MessageBoxType.Information);
                        return;
                    }
                }

                if (taskProps.ContainsKey("Penalty"))
                {
                    if (!VerifyPenalty(taskProps["Penalty"]))
                    {
                        Global_MessageBoxHandlerController.ShowMessageBox("Ещё одна деталь", "Минимальный размер штрафа:\n1 монета.", MessageBoxType.Information);
                        return;
                    }
                }

                //SolutionTimeOver
                var solutionTimeToggle = Toogles.Toggles.Where(x => x.name.Contains("SolutionTime")).FirstOrDefault();

                if (selectedSolutionTime != DateTime.MinValue &&
                    taskProps.ContainsKey("SolutionTimeOver") &&
                    solutionTimeToggle != null &&
                    solutionTimeToggle.isOn)
                {
                    if ((selectedSolutionTime.ToUniversalTime() - DateTime.UtcNow).TotalMinutes < 10)
                    {
                        Global_MessageBoxHandlerController.ShowMessageBox("Ещё одна деталь", "Минимальный срок завершения задания: 10 минут.", MessageBoxType.Information);
                        return;
                    }

                    //TODO: darkmagic
                    taskProps.Remove("SolutionTimeOver");
                    taskProps["AvailableUntil"] = JsonConvert.SerializeObject(selectedSolutionTime.ToUniversalTime());
                }                

                Global_MessageBoxHandlerController.ShowMessageBox("Новый черновик", "Черновик задания будет доступен для редактирования и объявления в дальнейшем.\n\nПродолжить?", MessageBoxType.Information, MessageBoxButtonsType.OkCancel)
                    .Then((dialogRes) =>
                    {
                        if (dialogRes == MessageBoxResult.Ok)
                        {
                            CircleProgressBar.SetActive(true);


                            //AvailableFor
                            var availableFor_serialized = JsonConvert.SerializeObject(new List<Guid>());

                            //Пустой AvailableFor = доступ для всех детей группы
                            if (availableFor.Count != DataModel.Instance.Credentials.ChildrenUsers.Count ||
                                DataModel.Instance.Credentials.ChildrenUsers.Count == 1)
                            {
                                availableFor_serialized = JsonConvert.SerializeObject(availableFor);
                            }

                            taskProps["AvailableFor"] = availableFor_serialized;


                            TaskLogicController.CreateDraftTask(taskProps)
                                .Then((res) =>
                                {
                                    Debug.Log($"status: {res.status}");
                                    if (res.result)
                                    {
                                        //Сброс селекта с пользаков AvailableFor
                                        foreach (var user in DataModel.Instance.Credentials.ChildrenUsers)
                                        {
                                            user.Selected = false;
                                        }

                                        Global_MessageBoxHandlerController.ShowMessageBox("Новый черновик", "Черновик задания сохранен!");

                                        m_thisPopup.Close();
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

        public void OnClick_ButtonApply()
        {
            try
            {
                if (!ReadForm(out var taskProps))
                {
                    throw new FQServiceException(FQServiceException.FQServiceExceptionType.EmptyRequiredField);
                }

                if (taskProps.ContainsKey("Cost"))
                {
                    if (!VerifyCost(taskProps["Cost"]))
                    {
                        Global_MessageBoxHandlerController.ShowMessageBox("Ещё одна деталь", "Минимальный размер вознаграждения: 1 монета.", MessageBoxType.Information);
                        return;
                    }
                }

                if (taskProps.ContainsKey("Penalty"))
                {
                    if (!VerifyPenalty(taskProps["Penalty"]))
                    {
                        Global_MessageBoxHandlerController.ShowMessageBox("Ещё одна деталь", "Минимальный размер штрафа:\n1 монета.", MessageBoxType.Information);
                        return;
                    }
                }

                //SolutionTimeOver
                var solutionTimeToggle = Toogles.Toggles.Where(x => x.name.Contains("SolutionTime")).FirstOrDefault();

                if (selectedSolutionTime != DateTime.MinValue && 
                    taskProps.ContainsKey("SolutionTimeOver") && 
                    solutionTimeToggle != null && 
                    solutionTimeToggle.isOn)
                {
                    if ((selectedSolutionTime.ToUniversalTime() - DateTime.UtcNow).TotalMinutes < 10)
                    {
                        Global_MessageBoxHandlerController.ShowMessageBox("Ещё одна деталь", "Минимальный срок завершения задания: 10 минут.", MessageBoxType.Information);
                        return;
                    }

                    //TODO: darkmagic
                    taskProps.Remove("SolutionTimeOver");
                    taskProps["AvailableUntil"] = JsonConvert.SerializeObject(selectedSolutionTime.ToUniversalTime());
                }
                else
                {
                    //TODO: darkmagic
                    taskProps.Remove("SolutionTimeOver");
                    taskProps["AvailableUntil"] = JsonConvert.SerializeObject(CommonData.dateTime_FQDB_MinValue);
                }
                //---------------

                //Penalty
                var penaltyToggle = Toogles.Toggles.Where(x => x.name.Contains("Penalty")).FirstOrDefault();

                if (!taskProps.ContainsKey("Penalty") ||
                    penaltyToggle == null ||
                    !penaltyToggle.isOn)
                {
                    taskProps["Penalty"] = "0";
                }

                Global_MessageBoxHandlerController.ShowMessageBox("Сохранение изменений", "Изменения будут внесены в задание.\n\nПродолжить?", MessageBoxType.Information, MessageBoxButtonsType.OkCancel)
                    .Then((dialogRes) =>
                    {
                        if (dialogRes == MessageBoxResult.Ok)
                        {
                            CircleProgressBar.SetActive(true);

                            var availableFor_serialized = JsonConvert.SerializeObject(new List<Guid>());

                            //Пустой AvailableFor = доступ для всех детей группы
                            if (availableFor.Count != DataModel.Instance.Credentials.ChildrenUsers.Count ||
                                DataModel.Instance.Credentials.ChildrenUsers.Count == 1)
                            {
                                availableFor_serialized = JsonConvert.SerializeObject(availableFor);
                            }

                            taskProps["AvailableFor"] = availableFor_serialized;

                            //Теперь при редактировании задания отправляем текущий статус
                            //Чтобы другой родитель, который не обновил инфу и видит задание (реально находящееся на проверке) в статусе черновика, 
                            //не мог вносить в него правки
                            taskProps.Add("Status", m_textFieldsFiller.Data["Status"].ToString());

                            TaskLogicController.UpdateTask(Guid.Parse(m_textFieldsFiller.TextData["Id"]), taskProps)
                                .Then((res) =>
                                {
                                    Debug.Log($"status: {res.status}");

                                    if (res.result)
                                    {

                                        Debug.Log($"status: {res.status}");

                                        var updatedTask = DataModel.Instance.Tasks.Tasks.Where(x =>
                                        {
                                            if (x["Id"].Equals(m_textFieldsFiller.TextData["Id"]))
                                            {
                                                return true;
                                            }
                                            else
                                            {
                                                return false;
                                            }
                                        })
                                        .FirstOrDefault();

                                        if (updatedTask != null)
                                        {
                                            ReturnAndClose(updatedTask);
                                        }
                                        else
                                        {
                                            FQServiceException.ShowExceptionMessage(FQServiceException.FQServiceExceptionType.DefaultError);
                                            CircleProgressBar.SetActive(false);
                                        }
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

        public void OnClick_ButtonCancel()
        {
            try
            {
                m_thisPopup.Close();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                FQServiceException.ShowExceptionMessage(ex);
            }
        }

        public void OnClick_ButtonSelectUser(Ricimi.PopupOpener editPopupOpener)
        {
            try
            {
                //Проверка, не залочено ли поле
                var availableForInputField = transform.GetComponentsInChildren<TMP_InputField>().Where(x => x.name.Contains("AvailableFor")).FirstOrDefault();

                if (availableForInputField != null && availableForInputField.interactable)
                {
                    SetActualSelectedUsers();

                    editPopupOpener.OpenPopup(out var popup);
                    PopupUserSelectorPageController userSelectorPageController = popup.GetComponent<PopupUserSelectorPageController>();
                    userSelectorPageController.AfterCredentialsSelectedDelegate = AfterCredentialsSelected;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                FQServiceException.ShowExceptionMessage(ex);
            }
        }

        public void OnClick_ButtonSelectSolutionTime(PopupOpener dateTimePickerPopupOpener)
        {
            try
            {
                //Проверка, не залочено ли поле
                var solutionTimeInputField = transform.GetComponentsInChildren<TMP_InputField>().Where(x => x.name.Contains("SolutionTime")).FirstOrDefault();

                if (solutionTimeInputField != null && solutionTimeInputField.interactable)
                {
                    dateTimePickerPopupOpener.OpenPopup(out var popup);
                    DateTimePickerController dateTimePickerController = popup.GetComponentInChildren<DateTimePickerController>();

                    if (selectedSolutionTime != DateTime.MinValue)
                    {
                        dateTimePickerController.SetData(selectedSolutionTime);
                    }

                    dateTimePickerController.AfterTimeChangedDelegate = AfterSolutionTimeChangedDelegate;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        public void OnClick_ButtonSelectAvailableUntil(PopupOpener dateTimePickerPopupOpener)
        {
            try
            {
                dateTimePickerPopupOpener.OpenPopup(out var popup);
                DateTimePickerController dateTimePickerController = popup.GetComponentInChildren<DateTimePickerController>();

                if (selectedAvailableUntil != DateTime.MinValue)
                {
                    dateTimePickerController.SetData(selectedAvailableUntil);
                }

                dateTimePickerController.AfterTimeChangedDelegate = AfterAvailableUntilChangedDelegate;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        private bool ReadForm(out Dictionary<string, string> taskProps)
        {
            try
            {
                taskProps = new Dictionary<string, string>();
                var inputFields = transform.GetComponentsInChildren<TMP_InputField>();

                foreach (var inputField in inputFields)
                {
                    if (inputField.IsActive())
                    {
                        //Available - Здесь отображаемые имена игнорим. Реальные данные уже в availableFor
                        if (!inputField.name.Contains("AvailableFor") &&
                            !inputField.name.Contains("SolutionTime"))
                        {
                            if (!string.IsNullOrWhiteSpace(inputField.text) || inputField.name.Contains("Description"))
                            {
                                string propName = inputField.name.Replace("Input_", "");
                                taskProps.Add(propName, inputField.text.Trim());
                            }
                            else
                            {
                                return false;
                            }
                        }

                        //TODO: darkmagic
                        if (inputField.name.Contains("SolutionTime"))
                        {
                            var textArea = inputField.GetComponentInChildren<TMP_Text>();
                            if (!string.IsNullOrWhiteSpace(textArea.text))
                            {
                                string propName = inputField.name.Replace("Input_", "");
                                taskProps.Add(propName, string.Empty);
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                }

                //Если здесь пустой - значит пользователей не добавили.
                //Для "Все = пусто" - устанавливается дальше.
                if (availableFor.Count == 0)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        private bool VerifyCost(string cost)
        {
            if (Int32.TryParse(cost, out int costInt) && costInt > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool VerifyPenalty(string penalty)
        {
            if (Int32.TryParse(penalty, out int penaltytInt) && penaltytInt > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void ShowTooltip(string text)
        {
            try
            {
                if (!m_tooltipController.IsActive)
                    m_tooltipController.Show(text);
                else
                    m_tooltipController.Hide();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        private static void ShowExclamation(TMP_InputField inputField, bool show)
        {
            try
            {
                var excl = inputField.transform.Find("CImage_Exclamation");
                excl?.gameObject.SetActive(show);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        private void ReturnAndClose(Dictionary<string, object> updatedTask)
        {
            try
            {
                if (m_afterTaskChangedDelegate != null)
                    m_afterTaskChangedDelegate(updatedTask);
                m_thisPopup.Close();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        private void TogglesState()
        {
            try
            {
                Toogles.ToggleSet("Cost", m_textFieldsFiller.TextData.ContainsKey("Cost") && !string.IsNullOrEmpty(m_textFieldsFiller.TextData["Cost"]));
                Toogles.ToggleSet("Penalty", m_textFieldsFiller.TextData.ContainsKey("Penalty") && !string.IsNullOrEmpty(m_textFieldsFiller.TextData["Penalty"]));
                Toogles.ToggleSet("AvailableUntil", m_textFieldsFiller.TextData.ContainsKey("AvailableUntil") && !string.IsNullOrEmpty(m_textFieldsFiller.TextData["AvailableUntil"]));
                Toogles.ToggleSet("SolutionTime", m_textFieldsFiller.TextData.ContainsKey("SolutionTime") && !string.IsNullOrEmpty(m_textFieldsFiller.TextData["SolutionTime"]));
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
                var inputFields = transform.GetComponentsInChildren<TMP_InputField>();

                var input_availableFor = inputFields.Where(x => x.name.Equals("Input_AvailableFor")).FirstOrDefault();

                if (input_availableFor != null)
                {
                    var textComponent = input_availableFor.GetComponentsInChildren<TMP_Text>().FirstOrDefault();

                    if (textComponent != null)
                    {
                        var destinationUsersNames = new List<string>();

                        textComponent.text = string.Empty;
                        availableFor.Clear();

                        var selectedUsers = DataModel.Instance.Credentials.ChildrenUsers.Where(x => x.Selected);

                        if (selectedUsers.Count() > 0)
                        {
                            foreach (var selectedUser in selectedUsers)
                            {
                                availableFor.Add(selectedUser.Id);
                            }

                            //Отображение
                            Placeholder_AvailableUsers.SetActive(false);

                            destinationUsersNames = new List<string>(selectedUsers.Select(x1 => x1.Name).OrderBy(x2 => x2));

                            if (destinationUsersNames.Count() == 1)
                            {
                                textComponent.text = destinationUsersNames.First();
                            }
                            else
                            {
                                if (destinationUsersNames.Count() == DataModel.Instance.Credentials.ChildrenUsers.Count)
                                {
                                    textComponent.text = "Все";
                                }
                                else
                                {
                                    textComponent.text = string.Join(", ", destinationUsersNames);
                                }
                            }
                        }

                        TextFieldsFiller.TruncateAvailableUsers(textComponent, null, destinationUsersNames.Count());
                    }
                    else
                    {
                        ; //TODO
                    }
                }
                else
                {
                    ; //TODO
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }
        
        private void SetActualSelectedUsers(bool startLoad = false)
        {
            try
            {
                foreach (var user in DataModel.Instance.Credentials.ChildrenUsers)
                {
                    user.Selected = false;
                }

                List<User> destinationUsers = new List<User>();

                if (m_textFieldsFiller.TextData != null && startLoad) //Значит тут редактирование и нужно показать выбор пользаков, установленных в AvailableFor
                {
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
                }
                else
                {
                    destinationUsers = DataModel.Instance.Credentials.ChildrenUsers.Where(x => availableFor.Contains(x.Id)).ToList();
                }

                foreach (var user in destinationUsers)
                {
                    user.Selected = true;
                }

                AfterCredentialsSelected();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        private void AfterSolutionTimeChangedDelegate(DateTime selectedDT, string selectedDT_Str)
        {
            try
            {
                selectedSolutionTime = selectedDT;

                var inputFields = transform.GetComponentsInChildren<TMP_InputField>();

                var input_SolutionTimeOver = inputFields.Where(x => x.name.Equals("Input_SolutionTimeOver")).FirstOrDefault();

                if (input_SolutionTimeOver != null)
                {
                    var textComponent = input_SolutionTimeOver.GetComponentsInChildren<TMP_Text>().FirstOrDefault();

                    if (textComponent != null)
                    {
                        Placeholder_SolutionTime.SetActive(false);

                        textComponent.text = selectedDT_Str;
                    }
                    else
                    {
                        ; //TODO
                    }
                }
                else
                {
                    ; //TODO
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        private void AfterAvailableUntilChangedDelegate(DateTime selectedDT, string selectedDT_Str)
        {
            try
            {
                selectedAvailableUntil = selectedDT;

                var inputFields = transform.GetComponentsInChildren<TMP_InputField>();

                var input_AvailableUntil = inputFields.Where(x => x.name.Equals("Input_AvailableUntil")).FirstOrDefault();

                if (input_AvailableUntil != null)
                {
                    var textComponent = input_AvailableUntil.GetComponentsInChildren<TMP_Text>().FirstOrDefault();

                    if (textComponent != null)
                    {
                        textComponent.text = selectedDT_Str;
                    }
                    else
                    {
                        ; //TODO
                    }
                }
                else
                {
                    ; //TODO
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