using System;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

using Code.Controllers.MessageBox;
using Assets.Code.Controllers;
using Code.Models;
using Code.Models.REST;
using System.Linq;
using Code.Models.RoleModel;
using Assets.Code.Models.REST.CommonTypes;
using Code.Models.REST.Users;

namespace Code.ViewControllers
{
    public class FormLoginController : MonoBehaviour
    {
        public GameObject thisForm;
        public GameObject regForm;
        public GameObject restoreForm;

        public GameObject CircleProgressBar;

        public TMP_InputField loginInputField;
        public TMP_InputField passInputField;
        //public TMP_InputField serverAddressInputField;

        void Start()
        {
            try
            {
                string storedLogin = PlayerPrefs.GetString("Login", string.Empty);
                string storedAuthToken = PlayerPrefs.GetString("AuthToken", string.Empty);

                if (!string.IsNullOrEmpty(storedLogin) &&
                    !string.IsNullOrEmpty(storedAuthToken))
                {
                    if (loginInputField != null)
                    {
                        loginInputField.text = PlayerPrefs.GetString("Login", "");
                    }

                    CredentialHandler.Instance.Credentials.Login = storedLogin;
                    CredentialHandler.Instance.Credentials.tokenB64 = storedAuthToken;

                    CircleProgressBar.SetActive(true);

                    AuthController.Auth(CredentialHandler.Instance.Credentials.Login, string.Empty, CredentialHandler.Instance.Credentials.tokenB64)
                        .Then((res) =>
                        {
                            Debug.Log($"status: {res.status}");

                            if (res.result)
                            {
                                DataModel.Instance.Credentials.UpdateAllCredentials()
                                    .Then((resUsers) =>
                                    {
                                        if (CredentialHandler.Instance.CurrentUser != null)
                                        {
                                            RoleModel.Instance.CurrentRole = CredentialHandler.Instance.CurrentUser.Role;

                                            try
                                            {
                                                Controllers.NotificationController.Instance.Init();
                                            }
                                            catch (Exception exNotification)
                                            {
                                                Debug.LogError(exNotification);
                                            }

                                            SceneManager.LoadScene("Main");

                                            CircleProgressBar.SetActive(false);
                                        }
                                        else
                                        {
                                            CircleProgressBar.SetActive(false);
                                            throw new FQServiceException(FQServiceException.FQServiceExceptionType.DefaultError);
                                        }
                                    })
                                    .Catch((ex) =>
                                    {
                                        //Если сюда попали, то ошибку уже обработали в DataModel и показали уведомление
                                        //Нужно только убрать индикацию загрузки
                                        CircleProgressBar.SetActive(false);
                                    });

                                //В противном случае, уже выкинулся эксепшн и сцена перезапустилась
                            }
                            else
                            {
                                PlayerPrefs.SetString("AuthToken", string.Empty);
                                CircleProgressBar.SetActive(false);
                            }
                        })
                        .Catch((ex) =>
                        {
                            PlayerPrefs.SetString("AuthToken", string.Empty);
                            //Если сюда попали, то ошибку уже обработали в DataModel и показали уведомление
                            //Нужно только убрать индикацию загрузки
                            CircleProgressBar.SetActive(false);
                        });                       
                }                

                //if (serverAddressInputField != null)
                //{
                //    serverAddressInputField.text = PlayerPrefs.GetString("ServerUri", "");
                //}

                //TODO: пока отключил снижение фпс
                ////Вытавим maxFPS = 24 кадра
                //QualitySettings.vSyncCount = 0;
                //Application.targetFrameRate = 24;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        public void OnClick_ButtonLogon()
        {
            try
            {
                loginInputField.text = loginInputField.text.Trim();

                // чтение, проверка введенных данных и их сохранение в PlayerPrefs
                if (string.IsNullOrWhiteSpace(loginInputField.text))
                {
                    Global_MessageBoxHandlerController.ShowMessageBox("Упс..", "Введите логин.", MessageBoxType.Warning);
                    return;
                }
                else
                {
                    if (!Account.VerifyLoginAsEmail(loginInputField.text) &&
                        !Account.VerifyLoginAsFQInnerLogin(loginInputField.text))
                    {
                        throw new FQServiceException(FQServiceException.FQServiceExceptionType.IncorrectLoginFormat);
                    }

                    PlayerPrefs.SetString("Login", loginInputField.text);
                }

                if (string.IsNullOrWhiteSpace(passInputField.text))
                {
                    Global_MessageBoxHandlerController.ShowMessageBox("Упс..", "Введите пароль.", MessageBoxType.Warning);
                    return;
                }

                //if (ValidateServerAddress(serverAddressInputField.text, out Uri serverUri))
                //{
                //    PlayerPrefs.SetString("ServerUri", serverUri.ToString());
                //    //PlayerPrefs.SetString("ServerAddress", serverUri.Host);
                //    //PlayerPrefs.SetString("ServerPort", serverUri.Port.ToString());
                //    //PlayerPrefs.SetString("ServerProto", serverUri.Scheme);
                //}
                //else
                //{
                //    Global_MessageBoxHandlerController.ShowMessageBox("Упс..", "Введите корректный адрес сервера.");
                //    return;
                //}

                CircleProgressBar.SetActive(true);

                AuthController.Auth(loginInputField.text, passInputField.text)
                    .Then((res) =>
                    {
                        Debug.Log($"status: {res.status}");

                        if (res.result)
                        {
                            //Запомним логни.
                            //Если сюда попали - новый токен уже в Credentials.
                            CredentialHandler.Instance.Credentials.Login = loginInputField.text;

                            DataModel.Instance.Credentials.UpdateAllCredentials()
                                .Then((resUsers) =>
                                {
                                    if (CredentialHandler.Instance.CurrentUser != null)
                                    {
                                        RoleModel.Instance.CurrentRole = CredentialHandler.Instance.CurrentUser.Role;

                                        try
                                        {
                                            Controllers.NotificationController.Instance.Init();
                                        }
                                        catch (Exception exNotification)
                                        {
                                            Debug.LogError(exNotification);
                                        }

                                        SceneManager.LoadScene("Main");

                                        CircleProgressBar.SetActive(false);
                                    }
                                    else
                                    {
                                        CircleProgressBar.SetActive(false);
                                        throw new FQServiceException(FQServiceException.FQServiceExceptionType.DefaultError);
                                    }
                                })
                                .Catch((ex) =>
                                {
                                    //Если сюда попали, то ошибку уже обработали в DataModel и показали уведомление
                                    //Нужно только убрать индикацию загрузки
                                    CircleProgressBar.SetActive(false);
                                });

                            //В противном случае, уже выкинулся эксепшн и сцена перезапустилась
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
            catch (Exception ex)
            {
                Debug.LogError(ex);

                CircleProgressBar.SetActive(false);

                FQServiceException.ShowExceptionMessage(ex);
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

        public bool ValidateServerAddress(string input, out Uri serverUri)
        {
            try
            {
                serverUri = null;
                bool parseResult = false;

                parseResult = Uri.TryCreate(input, UriKind.RelativeOrAbsolute, out var uri);
                if (true == parseResult)
                {
                    if (!uri.IsAbsoluteUri || (!uri.Scheme.Equals(Uri.UriSchemeHttp) && !uri.Scheme.Equals(Uri.UriSchemeHttps)))
                    {
                        input = Uri.UriSchemeHttps + Uri.SchemeDelimiter + input; // по умолчанию ставим https, если не указано иное
                        parseResult = Uri.TryCreate(input, UriKind.RelativeOrAbsolute, out uri);
                    }
                }

                if (true == parseResult)
                {
                    UriBuilder builder = new UriBuilder(input);
                        
                    if (-1 == builder.Port)
                        builder.Port = 443; // по умолчанию ставим 443, если не указано иное

                    serverUri = builder.Uri;
                }

                return parseResult;

                //UriHostNameType uriType = Uri.CheckHostName(input);
                //if (uriType != UriHostNameType.Unknown)
                //{
                //    switch (uriType)
                //    {
                //        case UriHostNameType.Basic:
                //            return false;
                //        case UriHostNameType.Dns:
                //            break;
                //        case UriHostNameType.IPv4:
                //        case UriHostNameType.IPv6:
                //            if (!(IPAddress.TryParse(input, out var addr)))
                //                return false;
                //            break;
                //    }
                //    return true;
                //}

                //return false;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        public void OnClick_ShowRegForm()
        {
            thisForm.SetActive(false);
            regForm.SetActive(true);
        }

        public void OnClick_ShowRestoreForm()
        {
            loginInputField.text = loginInputField.text.Trim();

            CredentialHandler.Instance.Credentials.Login = loginInputField.text;
            thisForm.SetActive(false);
            restoreForm.SetActive(true);
        }
    } 
}