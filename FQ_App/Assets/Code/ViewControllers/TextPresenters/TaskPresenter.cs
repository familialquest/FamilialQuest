using System;
using System.Collections.Generic;
using UnityEngine;
using Code.Models.REST.CommonType.Tasks;
using Code.Models.REST;
using Code.Models;
using System.Linq;
using Newtonsoft.Json;
using System.Globalization;
using Code.Models.RoleModel;
using Code.ViewControllers;
using Assets.Code.Models.REST.CommonTypes.Common;

public class TaskPresenter : MonoBehaviour, ITextPresenter
{
    public int MaximumIntLength = 5;
    /// <summary>
    /// По-хорошему тут выполняется локализация данных в формат языка (например, даты),
    /// перевод или применение из текущей темы "шаблонных" значений полей (например, статуса).        
    /// </summary>
    /// <param name="textFields"></param>
    public Dictionary<string, string> Present(Dictionary<string, object> textFields)
    {
        try
        {
            Dictionary<string, string> presentedText = new Dictionary<string, string>();

            // пробуем конвертнуть все поля в строки по умолчанию
            foreach (var field in textFields)
            {
                try
                {
                    string val = field.Value != null ? field.Value.ToString() : string.Empty;
                    presentedText.Add(field.Key, val);
                }
                catch (Exception ex)
                {

                }
            }

            // далее конвертируем "особенные" поля, 
            // для которых нужно собственное представление
            object value = "";
            if (textFields.TryGetValue("Status", out value))
            {
                presentedText["Status"] = Utils.StatusToString(Utils.StatusFromString(value.ToString()));
            }            
                        
            //Set_AvailableUntil(presentedText, textFields);

            Set_SolutionTime(presentedText, textFields);

            Set_StatusLabel(presentedText, textFields);

            Set_UsersLabels(presentedText, textFields);

            Set_CreatorLabel(presentedText);

            Set_CostAndPenalty(presentedText);

            Set_CreationDate(presentedText, textFields);

            Set_CompletionDate(presentedText, textFields);

            Set_Description(presentedText);

            return presentedText;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }

    private static string RoundTime_old(TimeSpan timeSpanValue)
    {
        string presentedText = "";
        if (timeSpanValue.TotalSeconds < 0)
        {
            presentedText += "- ";
            timeSpanValue = timeSpanValue.Negate();
        }
        if (timeSpanValue.TotalDays >= 1)
            presentedText += $"{timeSpanValue.Days}д " + (timeSpanValue.Hours != 0 ? $"{timeSpanValue.Hours}ч" : "");
        else if (timeSpanValue.TotalHours >= 1)
            presentedText += $"{timeSpanValue.Hours}ч " + (timeSpanValue.Minutes != 0 ? $"{timeSpanValue.Minutes}м" : "");
        else
            presentedText += $"{timeSpanValue.Minutes}м";

        return presentedText;
    }

    private static string RoundTime(TimeSpan timeSpanValue)
    {
        string presentedText = "";

        if (timeSpanValue.TotalSeconds < 0)
        {
            presentedText += "- ";
            timeSpanValue = timeSpanValue.Negate();
        }

        if (timeSpanValue.TotalDays >= 1)
        {
            presentedText = $"{timeSpanValue.Days}";

            var lastNumber = timeSpanValue.Days % 10;
            var lastDecade = timeSpanValue.Days % 100;

            switch (lastNumber)
            {
                case 0:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                    presentedText += " дней";
                    break;
                case 1:
                    if (lastDecade == 11)
                    {
                        presentedText += " дней";
                    }
                    else
                    {
                        presentedText += " день";
                    }
                    break;
                case 2:
                case 3:
                case 4:
                    if (lastDecade >= 12 && lastDecade <= 14)
                    {
                        presentedText += " дней";
                    }
                    else
                    {
                        presentedText += " дня";
                    }
                    break;
            }

        }
        else
        {
            if (timeSpanValue.TotalHours >= 1)
            {
                presentedText = $"{timeSpanValue.Hours}";

                var lastNumber = timeSpanValue.Hours % 10;
                var lastDecade = timeSpanValue.Hours % 100;

                switch (lastNumber)
                {
                    case 0:
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                        presentedText += " часов";
                        break;
                    case 1:
                        if (lastDecade == 11)
                        {
                            presentedText += " часов";
                        }
                        else
                        {
                            presentedText += " час";
                        }
                        break;
                    case 2:
                    case 3:
                    case 4:
                        if (lastDecade >= 12 && lastDecade <= 14)
                        {
                            presentedText += " часов";
                        }
                        else
                        {
                            presentedText += " часа";
                        }
                        break;
                }
            }
            else
            {

                if (timeSpanValue.TotalMinutes >= 1)
                {
                    presentedText = $"{timeSpanValue.Minutes}";

                    var lastNumber = timeSpanValue.Minutes % 10;
                    var lastDecade = timeSpanValue.Minutes % 100;

                    switch (lastNumber)
                    {
                        case 0:
                        case 5:
                        case 6:
                        case 7:
                        case 8:
                        case 9:
                            presentedText += " минут";
                            break;
                        case 1:
                            if (lastDecade == 11)
                            {
                                presentedText += " минут";
                            }
                            else
                            {
                                presentedText += " минута";
                            }
                            break;
                        case 2:
                        case 3:
                        case 4:
                            if (lastDecade >= 12 && lastDecade <= 14)
                            {
                                presentedText += " минут";
                            }
                            else
                            {
                                presentedText += " минуты";
                            }
                            break;
                    } 
                }
                else
                {
                    presentedText = "1 минута";
                }
            }
        }

        return presentedText;
    }

    private static string RoundTime_ShortLabel(TimeSpan timeSpanValue)
    {
        string presentedText = "";

        if (timeSpanValue.TotalDays >= 1)
        {
            if (timeSpanValue.Days >= 1)
            {
                presentedText += $"{timeSpanValue.Days} д";
            }

            if (timeSpanValue.Hours >= 1)
            {
                presentedText += $" {timeSpanValue.Hours} ч";
            }
        }
        else
        {
            if (timeSpanValue.TotalHours >= 1)
            {
                if (timeSpanValue.Hours >= 1)
                {
                    presentedText += $" {timeSpanValue.Hours} ч";
                }

                if (timeSpanValue.Minutes >= 1)
                {
                    presentedText += $" {timeSpanValue.Minutes} мин";
                }
            }
            else
            {
                if (timeSpanValue.TotalMinutes >= 1)
                {
                    if (timeSpanValue.Minutes >= 1)
                    {
                        presentedText += $" {timeSpanValue.Minutes} мин";
                    }
                }
                else
                {
                    presentedText += " 1 мин";
                }
            }
        }

        

        

        return presentedText;
    }

    private static void Set_AvailableUntil(Dictionary<string, string> _presentedText, Dictionary<string, object> _textFields)
    {
        string availableUntil = string.Empty;

        DateTime dateTimeValue = CommonData.dateTime_FQDB_MinValue;

        if (_textFields.TryGetValue("AvailableUntil", out object value))
        {
            if (DateTime.TryParse(value.ToString(), out dateTimeValue) && (dateTimeValue != CommonData.dateTime_FQDB_MinValue))
            {
                availableUntil = dateTimeValue.ToLocalTime().ToString();
            }
        }

        _presentedText["AvailableUntil"] = availableUntil;
    }

    private static void Set_SolutionTime(Dictionary<string, string> _presentedText, Dictionary<string, object> _textFields)
    {
        //TODO: darkmagic AvailableUntil

        string solutionTime = string.Empty;

        DateTime dateTimeValue = CommonData.dateTime_FQDB_MinValue;
        
        if (_textFields.TryGetValue("AvailableUntil", out object value))
        {
            if (DateTime.TryParse(value.ToString(), out dateTimeValue) && (dateTimeValue != CommonData.dateTime_FQDB_MinValue))
            {
                solutionTime = DateTimePickerController.GetTextFromDate(dateTimeValue.ToLocalTime()); // dateTimeValue.ToLocalTime().ToString();
            }
        }

        _presentedText["SolutionTime"] = solutionTime;
    }

    private static void Set_StatusLabel(Dictionary<string, string> _presentedText, Dictionary<string, object> _textFields)
    {
        string statusLabel = string.Empty;

        try
        {
            switch (Utils.StatusFromString(_presentedText["Status"]))
            {
                case BaseTaskStatus.Created:
                    {
                        statusLabel = "Черновик";
                        break;
                    }
                case BaseTaskStatus.Assigned:
                    {
                        statusLabel = "Объявлено";

                        //if (!string.IsNullOrEmpty(_presentedText["AvailableUntil"]))
                        //{
                        //    statusLabel = String.Format("{0}: осталось {1}", statusLabel, _presentedText["AvailableUntil"]);
                        //}

                        break;
                    }
                case BaseTaskStatus.InProgress:
                    {
                        DateTime dateTimeValue = CommonData.dateTime_FQDB_MinValue;

                        //TODO: darkmagic AvailableUntil
                        if (_textFields.TryGetValue("AvailableUntil", out object value) &&
                            DateTime.TryParse(value.ToString(), out dateTimeValue) && 
                            (dateTimeValue != CommonData.dateTime_FQDB_MinValue))
                        {
                            var timeLeft = RoundTime_ShortLabel(dateTimeValue.ToLocalTime() - DateTime.Now);
                            statusLabel = String.Format("Выполняется: осталось {0}", timeLeft);
                        }
                        else
                        {
                            statusLabel = "Выполняется";
                        }

                        break;
                    }
                case BaseTaskStatus.PendingReview:
                    {
                        statusLabel = "Ожидает проверки";

                        break;
                    }
                case BaseTaskStatus.Successed:
                    {
                        statusLabel = "Выполнено";
                        break;
                    }
                case BaseTaskStatus.Failed:
                    {
                        statusLabel = "Провалено";
                        break;
                    }
                case BaseTaskStatus.Declined:
                    {
                        statusLabel = "Отклонено";
                        break;
                    }
                case BaseTaskStatus.Canceled:
                    {
                        statusLabel = "Отменено";
                        break;
                    }
                case BaseTaskStatus.SolutionTimeOver:
                    {
                        statusLabel = "Истекло время выполнения";
                        break;
                    }
                case BaseTaskStatus.AvailableUntilPassed:
                    {
                        statusLabel = "Истекло время отклика";
                        break;
                    }
            }
        }
        catch (Exception)
        {

        }

        _presentedText.Add("StatusLabel_Details", string.Format("{0}", statusLabel));
        _presentedText.Add("StatusLabel", string.Format("{0}", statusLabel));
    }

    private static void Set_UsersLabels(Dictionary<string, string> _presentedText, Dictionary<string, object> _textFields)
    {
        string availableFor = string.Empty;

        string destinationUserLabel = "<неизвестно>";
        string DestinationUsersCount = string.Empty;
        string actualUserLabel = string.Empty;
        string executorLabelHeading = string.Empty;
        string executorLabel = string.Empty;

        // и для кого    
        try
        {
            List<Guid> availableForUsers = new List<Guid>();

            //TODO: (1) - в таком формате приходи после создания задачи. (2) - в таком формате приходит после апдейта задачи.
            try
            {
                //(2)
                availableForUsers = new List<Guid>((Guid[])_textFields["AvailableFor"]);

                //приводим к единообразию типу (1). Потому что сейчас там вместо списка гуидов лежит значение "System.Guid[]"
                availableFor = JsonConvert.SerializeObject(availableForUsers);
            }
            catch
            {
                try
                {
                    //(1)
                    availableForUsers = JsonConvert.DeserializeObject<List<Guid>>(_presentedText["AvailableFor"]);
                    availableFor = _presentedText["AvailableFor"];
                }
                catch
                {
                    //TODO: мб тут выкидывать?
                    ;
                }
            }

            #region destinationUserLabel
            if (availableForUsers.Count == 0)
            {
                destinationUserLabel = "Все";
            }
            else
            {
                List<string> destinationUsersNames = new List<string>();

                try
                {
                    foreach (Guid destinationUserId in availableForUsers)
                    {
                        //Получаем имя пользака
                        var destinationUser = DataModel.Instance.Credentials.Users.Where(x => x.Id == destinationUserId).FirstOrDefault();
                        var destinationUserFullName = String.Format("{0}", destinationUser.Name);
                        destinationUserFullName = destinationUserFullName.Trim();

                        destinationUsersNames.Add(destinationUserFullName);
                    }

                    //Отображение
                    destinationUsersNames = new List<string>(destinationUsersNames.OrderBy(x => x));
                    if (destinationUsersNames.Count() == 1)
                    {
                        destinationUserLabel = destinationUsersNames.First();
                    }
                    else
                    {
                        if (destinationUsersNames.Count() == DataModel.Instance.Credentials.ChildrenUsers.Count)
                        {
                            destinationUserLabel = "Все";
                        }
                        else
                        {
                            destinationUserLabel = string.Join(", ", destinationUsersNames);

                            DestinationUsersCount = destinationUsersNames.Count().ToString();
                        }
                    }
                }
                catch
                {
                    ;
                }
            }

            actualUserLabel = destinationUserLabel;
            #endregion

            #region executorLabel
            switch (Utils.StatusFromString(_presentedText["Status"]))
            {
                case BaseTaskStatus.InProgress:
                case BaseTaskStatus.PendingReview:
                    {
                        executorLabelHeading = "Выполняет";

                        //Получаем имя пользака
                        var executorUser = DataModel.Instance.Credentials.Users.Where(x => x.Id == Guid.Parse(_presentedText["Executor"])).FirstOrDefault();

                        //presentedText["ExecutorLabel"] = String.Format("{0} {1}", executorUser.Title, executorUser.Name);
                        //presentedText["ExecutorLabel"] = presentedText["ExecutorLabel"].Trim();

                        executorLabel = executorUser.Name;
                        actualUserLabel = executorLabel;

                        break;
                    }
                case BaseTaskStatus.Successed:
                case BaseTaskStatus.Failed:
                case BaseTaskStatus.Declined:
                case BaseTaskStatus.Canceled:
                case BaseTaskStatus.SolutionTimeOver:
                    {
                        //Получаем имя пользака
                        var executorUser = DataModel.Instance.Credentials.Users.Where(x => x.Id == Guid.Parse(_presentedText["Executor"])).FirstOrDefault();

                        if (executorUser != null)
                        {
                            //presentedText["ExecutorLabel"] = String.Format("{0} {1}", executorUser.Title, executorUser.Name);
                            //presentedText["ExecutorLabel"] = presentedText["ExecutorLabel"].Trim();

                            executorLabelHeading = "Выполнял";

                            executorLabel = executorUser.Name;
                            actualUserLabel = executorLabel;
                        }

                        break;
                    }
            }
            #endregion
        }
        catch (Exception)
        {

        }

        _presentedText["AvailableFor"] = availableFor;

        _presentedText.Add("ActualUserLabel", actualUserLabel);
        _presentedText.Add("DestinationUsersCount", DestinationUsersCount);
        _presentedText.Add("DestinationUserLabel", destinationUserLabel);
        _presentedText.Add("ExecutorLabelHeading", executorLabelHeading);
        _presentedText.Add("ExecutorLabel", executorLabel);
    }

    private static void Set_CreatorLabel(Dictionary<string, string> _presentedText)
    {
        string creatorLabel = "<неизвестно>";

        try
        {
            var creatorUser = DataModel.Instance.Credentials.Users.Where(x => x.Id == Guid.Parse(_presentedText["Creator"])).FirstOrDefault();

            creatorLabel = String.Format("{0} {1}", creatorUser.Title, creatorUser.Name).Trim();
        }
        catch (Exception)
        {

        }

        _presentedText.Add("CreatorLabel", creatorLabel);
    }

    private static void Set_CostAndPenalty(Dictionary<string, string> _presentedText)
    {
        string costLabel = "<неизвестно>";
        string penaltyLabel = string.Empty; //т.к. параметризованное значение проверяется на string.Empty и скрывается

        #region ParseToInt
        int cost = 0;
        int penalty = 0;

        try
        {
            cost = Convert.ToInt32(_presentedText["Cost"]);
        }
        catch
        {
            ;
        }

        try
        {
            penalty = Convert.ToInt32(_presentedText["Penalty"]);
        }
        catch
        {
            ;
        }
        #endregion

        //Отображение стоимости
        if (cost >= 1000)
        {
            costLabel = String.Format("+{0}", cost.ToString("0,0", CultureInfo.CreateSpecificCulture("el-GR")));
        }
        else
        {
            costLabel = String.Format("+{0}", _presentedText["Cost"]);
        }

        if (penalty >= 1000)
        {
            penaltyLabel = penalty.ToString("0,0", CultureInfo.CreateSpecificCulture("el-GR"));
        }
        else
        {
            penaltyLabel = penalty.ToString();
        }

        //Формальное представление награды и штрафа
        _presentedText.Add("CostLabel", costLabel);
        _presentedText.Add("PenaltyLabel", penaltyLabel);

        _presentedText.Add("CostLabelFormal", "Вознаграждение");
        _presentedText.Add("CostFormal", String.Format("<b>{0}</b>", costLabel));
                
        if (penalty > 0)
        {
            _presentedText["CostLabelFormal"] += "/Штраф";
            _presentedText["CostFormal"] += String.Format("  <color=#857D5F>/</color>  <b><color=#CB7A64>-{0}</color></b>", penaltyLabel);
        }
        else
        {
            if (_presentedText.ContainsKey("Penalty"))
            {
                _presentedText.Remove("Penalty");
            }
        }
    }

    private static void Set_CreationDate(Dictionary<string, string> _presentedText, Dictionary<string, object> _textFields)
    {
        string creationDate = string.Empty;

        if (_textFields.TryGetValue("CreationDate", out object value))
        {
            if (DateTime.TryParse(value.ToString(), out DateTime dateTimeValue) && (dateTimeValue != CommonData.dateTime_FQDB_MinValue))
            {
                creationDate = DateTimePickerController.GetTextFromDate(dateTimeValue.ToLocalTime());
            }
        }

        _presentedText["CreationDate"] = creationDate;
    }

    private static void Set_CompletionDate(Dictionary<string, string> _presentedText, Dictionary<string, object> _textFields)
    {
        string completionDate = string.Empty;

        if (_textFields.TryGetValue("CompletionDate", out object value))
        {
            if (DateTime.TryParse(value.ToString(), out DateTime dateTimeValue) && (dateTimeValue != CommonData.dateTime_FQDB_MinValue))
            {
                completionDate = dateTimeValue.ToString("HH:mm yyyy-MM-dd"); // TODO: формат времени
            }
        }

        _presentedText["CompletionDate"] = completionDate;
    }

    private static void Set_Description(Dictionary<string, string> _presentedText)
    {
        //Для ситуации, когда правим и затираем описание - иначе не перерисовывает содержимое этого поля в Details
        if (!_presentedText.ContainsKey("Description"))
        {
            _presentedText.Add("Description", string.Empty);
        }
    }
}
