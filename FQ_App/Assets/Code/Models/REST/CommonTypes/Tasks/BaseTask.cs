using System;
using System.Collections.Generic;
using System.Text;
using Assets.Code.Models.REST.CommonTypes.Common;
using Code.Models.REST.CommonTypes;

namespace Code.Models.REST.CommonType.Tasks
{
    /// <summary>
    /// Базовая задача
    /// </summary>
    public class BaseTask
    {
        public Guid Id;                                                         // ID uuid NOT NULL UNIQUE,
        public BaseTaskType Type = BaseTaskType.Base;                           // Type INT NOT NULL,
        public string Name;                                                     // Name TEXT NOT NULL,
        public string Description = String.Empty;                                              // Description TEXT,
        public int Cost = default(int);                                         // Cost INT NOT NULL,
        public int Penalty = default(int);                                      // Penalty INT,
        public DateTime AvailableUntil = CommonData.dateTime_FQDB_MinValue;     // AvailableUntil INT NOT NULL,
        public TimeSpan SolutionTime = TimeSpan.Zero;                           // SolutionTime INT,
        public int SpeedBonus = default(int);                                   // SpeedBonus INT,
        public Guid OwnerGroup = Guid.Empty;                                    // OwnerGroup uuid NOT NULL,
        public Guid[] AvailableFor = Array.Empty<Guid>();                       // Available uuid[],
        public Guid Creator = Guid.Empty;                                       // Creator uuid NOT NULL,
        public Guid Executor = Guid.Empty;                                      // Executor uuid NOT NULL,
        public BaseTaskStatus Status = BaseTaskStatus.None;                     // Status INT NOT NULL,
        public DateTime CreationDate = CommonData.dateTime_FQDB_MinValue;       // CreationDate TIMESTAMP,
        public DateTime CompletionDate = CommonData.dateTime_FQDB_MinValue;     // CompletionDate TIMESTAMP
        public DateTime ModificationTime = CommonData.dateTime_FQDB_MinValue;   // ModificationTime TIMESTAMP

        /// <summary>
        /// Функция заполняет дикт полями задачи, значения которых отличаются от "по умолчанию"
        /// </summary>
        /// <param name="task">Задача</param>
        /// <returns></returns>
        public static Dictionary<string, object> ToDictionary(BaseTask task)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            if (task.Id != Guid.Empty)
                dict.Add("Id", task.Id);
            if (task.Type != BaseTaskType.Base)
                dict.Add("Type", (int)task.Type);
            if (!string.IsNullOrEmpty(task.Name))
                dict.Add("Name", task.Name);
            if (!string.IsNullOrEmpty(task.Description))
                dict.Add("Description", task.Description);
            if (task.Cost != default(int))
                dict.Add("Cost", task.Cost);
            if (task.Penalty != default(int))
                dict.Add("Penalty", task.Penalty);
            if (task.AvailableUntil != CommonData.dateTime_FQDB_MinValue)
                dict.Add("AvailableUntil", task.AvailableUntil);
            if (task.SolutionTime != TimeSpan.Zero)
                dict.Add("SolutionTime", task.SolutionTime);
            if (task.SpeedBonus != default(int))
                dict.Add("SpeedBonus", task.SpeedBonus);
            if (task.OwnerGroup != Guid.Empty)
                dict.Add("OwnerGroup", task.OwnerGroup);
            if (task.AvailableFor != null && task.AvailableFor != Array.Empty<Guid>())
                dict.Add("AvailableFor", task.AvailableFor);
            if (task.Creator != Guid.Empty)
                dict.Add("Creator", task.Creator);
            if (task.Executor != Guid.Empty)
                dict.Add("Executor", task.Executor);
            if (task.Status != BaseTaskStatus.None)
                dict.Add("Status", (int)task.Status);
            if (task.CreationDate != CommonData.dateTime_FQDB_MinValue)
                dict.Add("CreationDate", task.CreationDate);
            if (task.CompletionDate != CommonData.dateTime_FQDB_MinValue)
                dict.Add("CompletionDate", task.CompletionDate);
            if (task.ModificationTime != CommonData.dateTime_FQDB_MinValue)
                dict.Add("ModificationTime", task.ModificationTime);

