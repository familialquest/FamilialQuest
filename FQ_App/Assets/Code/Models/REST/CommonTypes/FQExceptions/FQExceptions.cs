using Code.Controllers.MessageBox;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.Code.Models.REST.CommonTypes
{
    public class FQServiceException : Exception
    {
        private static string logFileName = Application.persistentDataPath + Path.DirectorySeparatorChar + "FQLog.txt";

        public enum FQServiceExceptionType
        {
            //Общие
            DefaultError = 0,
            UnsupportedClientVersion,
            EmptyRequiredField,
            NotEnoughRights,
            IncorrectLoginFormat,
            ItemNotFound,
            UnsupportedStatusChanging,
            TotalItemsLimitAchieved,

            //Аутентификация
            AuthError = 10,
            ShortPassword,

            //Рега/подтверждение
            WrongConfirmCode = 20,
            WrongPassword,
            WrongPasswordEq,

            //Подписка
            LimitAchieved = 100,
            PurchaseStateIsCanceled,
            AcknowledgementStateIsAcknowledged,
            PurchaseIsAlreadyExists,
            IncorrectPlatform,

            //Награды
            NotEnoughCoins = 200,

            //Технические
            NetworkError = 1000
        }

        public static Dictionary<string, string> messageTemplates = new Dictionary<string, string>();

        //TODO: для предотвращения наслаивания эксепшенов.
        public static bool msgBoxOpened = false;

        static FQServiceException()
        {
            loadData();
        }

        /// <summary>
        /// Считывание списка шаблонов сообщений из сопутствующего routes.json
        /// </summary>
        private static void loadData()
        {
            try
            {
                string templatesJson = Resources.Load<TextAsset>("FQExceptionsMessagesTemplates").text;
                messageTemplates = JsonConvert.DeserializeObject<Dictionary<string, string>>(templatesJson);
            }
            catch
            {             
                //logger.Error(ex);
            }
        }


        public FQServiceExceptionType exType;

        public FQServiceException(FQServiceExceptionType _exType)
        : base(_exType.ToString())
        {
            exType = _exType;
        }
        
        public static RSG.Promise<MessageBoxResult> ShowExceptionMessage(FQServiceExceptionType _exType, string customeText = "")
        {
            msgBoxOpened = true;

            string msgTitle = "Упс..";
            string msgBody = messageTemplates[FQServiceExceptionType.DefaultError.ToString()];

            if (string.IsNullOrEmpty(customeText))
            {
                if (_exType == FQServiceExceptionType.IncorrectPlatform)
                {
                    msgTitle = "Премиум-доступ";
                }

                messageTemplates.TryGetValue(_exType.ToString(), out msgBody);
            }
            else
            {
                msgBody = customeText;
            }

            return Global_MessageBoxHandlerController.ShowMessageBox(msgTitle, msgBody, MessageBoxType.Warning);            
        }

        public static RSG.Promise<MessageBoxResult> ShowExceptionMessage(Exception ex)
        {
            string msgTitle = "Упс..";
            string msgBody = messageTemplates[FQServiceExceptionType.DefaultError.ToString()];

            if (ex is FQServiceException)
            {
                FQServiceException fqEx = (FQServiceException)ex;
                messageTemplates.TryGetValue(fqEx.exType.ToString(), out msgBody);
            }

            return Global_MessageBoxHandlerController.ShowMessageBox(msgTitle, msgBody, MessageBoxType.Warning);
        }

        public static RSG.Promise<MessageBoxResult> ShowExceptionMessage(string exText)
        {           
            string msgTitle = "Упс..";
            string msgBody = messageTemplates[FQServiceExceptionType.DefaultError.ToString()];

            if (Enum.TryParse(exText, out FQServiceExceptionType _exType))
            {                
                if (_exType == FQServiceExceptionType.PurchaseStateIsCanceled ||
                    _exType == FQServiceExceptionType.AcknowledgementStateIsAcknowledged ||
                    _exType == FQServiceExceptionType.PurchaseIsAlreadyExists)
                {
                    msgTitle = "Премиум-доступ";
                }

                msgBody = FQServiceException.messageTemplates[_exType.ToString()];
            }

            return Global_MessageBoxHandlerController.ShowMessageBox(msgTitle, msgBody, MessageBoxType.Warning);
        }
    }
}
