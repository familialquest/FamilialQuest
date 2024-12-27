using System;
using System.Collections.Generic;
using UnityEngine;
using Code.Models.REST.Rewards;
using Code.Models;
using System.Linq;
using Code.Models.REST;
using static Code.Models.CredentialsModel;
using Code.Models.RoleModel;
using Assets.Code.Models.REST.CommonTypes.Common;

public class CredentialPresenter : MonoBehaviour, ITextPresenter
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

            Set_RoleLabel(presentedText);

            Set_CoinsLabel(presentedText);

            Set_LastActionLabel(presentedText);

            return presentedText;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
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

    private static void Set_RoleLabel(Dictionary<string, string> _presentedText)
    {
        string roleLabel = "<неизвестно>";

        switch((RoleTypes)Enum.Parse(typeof(RoleTypes), _presentedText["Role"]))
        {
            case RoleTypes.User:
                {
                    roleLabel = "Отряд героев";
                    break;
                }
            case RoleTypes.Administrator:
                {
                    roleLabel = "Королевский двор";
                    break;
                }
        }

        _presentedText["Role"] = roleLabel;
    }

    private static void Set_CoinsLabel(Dictionary<string, string> _presentedText)
    {
        _presentedText["Coins"] += " монет";
    }

    private static void Set_LastActionLabel(Dictionary<string, string> _presentedText)
    {
        string lastAction = string.Empty;
        string lastActionLabel = "Был в сети <неизвестно>";

        if (Int32.TryParse(_presentedText["LastAction"], out int dtLastAction) && dtLastAction != -1)
        {            
            if (dtLastAction > -1)
            {
                lastAction = dtLastAction.ToString();

                if (dtLastAction > 15)
                {
                    lastActionLabel = string.Format("Был в сети {0}", RoundTime(TimeSpan.FromMinutes(dtLastAction)));
                }
                else
                {
                    lastActionLabel = "В сети";
                }
            }            
        }        

        _presentedText["LastAction"] = lastAction;
        _presentedText["LastActionLabel"] = string.Format("{0}", lastActionLabel);
    }
}
