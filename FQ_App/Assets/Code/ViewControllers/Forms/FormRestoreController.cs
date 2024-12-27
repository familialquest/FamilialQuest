using System.Collections.Generic;
using TMPro;
using UnityEngine;

using Code.Models.REST;
using Ricimi;
using Code.Controllers;
using Code.Controllers.MessageBox;
using System;
using Assets.Code.Models.REST.CommonTypes;
using Code.Models.REST.Users;

namespace Code.ViewControllers
{
    public class FormRestoreController : MonoBehaviour
    {
        public GameObject thisForm;
        public GameObject loginForm;
        public GameObject regForm;
        public GameObject confirmForm;
        public FormConfirmController confirmController;

        public GameObject CircleProgressBar;

        public TMP_InputField loginInputField;
        public TMP_InputField passwordInputField;

        public TMP_InputField loginInputField_LoginForm;

        public PopupOpener ConfirmationPopup;

        private Popup m_thisPopup;

        void Start()
        {
            try
            {
                if (!string.IsNullOrEmpty(CredentialHandler.Instance.Credentials.Login))
                {
                    loginInputField.text = CredentialHandler.Instance.Credentials.Login;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        public void OnClick_ButtonSend()
        {
            try
            {
                if (!ReadForm(out var restoreProps))
                {
                    throw new FQServiceException(FQServiceException.FQServiceExceptionType.EmptyRequiredField);
                }

                if (!Account.VerifyLoginAsEmail(restoreProps["Login"]))
                {
                    throw new FQServiceException(FQServiceException.FQServiceExceptionType.IncorrectLoginFormat);
                }

                if (restoreProps["Password"].Length < 8)
                {
                    throw new FQServiceException(FQServiceException.FQServiceExceptionType.ShortPassword);
                }

                CircleProgressBar.SetActive(true);

                RestoreController.Restore(restoreProps["Login"], restoreProps["Password"])
                    .Then((res) =>
                    {
                        Debug.Log($"status: {res.status}");

                        if (res.result)
                        {
                            //Сохранение логина для подтверждения
                            CredentialHandler.Instance.Credentials = new UserCredentials(true);
                            CredentialHandler.Instance.Credentials.Login = restoreProps["Login"];

                            confirmController.confirmType = FormConfirmController.ConfirmType.Restore;
                            confirmController.codeConfirmedDelegate = OnClick_ShowLoginForm;
                            confirmController.resendCodeDelegate = OnClick_ButtonSend;
                            thisForm.SetActive(false);
                            confirmForm.SetActive(true);
                        }

                        CircleProgressBar.SetActive(false);
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

        public void OnClick_ShowLoginForm()
        {
            loginInputField.text = string.Empty;
            passwordInputField.text = string.Empty;

            loginInputField_LoginForm.text = CredentialHandler.Instance.Credentials.Login;

            thisForm.SetActive(false);
            loginForm.SetActive(true);
        }

        public void OnClick_ShowRegForm()
        {
            thisForm.SetActive(false);
            regForm.SetActive(true);
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
                        if (!string.IsNullOrWhiteSpace(inputField.text) || inputField.name.Contains("Description"))
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
    }

}