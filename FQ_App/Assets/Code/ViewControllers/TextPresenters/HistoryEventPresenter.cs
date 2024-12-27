using System;
using System.Collections.Generic;
using UnityEngine;
using Code.Models.REST.Rewards;
using Code.Models;
using System.Linq;
using Code.Models.REST;
using static Assets.Code.Models.Reward.BaseReward;
using Code.Models.REST.HistoryEvent;
using Code.ViewControllers;

public class HistoryEventPresenter : MonoBehaviour, ITextPresenter
{
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

            //int intValue = int.MinValue;
            //List<string> keys = new List<string>(textFields.Keys);
            //foreach (var key in keys)
            //{
            //    // не заданные числовые значения
            //    if (int.TryParse(textFields[key]?.ToString(), out intValue) &&
            //        intValue < 0)
            //        presentedText[key] = "";
            //}

            presentedText.Add("MessageTitle", presentedText["EventTitle"]);
            presentedText.Add("MessageBody", presentedText["EventText"]);

            if (Int32.TryParse(presentedText["CreationDateMinAgo"], out int creationDateMinAgo) && creationDateMinAgo > -1)
            {
                if (creationDateMinAgo < (24 * 60))
                {
                    presentedText["CreationDate"] = RoundTime(TimeSpan.FromMinutes(creationDateMinAgo));
                }
                else
                {
                    presentedText["CreationDate"] = DateTimePickerController.GetTextFromDate(DateTime.Parse(presentedText["CreationDate"]), true, false);
                }
            }
            else
            {
                presentedText["CreationDate"] = "-";
            }

            //presentedText["CreationDate"] = RoundTime(new TimeSpan((DateTime.UtcNow - DateTime.Parse(presentedText["CreationDate"])).Ticks));

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
        if (timeSpanValue.TotalDays >= 1)
            presentedText = $"{timeSpanValue.Days} д" + (timeSpanValue.Hours != 0 ? $" и {timeSpanValue.Hours} ч" : "");
        else if (timeSpanValue.TotalHours >= 1)
            presentedText = $"{timeSpanValue.Hours} ч" + (timeSpanValue.Minutes != 0 ? $" и {timeSpanValue.Minutes} м" : "");
        else
            presentedText = $"{timeSpanValue.Minutes} м";

        return presentedText;
    }

    private static string RoundTime(TimeSpan timeSpanValue)
    {
        string presentedText = "";
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
                                presentedText += " минуту";
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
                    presentedText += "1 минуту";
                }
            }
        }
        //presentedText += " назад";

        return presentedText;
    }
}
