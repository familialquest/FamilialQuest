using System;
using System.Collections.Generic;
using System.Text;

namespace CommonLib
{
    public class FQServiceException : Exception
    {
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

        public FQServiceException()
        {}

        public FQServiceExceptionType exType;

        public FQServiceException(string message)
        : base(message)
        {
            if (Enum.TryParse(message, out FQServiceExceptionType _exType))
            {
                exType = _exType;
            }
            else
            {
                exType = FQServiceExceptionType.DefaultError;
            }
        }

        public FQServiceException(FQServiceExceptionType _exType)
        : base(_exType.ToString())
        {
            exType = _exType;
        }
    }
}
