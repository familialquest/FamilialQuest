using System.Collections.Generic;
using TMPro;
using UnityEngine;

using Ricimi;
using Code.Models.REST;
using Code.Controllers.MessageBox;
using Code.Controllers;
using System;
using Assets.Code.Models.REST.CommonTypes;
using Code.Models.REST.Users;

namespace Code.ViewControllers
{
    public class FormRegistrationController : MonoBehaviour
    {
        public GameObject thisForm;
        public GameObject loginForm;
        public GameObject restoreForm;
        public GameObject confirmForm;

        public TMP_InputField loginInputField;
        public TMP_InputField passwordInputField;

        public TMP_InputField loginInputField_LoginForm;

        public FormConfirmController confirmController;

        public GameObject CircleProgressBar;

        void Start()
        {

        }

        public void OnClick_ButtonSignUp()
        {
            try
            {
                if (!ReadForm(out var authProps))
                {
                    throw new FQServiceException(FQServiceException.FQServiceExceptionType.EmptyRequiredField);
                }

                if (!Account.VerifyLoginAsEmail(authProps["Login"]))
                {
                    throw new FQServiceException(FQServiceException.FQServiceExceptionType.IncorrectLoginFormat);
                }

                if (authProps["Password"].Length < 8)
                {
                    throw new FQServiceException(FQServiceException.FQServiceExceptionType.ShortPassword);
                }

                CircleProgressBar.SetActive(true);

                RegistrationController.Reg(authProps["Login"], authProps["Password"])
                    .Then((res) =>
                    {
                        Debug.Log($"status: {res.status}");

                        if (res.result)
                        {
                            //Сохранение логина для подтверждения
                            CredentialHandler.Instance.Credentials = new UserCredentials(true);
                            CredentialHandler.Instance.Credentials.Login = authProps["Login"];

                            confirmController.confirmType = FormConfirmController.ConfirmType.Registration;
                            confirmController.codeConfirmedDelegate = OnClick_ShowLoginForm;
                            confirmController.resendCodeDelegate = OnClick_ButtonSignUp;
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

        public void OnClick_ShowRestoreForm()
        {
            thisForm.SetActive(false);
            restoreForm.SetActive(true);
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