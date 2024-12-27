//using System;
//using System.Linq;
//using System.Collections.Generic;
//using UnityEngine;
//using TMPro;
//using Ricimi;

//using Code.Controllers.MessageBox;
//using Code.Controllers;
//using Code.Models;
//using Code.Models.REST;
//using Code.Models.REST.Users;
//using static Code.Models.CredentialsModel;
//using Code.Models.RoleModel;
//using Assets.Code.Models.REST.CommonTypes;

//namespace Code.ViewControllers
//{
//    [RequireComponent(typeof(Popup))]    
//    public class PopupReportCreateController : MonoBehaviour
//    {
//        private Popup m_thisPopup;
//        private TextFieldsFiller m_textFieldsFiller;
//        public GameObject CircleProgressBar;

//        void Awake()
//        {
//            try
//            {
//                m_thisPopup = GetComponent<Popup>();               
//            }
//            catch (Exception ex)
//            {
//                Debug.LogError(ex);
//                throw;
//            }
//        }
//        private void Start()
//        {

//        }

//        public void OnClick_ButtonCreate()
//        {
//            try
//            {
//                ReadForm(out var userProps);

//                Global_MessageBoxHandlerController.ShowMessageBox("Отправка сообщения", "Будет отправлено сообщение об ошибке\n\nПродолжить?", MessageBoxType.Information, MessageBoxButtonsType.OkCancel)
//                   .Then((dialogRes) =>
//                   {
//                       if (dialogRes == MessageBoxResult.Ok)
//                       {
//                           CircleProgressBar.SetActive(true);

//                           string reportDetails = userProps.ContainsKey("Description") ? userProps["Description"] : string.Empty;

//                           FQServiceException.SendReport(CredentialHandler.Instance.Credentials.Login, reportDetails);

//                           Global_MessageBoxHandlerController.ShowMessageBox("Отправка сообщения", "Спасибо, Ваше обращение будет рассмотрено в ближайшее время.", MessageBoxType.Information);

//                           CircleProgressBar.SetActive(false);

//                           m_thisPopup.Close();
//                       }
//                   })
//                   .Catch((ex) =>
//                   {
//                       Debug.LogError(ex);

//                       CircleProgressBar.SetActive(false);

//                       FQServiceException.ShowExceptionMessage(ex);
//                   });                
//            }
//            catch (Exception ex)
//            {
//                Debug.LogError(ex);

//                CircleProgressBar.SetActive(false);

//                FQServiceException.ShowExceptionMessage(ex);
//            }            
//        }

//        private bool ReadForm(out Dictionary<string, string> taskProps)
//        {
//            try
//            {
//                taskProps = new Dictionary<string, string>();
//                var inputFields = transform.GetComponentsInChildren<TMP_InputField>();

//                foreach (var inputField in inputFields)
//                {
//                    if (inputField.IsActive())
//                    {                        
//                        if (!string.IsNullOrWhiteSpace(inputField.text))
//                        {
//                            string propName = inputField.name.Replace("Input_", "");
//                            taskProps.Add(propName, inputField.text);
//                        }
//                        else
//                        {
//                            return false;
//                        }                        
//                    }
//                }

//                return true;
//            }
//            catch (Exception ex)
//            {
//                Debug.LogError(ex);
//                throw;
//            }
//        }
//    }
//}