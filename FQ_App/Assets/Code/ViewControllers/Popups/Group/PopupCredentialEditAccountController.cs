using Ricimi;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Code.Controllers.MessageBox;
using Assets.Code.Controllers;
using Code.Models.REST;
using Code.Models.REST.Rewards;
using System;
using Code.Models;
using System.Linq;
using Code.Models.REST.Users;
using Code.Controllers;
using Assets.Code.Models.REST.CommonTypes;
using UnityEngine.SceneManagement;

public class PopupCredentialEditAccountController : MonoBehaviour
{
    public TMP_Text TextInfo;

    public GameObject CircleProgressBar;

    //bool IsAllFieldValid = false;
    private Popup m_thisPopup;

    private TooltipController m_tooltipController;
    private User m_DestUser;

    public delegate void AfterCredentialsChanged(Dictionary<string, object> newCredentialsProps);
    private AfterCredentialsChanged m_afterCredentialsChangedDelegate = null;
    public AfterCredentialsChanged AfterCredentialsChangedDelegate { get => m_afterCredentialsChangedDelegate; set => m_afterCredentialsChangedDelegate = value; }

    //TODO: не менять Start на Awake, т.к. сначала должен отработать SetDestinationUserId
    void Start()
    {
        try
        {
            m_thisPopup = GetComponent<Popup>();
            m_tooltipController = GetComponent<TooltipController>();

            string message = String.Empty;

            if (m_DestUser.Id == CredentialHandler.Instance.Credentials.userId)
            {
                message = "Для изменения <b>своего</b> пароля\nукажите <b>новый пароль</b>,\nа также <b>ваш текущий пароль</b>\nдля подтверждения операции";
            }
            else
            {
                message = string.Format("Для изменения пароля\n<b>{0}</b>\nукажите <b>новый пароль</b>,\nа также <b>ваш текущий пароль</b>\nдля подтверждения операции", m_DestUser.Name);
            }

            TextInfo.text = message;
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

    public void OnClick_ButtonChange()
    {
        try
        {
            if (!ReadForm(out var userProps))
            {
                throw new FQServiceException(FQServiceException.FQServiceExceptionType.EmptyRequiredField);
            }

            if (userProps["Password"].Length < 8)
            {
                throw new FQServiceException(FQServiceException.FQServiceExceptionType.ShortPassword);
            }

            if (!userProps["Password"].Equals(userProps["PasswordConfirm"]))
            {
                throw new FQServiceException(FQServiceException.FQServiceExceptionType.WrongPasswordEq);
            }

            Global_MessageBoxHandlerController.ShowMessageBox("Смена пароля", "Изменения будут внесены в профиль пользователя\n\nПродолжить?", MessageBoxType.Information, MessageBoxButtonsType.OkCancel)
                 .Then((dialogRes) =>
                 {
                     if (dialogRes == MessageBoxResult.Ok)
                     {
                         CircleProgressBar.SetActive(true);

                         if (m_DestUser.Id == CredentialHandler.Instance.Credentials.userId)
                         {
                             CredentialsController.ChangeSelfPassword(userProps["AdminOrCurrentPassword"], userProps["Password"])
                                 .Then((res) =>
                                 {
                                     Debug.Log($"status: {res.status}");

                                     if (res.result)
                                     {
                                         CircleProgressBar.SetActive(false);
                                                                                  
                                         //Чтобы после перезахода не проскользнул старый мусор
                                         DataModel.Instance.Credentials.Users = null;
                                         DataModel.Instance.GroupInfo.MyGroup = null;
                                         DataModel.Instance.Rewards.Rewards = null;
                                         DataModel.Instance.Tasks.Tasks = null;
                                         DataModel.Instance.HistoryEvents.HistoryEvents = null;

                                         CredentialHandler.Instance.Credentials = new UserCredentials();
                                         CredentialHandler.Instance.CurrentUser = new User();

                                         PlayerPrefs.SetString("AuthToken", string.Empty);

                                         Code.Controllers.NotificationController.Instance.Uninit();

                                         Global_MessageBoxHandlerController.ShowMessageBox("Смена пароля", "Пароль успешно изменен!\n\nВыполните вход в аккаунт с новыми учетными данными.")
                                             .Then((dialogRes_Completed) =>
                                             {
                                                 SceneManager.LoadScene("StartPages", LoadSceneMode.Single);
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
                         else
                         {
                             CredentialsController.ChangeGroupUserPassword(m_DestUser.Id, userProps["AdminOrCurrentPassword"], userProps["Password"])
                                 .Then((res) =>
                                 {
                                     Debug.Log($"status: {res.status}");

                                     if (res.result)
                                     {
                                         Global_MessageBoxHandlerController.ShowMessageBox("Смена пароля", "Изменения успешно внесены в профиль пользователя.")
                                             .Then((dialogRes_Completed) =>
                                             {
                                                 var updatedCredentials = DataModel.Instance.Credentials.Users.Where(x => x.Id == m_DestUser.Id).FirstOrDefault();

                                                 if (updatedCredentials != null)
                                                 {
                                                     List<User> tempList = new List<User>();
                                                     tempList.Add(updatedCredentials);
                                                     var itemToDict = CredentialsModel.ToListOfDictionary(tempList).FirstOrDefault();
                                                     ReturnAndClose(itemToDict);
                                                 }
                                                 else
                                                 {
                                                     CircleProgressBar.SetActive(false);
                                                     throw new FQServiceException(FQServiceException.FQServiceExceptionType.DefaultError);
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
                    if (!string.IsNullOrWhiteSpace(inputField.text))
                    {
                        string propName = inputField.name.Replace("Input_", "");
                        taskProps.Add(propName, inputField.text);
                    }
                    else
                    {
                        return false;
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
    public void ShowTooltip(string text)
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

    public void SetDestinationUserId(string userId)
    {
        try
        {
            if (Guid.TryParse(userId, out Guid destUserId))
            {
                var destinationUser = DataModel.Instance.Credentials.Users.Where(x => x.Id == destUserId).FirstOrDefault();

                if (destinationUser != null)
                {
                    m_DestUser = destinationUser;
                }
                else
                {
                    throw new FQServiceException(FQServiceException.FQServiceExceptionType.DefaultError);
                }
            }
            else
            {
                throw new FQServiceException(FQServiceException.FQServiceExceptionType.DefaultError);
            }
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
}
