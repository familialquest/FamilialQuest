using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ricimi;

using Code.Controllers.MessageBox;
using Code.Controllers;
using Code.Models;
using Code.Models.REST;
using Code.Models.REST.Users;
using static Code.Models.CredentialsModel;
using Code.Models.RoleModel;
using Assets.Code.Models.REST.CommonTypes;

namespace Code.ViewControllers
{
    [RequireComponent(typeof(Popup))]
    [RequireComponent(typeof(TooltipController))]
    [RequireComponent(typeof(TextFieldsFiller))]
    [RequireComponent(typeof(CredentialPresenter))]
    public class PopupCredentialCreateController : MonoBehaviour
    {
        public GroupElementController ButtonGroup;
        public GroupElementController DescriptionValues;

        //Роль
        public TMP_Text RoleLabel;
        public GameObject[] RoleIcons;
        private int currentRole;

        private Popup m_thisPopup;
        private TooltipController m_tooltipController;
        private TextFieldsFiller m_textFieldsFiller;
        private bool m_editMode = false;

        public delegate void AfterCredentialsChanged(Dictionary<string, object> newCredentialsProps);
        private AfterCredentialsChanged m_afterCredentialsChangedDelegate = null;
        public AfterCredentialsChanged AfterCredentialsChangedDelegate { get => m_afterCredentialsChangedDelegate; set => m_afterCredentialsChangedDelegate = value; }

        public GameObject CircleProgressBar;