            return dict;
        }

        /// <summary>
        /// Создание задачи и заполнение значениями из дикта
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static BaseTask FromDictionary(Dictionary<string, string> dict)
        {
            return FromDictionary(dict, out bool result);
        }
        /// <summary>
        /// Создание задачи и заполнение значениями из дикта
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static BaseTask FromDictionary(Dictionary<string, string> dict, out bool result)
        {
            BaseTask task = new BaseTask();

            result = DictUtils.TryGetAndParseGuid(dict, "Id", out task.Id);
            if (result &= DictUtils.TryGetAndParseInt(dict, "Type", out int intValue))
            {
                task.Type = (BaseTaskType)intValue;
            }
            result &= dict.TryGetValue("Name", out task.Name);
            result &= dict.TryGetValue("Description", out task.Description);
            result &= DictUtils.TryGetAndParseInt(dict, "Cost", out task.Cost);
            result &= DictUtils.TryGetAndParseInt(dict, "Penalty", out task.Penalty);
            result &= DictUtils.TryGetAndParseDateTime(dict, "AvailableUntil", out task.AvailableUntil);
            result &= DictUtils.TryGetAndParseTimeSpan(dict, "SolutionTime", out task.SolutionTime);
            result &= DictUtils.TryGetAndParseInt(dict, "SpeedBonus", out task.SpeedBonus);
            result &= DictUtils.TryGetAndParseGuid(dict, "OwnerGroup", out task.OwnerGroup);
            result &= DictUtils.TryGetAndParseGuidArray(dict, "AvailableFor", out task.AvailableFor);
            result &= DictUtils.TryGetAndParseGuid(dict, "Creator", out task.Creator);
            result &= DictUtils.TryGetAndParseGuid(dict, "Executor", out task.Executor);
            result &= DictUtils.TryGetAndParseDateTime(dict, "CreationDate", out task.CreationDate);
            result &= DictUtils.TryGetAndParseDateTime(dict, "ModificationTime", out task.ModificationTime);

            return task;
        }
    }

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
        /// Когда время, выделенное на принятие задача, истекло.
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
            {BaseTaskStatus.Created, new List<BaseTaskStatus> { BaseTaskStatus.Assigned, BaseTaskStatus.Deleted} },
            {BaseTaskStatus.Assigned, new List<BaseTaskStatus> { BaseTaskStatus.Accepted, BaseTaskStatus.Canceled, BaseTaskStatus.Declined, BaseTaskStatus.AvailableUntilPassed} },
            {BaseTaskStatus.Accepted, new List<BaseTaskStatus> { BaseTaskStatus.InProgress, BaseTaskStatus.Canceled, BaseTaskStatus.SolutionTimeOver, BaseTaskStatus.Declined } },
            {BaseTaskStatus.InProgress, new List<BaseTaskStatus> { BaseTaskStatus.Completed, BaseTaskStatus.Canceled, BaseTaskStatus.Declined, BaseTaskStatus.SolutionTimeOver } },
            {BaseTaskStatus.Completed, new List<BaseTaskStatus> { BaseTaskStatus.PendingReview} },
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
        public static BaseTaskStatus StatusFromString(string statusString)
        {
            // если там цифровое представление
            int intStatus = 0;
            if (int.TryParse(statusString, out intStatus))
            {
                return (BaseTaskStatus)intStatus;
            }

            // если там строкове представление
            BaseTaskStatus status = BaseTaskStatus.None;
            Enum.TryParse<BaseTaskStatus>(statusString, out status);
            return status;
        }
        public static string StatusToString(BaseTaskStatus status)
        {
            return Enum.GetName(typeof(BaseTaskStatus), status);
        }
    }
}
