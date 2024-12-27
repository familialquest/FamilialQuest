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
    public class ExternalLinksController : MonoBehaviour
    {
        
        void Awake()
        {
           
        }

        public void OnButton_OpenPrivacyPolicy()
        {
            try
            {
                Application.OpenURL("https://familialquest.github.io.");
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);

                FQServiceException.ShowExceptionMessage(ex);
            }
        }

        public void OnButton_OpenHelp()
        {
            try
            {
                Application.OpenURL("https://docs.google.com/forms/d/e/1FAIpQLSf1sgwdp2EJR62ok8lrZ7dhCdsMXn9iNHZR73VXTdvTqgbW8Q/viewform");
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);

                FQServiceException.ShowExceptionMessage(ex);
            }
        }

        public void OnButton_OpenSite()
        {
            try
            {
                Application.OpenURL("https://familialquest.com");
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                FQServiceException.ShowExceptionMessage(ex);
            }
        }

        public void OnButton_OpenInstagram()
        {
            try
            {
                Application.OpenURL("https://www.instagram.com/familialquest/");
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);

                FQServiceException.ShowExceptionMessage(ex);
            }
        }

        public void OnButton_OpenTwitter()
        {
            try
            {
                Application.OpenURL("https://twitter.com/familialquest");
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);

                FQServiceException.ShowExceptionMessage(ex);
            }
        }
    }
}