        void Awake()
        {
            try
            {
                m_thisPopup = GetComponent<Popup>();
                m_tooltipController = GetComponent<TooltipController>();
                m_textFieldsFiller = GetComponent<TextFieldsFiller>();
                currentRole = 0;
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
                if (m_editMode)
                {
                    ButtonGroup.ShowOnlyGroup("Apply");
                    DescriptionValuesState();
                }
                else
                {
                    ButtonGroup.ShowOnlyGroup("DraftPublish");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        public void OnClick_ButtonChangeRole()
        {
            try
            {
                currentRole++;
                int current = currentRole;


                if (current >= Enum.GetNames(typeof(RoleTypes)).Length)
                {
                    currentRole = 0;
                    current = currentRole;
                }
                
                foreach (var roleIcon in RoleIcons)
                {
                    roleIcon.SetActive(false);
                }

                RoleIcons[current].SetActive(true);

                RoleLabel.text = RoleToString((RoleTypes)current);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                RoleIcons[(int)RoleTypes.User].SetActive(true);
                RoleLabel.text = RoleToString(RoleTypes.User);
            }
        }

        private string RoleToString(RoleTypes filterValue)
        {
            try
            {
                var roleLabel = string.Empty;

                switch (filterValue)
                {
                    case RoleTypes.User:
                        {
                            roleLabel = "Отряд героев";
                            break;
                        }
                    case RoleTypes.Administrator:
                        {
                            roleLabel = "Королевский двор";
                            break;
                        }
                }

                return roleLabel;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        //TODO: не актуально?
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
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        public void SetEditMode(bool isOn)
        {
            try
            {
                m_editMode = isOn;
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
                if (!ReadForm(out var userProps))
                {
                    throw new FQServiceException(FQServiceException.FQServiceExceptionType.EmptyRequiredField);
                }

                if (!Account.VerifyLoginAsFQInnerLogin(userProps["Login"], true))
                {
                    Global_MessageBoxHandlerController.ShowMessageBox("Добавление пользователя", "Имя для входа пользователя может содержать буквы латинского алфавита (a-z) и цифры (0-9).", MessageBoxType.Information, MessageBoxButtonsType.Ok);
                    return;
                }

                if (userProps["Password"].Length < 8)
                {
                    throw new FQServiceException(FQServiceException.FQServiceExceptionType.ShortPassword);
                }

                if (!userProps["Password"].Equals(userProps["PasswordConfirm"]))
                {
                    throw new FQServiceException(FQServiceException.FQServiceExceptionType.WrongPasswordEq);
                }                

                Global_MessageBoxHandlerController.ShowMessageBox("Добавление пользователя", "Будет добавлен новый пользователь\n\nПродолжить?", MessageBoxType.Information, MessageBoxButtonsType.OkCancel)
                    .Then((dialogRes) =>
                    {
                        if (dialogRes == MessageBoxResult.Ok)
                        {
                            CircleProgressBar.SetActive(true);

                            User u = new User();
                            u.Name = userProps["Name"];
                            u.Title = userProps["Title"];
                            u.Role = (RoleTypes)currentRole;

                            string login = userProps["Login"];
                            string password = userProps["Password"];

                            CredentialsController.GetFQTag(login)
                                .Then((res) =>
                                {
                                    Debug.Log($"status: {res.status}");

                                    if (res.result)
                                    {
                                        Debug.Log($"status: {res.status}");

                                        string fqTag = ((GetFQTagResponse)(res.ParsedResponse)).FQTag;

                                        Debug.Log($"fqTag: {fqTag}");

                                        login = string.Format("{0}#{1}", login, fqTag);

                                        CredentialsController.AddUser(login, password, u)
                                           .Then((res_AddUser) =>
                                           {
                                               if (res.result)
                                               {
                                                   DataModel.Instance.Tasks.UpdateTasks();
                                                   DataModel.Instance.Rewards.UpdateAllRewardsItems();

                                                   string roleTypeName = string.Empty;

                                                   if (u.Role == RoleTypes.User)
                                                   {
                                                       roleTypeName = "в <b>Отряд героев</b>";
                                                   }

                                                   if (u.Role == RoleTypes.Administrator)
                                                   {
                                                       roleTypeName = "в <b>Королевский двор</b>";
                                                   }

                                                   Global_MessageBoxHandlerController.ShowMessageBox("Добавление пользователя", $"Новый пользователь успешно добавлен!\n\nИмя для входа:\n<b>{login}</b>");

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
                if (!ReadForm(out var userProps))
                {
                    throw new FQServiceException(FQServiceException.FQServiceExceptionType.EmptyRequiredField);
                }

                Global_MessageBoxHandlerController.ShowMessageBox("Сохранение изменений", "Изменения будут внесены в профиль пользователя\n\nПродолжить?", MessageBoxType.Information, MessageBoxButtonsType.OkCancel)
                    .Then((dialogRes) =>
                    {
                        if (dialogRes == MessageBoxResult.Ok)
                        {
                            CircleProgressBar.SetActive(true);

                            var destinationUser = DataModel.Instance.Credentials.Users.Where(x => x.Id == Guid.Parse(m_textFieldsFiller.TextData["Id"])).FirstOrDefault();

                            User u = new User();
                            u.Id = destinationUser.Id;
                            u.Name = userProps["Name"];
                            u.Title = userProps["Title"];

                            CredentialsController.UpdateUser(u)
                                .Then((res) =>
                                {
                                    Debug.Log($"status: {res.status}");

                                    if (res.result)
                                    {
                                        Global_MessageBoxHandlerController.ShowMessageBox("Сохранение изменений", "Изменения успешно внесены в профиль пользователя!")
                                            .Then((dialogRes_Completed) =>
                                            {
                                                Debug.Log($"status: {res.status}");

                                                var updatedCredentials = DataModel.Instance.Credentials.Users.Where(x => x.Id == destinationUser.Id).FirstOrDefault();

                                                if (updatedCredentials != null)
                                                {
                                                    List<User> tempList = new List<User>();
                                                    tempList.Add(updatedCredentials);
                                                    var itemToDict = CredentialsModel.ToListOfDictionary(tempList).FirstOrDefault();
                                                    ReturnAndClose(itemToDict);
                                                }
                                                else
                                                {
                                                    FQServiceException.ShowExceptionMessage(FQServiceException.FQServiceExceptionType.DefaultError);
                                                    CircleProgressBar.SetActive(false);
                                                }
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
                        if (!inputField.name.Contains("Role"))
                        {
                            if (!string.IsNullOrWhiteSpace(inputField.text))
                            {
                                string propName = inputField.name.Replace("Input_", "");

                                if (propName == "Name" || propName == "Title")
                                {
                                    taskProps.Add(propName, inputField.text.Trim());
                                }
                                else
                                {
                                    taskProps.Add(propName, inputField.text);
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }
               
        //TODO: неактуально?
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
        private void ReturnAndClose(Dictionary<string, object> updatedCredentials)
        {
            try
            {
                if (m_afterCredentialsChangedDelegate != null)
                    m_afterCredentialsChangedDelegate(updatedCredentials);
                m_thisPopup.Close();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        private void DescriptionValuesState()
        {
            //DescriptionValues.TryDisableInteract("Login");
            //DescriptionValues.HideGroup("Password");
        }
    }
}