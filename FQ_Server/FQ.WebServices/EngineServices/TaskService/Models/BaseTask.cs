using CommonLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TaskService.Models
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
        /// Десериализация из JSON
        /// </summary>
        /// <param name="data">Строка JSON</param>
        /// <returns></returns>
        public static BaseTask TryDeserialize(string data)
        {
            BaseTask task = new BaseTask();

            try
            {
                var tempList = JsonConvert.DeserializeObject<Dictionary<string, string>>(data.ToString());

                List<Guid> availableFor = new List<Guid>();
                DateTime availableUntil = CommonData.dateTime_FQDB_MinValue;
                TimeSpan solutionTime = TimeSpan.Zero;

                try
                {
                    availableFor = JsonConvert.DeserializeObject<List<Guid>>(tempList["AvailableFor"]);
                }
                catch
                {

                }

                try
                {
                    availableUntil = JsonConvert.DeserializeObject<DateTime>(tempList["AvailableUntil"]);
                }
                catch
                {

                }

                try
                {
                    solutionTime = JsonConvert.DeserializeObject<TimeSpan>(tempList["SolutionTime"]);
                }
                catch
                {

                }

                tempList.Remove("AvailableFor");
                tempList.Remove("AvailableUntil");
                tempList.Remove("SolutionTime");

                var tempData = JsonConvert.SerializeObject(tempList);

                task = JsonConvert.DeserializeObject<BaseTask>(tempData);

                task.AvailableFor = availableFor.ToArray();
                task.AvailableUntil = availableUntil;
                task.SolutionTime = solutionTime;

                return task;
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка разбора входных данных", ex);
            }
        }

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
        /// Функция получает дикт всех полей.         
        /// </summary>
        /// <param name="task">Задача</param>
        /// <returns></returns>
        public static Dictionary<string, object> ToDictionaryFull(BaseTask task)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict.Add("Id", task.Id);
            dict.Add("Type", (int)task.Type);
            dict.Add("Name", task.Name);
            dict.Add("Description", task.Description);
            dict.Add("Cost", task.Cost);
            dict.Add("Penalty", task.Penalty);
            dict.Add("AvailableUntil", task.AvailableUntil);
            dict.Add("SolutionTime", task.SolutionTime);
            dict.Add("SpeedBonus", task.SpeedBonus);
            dict.Add("OwnerGroup", task.OwnerGroup);
            dict.Add("AvailableFor", task.AvailableFor);
            dict.Add("Creator", task.Creator);
            dict.Add("Executor", task.Executor);
            dict.Add("Status", (int)task.Status);
            dict.Add("CreationDate", task.CreationDate);
            dict.Add("CompletionDate", task.CompletionDate);
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
            BaseTask task = new BaseTask();

            CommonLib.DictUtils.TryGetAndParseGuid(dict, "Id", out task.Id);
            if (CommonLib.DictUtils.TryGetAndParseInt(dict, "Type", out int intValue))
            {
                task.Type = (BaseTaskType)intValue;
            }
            dict.TryGetValue("Name", out task.Name);
            dict.TryGetValue("Description", out task.Description);
            CommonLib.DictUtils.TryGetAndParseInt(dict, "Cost", out task.Cost);
            CommonLib.DictUtils.TryGetAndParseInt(dict, "Penalty", out task.Penalty);
            CommonLib.DictUtils.TryGetAndParseDateTime(dict, "AvailableUntil", out task.AvailableUntil);
            CommonLib.DictUtils.TryGetAndParseTimeSpan(dict, "SolutionTime", out task.SolutionTime);
            CommonLib.DictUtils.TryGetAndParseInt(dict, "SpeedBonus", out task.SpeedBonus);
            CommonLib.DictUtils.TryGetAndParseGuid(dict, "OwnerGroup", out task.OwnerGroup);
            CommonLib.DictUtils.TryGetAndParseGuidArray(dict, "AvailableFor", out task.AvailableFor);
            CommonLib.DictUtils.TryGetAndParseGuid(dict, "Creator", out task.Creator);
            CommonLib.DictUtils.TryGetAndParseGuid(dict, "Executor", out task.Executor);
            CommonLib.DictUtils.TryGetAndParseDateTime(dict, "CreationDate", out task.CreationDate);
            CommonLib.DictUtils.TryGetAndParseDateTime(dict, "ModificationTime", out task.ModificationTime);

            return task;
        }
    }
}
