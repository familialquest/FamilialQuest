using Code.Models;
using System;
using TMPro;
using UnityEngine;
using Code.ViewControllers.TList;
using Code.Models.REST;
using UnityEngine.UI;
using Assets.Code.Models.REST.CommonTypes;
using Code.Controllers.MessageBox;
using Ricimi;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Globalization;

namespace Code.ViewControllers
{
    [RequireComponent(typeof(Popup))]
    public class HelloWorldController : MonoBehaviour
    {
        
        void Awake()
        {
           
        }
        
        public void OnButton_Complete(PopupOpener wizardPopupOpener)
        {
            try
            {
                PlayerPrefs.SetInt(CredentialHandler.Instance.Credentials.Login, 1);
                wizardPopupOpener.OpenPopup();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);                

                FQServiceException.ShowExceptionMessage(ex);
            }
        }
    }
}