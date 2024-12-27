using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Ricimi;
using Code.Controllers;
using Code.Models.REST;
using Code.Controllers.MessageBox;
using Assets.Code.Models.REST.CommonTypes;

namespace Code.ViewControllers
{
    public class FormConfirmController : MonoBehaviour
    {
        public GameObject thisForm;
        public GameObject regForm;
        public GameObject restoreForm;

        public GameObject CircleProgressBar;

        public GameObject Group_LeftTime;
        public GameObject Group_LeftTime_Error;

        public enum ConfirmType
        {
            Registration = 0,
            Restore
        }

        [HideInInspector]
        public ConfirmType confirmType;

        public int confirmationSeconds = 300;
        private TimeSpan leftTime;

        public GameObject ConfirmCodeInputField;

        public TMP_Text TimeLable;
        
        public delegate void ResendCode();
        public ResendCode resendCodeDelegate = null;

        public delegate void CodeConfirmed();
        public CodeConfirmed codeConfirmedDelegate = null;

        private bool isCompleted = false;

        private DateTime timerStartTime;

        // Start is called before the first frame update
        void Start()
        {
            ResetState();
        }

        private void ResetState()
        {
            try
            {
                ConfirmCodeInputField.SetActive(true);
                var ConfirmCodeInputFieldTextArea = ConfirmCodeInputField.GetComponentInChildren<TMP_InputField>();
                ConfirmCodeInputFieldTextArea.text = "";

                Group_LeftTime_Error.SetActive(false);
                Group_LeftTime.SetActive(true);                
                leftTime = new TimeSpan(0, 0, confirmationSeconds);

                timerStartTime = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        private void FixedUpdate()
        {
            try
            {
                var timeDiff = (DateTime.UtcNow - timerStartTime).TotalSeconds;

                if (timeDiff >= confirmationSeconds)
                {
                    CancelInvoke("RunTimer");

                    SetInputDisabled();

                    return;
                }

                leftTime = TimeSpan.FromSeconds(confirmationSeconds - timeDiff);

                TimeLable.text = leftTime.ToString(@"mm\:ss");
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        public void OnEdit(string input)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(input) ||
                        input.Length < 6)
                    return;

                CircleProgressBar.SetActive(true);

                switch (confirmType)
                {
                    case ConfirmType.Registration:
                        {
                            RegistrationConfirmed(input);
                            break;
                        }
                    case ConfirmType.Restore:
                        {
                            RestoreConfirmed(input);
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        public void OnClick_ButtonResend()
        {
            try
            {
                ResetState();

                if (resendCodeDelegate != null)
                {
                    if (confirmType == ConfirmType.Registration)
                    {
                        thisForm.SetActive(false);
                        regForm.SetActive(true);
                    }
                    if (confirmType == ConfirmType.Restore)
                    {
                        thisForm.SetActive(false);
                        restoreForm.SetActive(true);
                    }

                    resendCodeDelegate();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        public void RegistrationConfirmed(string currentCode)
        {
            try
            {
                RegistrationController.RegConfirm(CredentialHandler.Instance.Credentials.Login, currentCode)
                        .Then((res) =>
                        {
                            if (res.result)
                            {
                                Global_MessageBoxHandlerController.ShowMessageBox("Добро пожаловать", string.Format("Регистрация\n<b>{0}</b>\nвыполнена успешно!", CredentialHandler.Instance.Credentials.Login));

                                if (codeConfirmedDelegate != null)
                                {
                                    codeConfirmedDelegate();
                                }

                                ResetState();
                                thisForm.SetActive(false);
                            }
                            else
                            {
                                SetInputDisabled();
                            }

                            CircleProgressBar.SetActive(false);
                        })
                        .Catch((ex) =>
                        {
                            SetInputDisabled();

                            //Если сюда попали, то ошибку уже обработали в DataModel и показали уведомление
                            //Нужно только убрать индикацию загрузки
                            CircleProgressBar.SetActive(false);
                        });
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);

                SetInputDisabled();

                CircleProgressBar.SetActive(false);

                FQServiceException.ShowExceptionMessage(ex);
            }
        }

        public void RestoreConfirmed(string currentCode)
        {
            try
            {
                RestoreController.RestoreConfirm(CredentialHandler.Instance.Credentials.Login, currentCode)
                        .Then((res) =>
                        {
                            if (res.result)
                            {
                                Global_MessageBoxHandlerController.ShowMessageBox("Восстановление доступа", "Восстановление выполнено успешно!");

                                if (codeConfirmedDelegate != null)
                                {
                                    codeConfirmedDelegate();
                                }

                                ResetState();
                                thisForm.SetActive(false);
                            }
                            else
                            {
                                SetInputDisabled();
                            }

                            CircleProgressBar.SetActive(false);

                        })
                        .Catch((ex) =>
                        {
                            SetInputDisabled();

                            //Если сюда попали, то ошибку уже обработали в DataModel и показали уведомление
                            //Нужно только убрать индикацию загрузки
                            CircleProgressBar.SetActive(false);
                        }); ;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);

                SetInputDisabled();

                CircleProgressBar.SetActive(false);

                FQServiceException.ShowExceptionMessage(ex);
            }
        }

        public void OnClick_Back()
        {
            ResetState();
            thisForm.SetActive(false);

            if (confirmType == ConfirmType.Registration)
            {
                regForm.SetActive(true);
            }
            if (confirmType == ConfirmType.Restore)
            {
                restoreForm.SetActive(true);
            }
        }

        private Dictionary<string, string> ReadForm()
        {
            try
            {
                Dictionary<string, string> taskProps = new Dictionary<string, string>();
                var inputFields = transform.GetComponentsInChildren<TMP_InputField>();
                foreach (var inputField in inputFields)
                {
                    taskProps.Add(inputField.name, inputField.text);
                }
                return taskProps;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        private void SetInputDisabled()
        {
            ConfirmCodeInputField.SetActive(false);

            Group_LeftTime.SetActive(false);
            Group_LeftTime_Error.SetActive(true);

            Group_LeftTime_Error.GetComponentInChildren<TMP_InputField>().text = ConfirmCodeInputField.GetComponentInChildren<TMP_InputField>().text;
        }
    }
}