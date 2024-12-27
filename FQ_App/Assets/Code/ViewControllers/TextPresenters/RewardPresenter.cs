using System;
using System.Collections.Generic;
using UnityEngine;
using Code.Models.REST.Rewards;
using Code.Models;
using System.Linq;
using Code.Models.REST;
using static Assets.Code.Models.Reward.BaseReward;
using System.Globalization;
using Code.Models.REST.Users;

public class RewardPresenter : MonoBehaviour, ITextPresenter
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

            Set_Creator(presentedText);

            Set_AvailableFor(presentedText, out User destinationUser);

            Set_CostLabel(presentedText, out int cost);

            Set_Status(presentedText, cost, destinationUser);

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

        presentedText += " назад";

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
        }

        presentedText += " назад";

        return presentedText;
    }

    private static void Set_Creator(Dictionary<string, string> _presentedText)
    {
        string creatorName = "<неизвестно>";

        var creator = DataModel.Instance.Credentials.Users.Where(x => x.Id == Guid.Parse(_presentedText["Creator"])).FirstOrDefault();

        if (creator != null)
        {
            creatorName = creator.Name;
        }

        _presentedText["Creator"] = String.Format("{0} {1}", creator.Title, creator.Name).Trim(); ;
    }

    private static void Set_AvailableFor(Dictionary<string, string> _presentedText, out User destinationUser)
    {
        string availableForName = "<неизвестно>";

        destinationUser = DataModel.Instance.Credentials.Users.Where(x => x.Id == Guid.Parse(_presentedText["AvailableFor"])).FirstOrDefault();

        //Если найден целевой пользователя, для кого назначена награда
        if (destinationUser != null)
        {
            availableForName = destinationUser.Name;
        }

        _presentedText["AvailableFor"] = availableForName;
    }

    private static void Set_CostLabel(Dictionary<string, string> _presentedText, out int cost)
    {
        string costLabel = "<неизвестно>";

        cost = Convert.ToInt32(_presentedText["Cost"]);       

        if (cost >= 1000)
        {
            costLabel = cost.ToString("0,0", CultureInfo.CreateSpecificCulture("el-GR"));
        }
        else
        {
            costLabel = _presentedText["Cost"];
        }

        _presentedText.Add("CostLabel", costLabel);
    }

    private static void Set_Status(Dictionary<string, string> _presentedText, int cost, User destinationUser)
    {
        var status = (BaseRewardStatus)Enum.Parse(typeof(BaseRewardStatus), _presentedText["Status"]);

        _presentedText.Add("StatusLabel", string.Empty);

        //Чтобы прокинуть инфу о дополнительном параметре в RewardDetailsController
        _presentedText.Add("CanPurchase", "false");

        switch (status)
        {
            case BaseRewardStatus.Registered:
                {
                    _presentedText["StatusLabel"] = "Доступно";
                    _presentedText["CreationDate"] = RoundTime(new TimeSpan((DateTime.UtcNow - DateTime.Parse(_presentedText["CreationDate"])).Ticks));
                    _presentedText.Remove("PurchaseDate");
                    _presentedText.Remove("HandedDate");

                    //Если найден целевой пользователя, для кого назначена награда
                    if (destinationUser != null)
                    {                        
                        if (destinationUser.Coins >= cost)
                        {
                            _presentedText["CanPurchase"] = "true";
                            _presentedText["StatusLabel"] = "Доступно";
                        }
                        else
                        {
                            _presentedText["CanPurchase"] = "false";
                            var currentCoinsPercents = (int)(((double)destinationUser.Coins / cost) * 100);
                            _presentedText["StatusLabel"] = String.Format("Накоплено: {0}%", currentCoinsPercents);
                        }                                    
                    }
                    else
                    {
                        _presentedText["Creator"] = "<неизвестно>";
                    }

                    break;
                }
            case BaseRewardStatus.Purchase:
                {
                    _presentedText["StatusLabel"] = "Приобретено";
                    _presentedText["CreationDate"] = RoundTime(new TimeSpan((DateTime.UtcNow - DateTime.Parse(_presentedText["CreationDate"])).Ticks));
                    _presentedText["PurchaseDate"] = RoundTime(new TimeSpan((DateTime.UtcNow - DateTime.Parse(_presentedText["PurchaseDate"])).Ticks));
                    _presentedText.Remove("HandedDate");

                    break;
                }
            case BaseRewardStatus.Handed:
                {
                    _presentedText["StatusLabel"] = "Получено";
                    _presentedText["CreationDate"] = RoundTime(new TimeSpan((DateTime.UtcNow - DateTime.Parse(_presentedText["CreationDate"])).Ticks));
                    _presentedText["PurchaseDate"] = RoundTime(new TimeSpan((DateTime.UtcNow - DateTime.Parse(_presentedText["PurchaseDate"])).Ticks));
                    _presentedText["HandedDate"] = RoundTime(new TimeSpan((DateTime.UtcNow - DateTime.Parse(_presentedText["HandedDate"])).Ticks));

                    break;
                }
        }        

        _presentedText.Add("StatusLabel_Details", string.Format("{0}", _presentedText["StatusLabel"]));
        _presentedText["StatusLabel"] = string.Format("{0}", _presentedText["StatusLabel"]);
    }
}
