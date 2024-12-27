using System;
using System.Collections.Generic;
using static CommonLib.FQServiceException;

namespace TaskService.Models
{
    /// <summary>
    /// Существующие типы задач. 
    /// Конвертируем в int.
    /// </summary>
    public enum BaseTaskType : int
    {
        /// <summary>
        /// Базовая стандартная задача
        /// </summary>
        Base = 0,
        /// <summary>
        /// Нестандартная задача
        /// </summary>
        Custom = int.MaxValue
    }

    /// <summary>
    /// Возможные значения статуса задачи.
    /// Конвертируем в int.
    /// </summary>
    public enum BaseTaskStatus : int
    {
        #region GOOD
        /// <summary>
        /// Когда задача только создана, но не ушла "в использование"
        /// </summary>
        Created = 0,
        /// <summary>
        /// Когда задача была только назначена пользователю или группе
        /// </summary>
        Assigned = 10,
        /// <summary>
        /// Когда задача была принята пользователем или группой. Выполнено не начиналось.
        /// </summary>
        Accepted = 20,
        /// <summary>
        /// Когда задача начала выполняться пользователем или группой.
        /// </summary>
        InProgress = 30,
        /// <summary>
        /// Когда задача была завершена пользователем или группой (неважно: успешно или нет).
        /// </summary>
        Completed = 40,
        /// <summary>
        /// Когда задача была отправлена на проверку "заказчикам".
        /// </summary>
        PendingReview = 50,
        /// <summary>
        /// Когда результат выполнения задачи был рассмотрен и оказался удовлетворительным.
        /// </summary>
        Successed = 60,
        /// <summary>
        /// Когда награда была "выплачена". 
        /// </summary>
        Closed = 70,
        #endregion

        #region BAD
        /// <summary>
        /// Когда задача была удалена до того, как она ушла "в использование".
        /// </summary>
        Deleted = 1000,
        /// <summary>
        /// Когда задача была отклонена назначенным исполнителем
        /// </summary>
        Declined = 1020,
        /// <summary>
        /// Когда время, выделенное на принятие задача, истекло. (Глобальнее - закрытие задачи,когда выполнение больше не актуально, и манипуляции с начислением\штрафом не требуются)
        /// </summary>
        AvailableUntilPassed = 1040,
        /// <summary>
        /// Когда время, выделенное на выполнение задачи, истекло.
        /// </summary>
        SolutionTimeOver = 1041,
        /// <summary>
        /// Когда задача была отменена уже после назначения пользователю
        /// </summary>
        Canceled = 1050,
        /// <summary>
        /// Когда результат выполнения задачи был рассмотрен и оказался НЕудовлетворительным.
        /// </summary>
        Failed = 1060,
        #endregion

        /// <summary>
        /// Статус неопределен
        /// </summary>
        None = -1
    }

    public static class TaskStatusRelations
    {
        private static Dictionary<BaseTaskStatus, List<BaseTaskStatus>> Rels = new Dictionary<BaseTaskStatus, List<BaseTaskStatus>>()
        {
            {BaseTaskStatus.Created, new List<BaseTaskStatus> { BaseTaskStatus.Assigned, BaseTaskStatus.Deleted } },
            {BaseTaskStatus.Assigned, new List<BaseTaskStatus> { BaseTaskStatus.Accepted, BaseTaskStatus.Canceled, BaseTaskStatus.Declined, BaseTaskStatus.AvailableUntilPassed } },
            {BaseTaskStatus.Accepted, new List<BaseTaskStatus> { BaseTaskStatus.InProgress, BaseTaskStatus.Canceled, BaseTaskStatus.SolutionTimeOver, BaseTaskStatus.Declined } },
            {BaseTaskStatus.InProgress, new List<BaseTaskStatus> { BaseTaskStatus.Completed, BaseTaskStatus.Canceled, BaseTaskStatus.Declined, BaseTaskStatus.SolutionTimeOver } },
            {BaseTaskStatus.Completed, new List<BaseTaskStatus> { BaseTaskStatus.PendingReview } },
            {BaseTaskStatus.PendingReview, new List<BaseTaskStatus> { BaseTaskStatus.Successed, BaseTaskStatus.Failed, BaseTaskStatus.InProgress } },
            {BaseTaskStatus.Successed, new List<BaseTaskStatus> { BaseTaskStatus.Closed, BaseTaskStatus.Deleted } },
            {BaseTaskStatus.Closed, new List<BaseTaskStatus> { BaseTaskStatus.Deleted } },

            {BaseTaskStatus.AvailableUntilPassed, new List<BaseTaskStatus> { BaseTaskStatus.Deleted } },
            {BaseTaskStatus.SolutionTimeOver, new List<BaseTaskStatus> { BaseTaskStatus.Deleted } },

            {BaseTaskStatus.Declined, new List<BaseTaskStatus> { BaseTaskStatus.Deleted } },
            {BaseTaskStatus.Canceled, new List<BaseTaskStatus> { BaseTaskStatus.Deleted } },
            {BaseTaskStatus.Failed, new List<BaseTaskStatus> { BaseTaskStatus.Closed, BaseTaskStatus.Deleted } },

            {BaseTaskStatus.Deleted, new List<BaseTaskStatus> {  } }
        };

        /// <summary>
        /// Проверка возможности перехода из одного состояния в другое.
        /// </summary>
        /// <param name="current">Текущее состояние</param>
        /// <param name="next">Состояние, в которое нужно осуществить переход</param>
        /// <returns>true - если переход возможен</returns>
        public static bool ValidateTransition(BaseTaskStatus current, BaseTaskStatus next)
        {
            return (Rels[current].Contains(next));
        }
    }

    public static class Utils
    {
        static NLog.Logger logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        /// <summary>
        /// Конвертирование <see cref="BaseTaskStatus"/> из строки.
        /// </summary>
        /// <param name="statusString">Строка, содержащая либо int-представление статуса, либо строка (название) статуса.</param>
        /// <returns>В случае ошибки - статус <see cref="BaseTaskStatus.None"/></returns>
        public static BaseTaskStatus StatusFromString(string statusString)
        {
            // если там цифровое представление
            int intStatus = -1;
            BaseTaskStatus status = BaseTaskStatus.None;

            try
            {
                if (int.TryParse(statusString, out intStatus))
                {
                    return (BaseTaskStatus)intStatus;
                }

                // если там строкове представление
                Enum.TryParse<BaseTaskStatus>(statusString, out status);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Не удалось получить статус из строки {statusString}");
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }
            return status;
        }

        /// <summary>
        /// Конвертирование <see cref="BaseTaskStatus"/> в строку, содержащую название статуса.
        /// </summary>
        /// <param name="status">Статус</param>
        /// <returns>В случае ошибки - пустая строка</returns>
        public static string StatusToString(BaseTaskStatus status)
        {
            try
            {
                return Enum.GetName(typeof(BaseTaskStatus), status);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Не удалось получить имя для статуса {(int)status}");
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }
            return "";
        }
    }
}
