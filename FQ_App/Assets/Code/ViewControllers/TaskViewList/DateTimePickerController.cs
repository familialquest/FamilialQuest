using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ricimi;

using Code.Controllers.MessageBox;
using Code.Controllers;
using Code.Models.REST.CommonType.Tasks;
using System.Linq;
using Code.Models;
using Newtonsoft.Json;
using Code.Models.REST.Users;

namespace Code.ViewControllers
{
    public class DateTimePickerController : MonoBehaviour
    {
        public Popup m_thisPopup;

        public TMP_Text Date_Prev_Number;
        public TMP_Text Date_Prev_Day;
        public TMP_Text Date_Prev_Month;
        public TMP_Text Date_Prev_Number_2;
        public TMP_Text Date_Prev_Day_2;
        public TMP_Text Date_Prev_Month_2;
        public TMP_Text Time_H_Prev;
        public TMP_Text Time_M_Prev;
        public TMP_Text Time_H_Prev_2;
        public TMP_Text Time_M_Prev_2;

        public TMP_Text Date_Selected_Number;
        public TMP_Text Date_Selected_Day;
        public TMP_Text Date_Selected_Month;
        public TMP_Text Time_H_Selected;
        public TMP_Text Time_M_Selected;

        public TMP_Text Date_Next_Number;
        public TMP_Text Date_Next_Day;
        public TMP_Text Date_Next_Month;
        public TMP_Text Date_Next_Number_2;
        public TMP_Text Date_Next_Day_2;
        public TMP_Text Date_Next_Month_2;
        public TMP_Text Time_H_Next;
        public TMP_Text Time_M_Next;
        public TMP_Text Time_H_Next_2;
        public TMP_Text Time_M_Next_2;

        private DateTime date;
        private int hours;
        private int minutes;

        private static List<string> DayNames;
        private static List<string> MonthNames;

        public delegate void AfterTimeChanged(DateTime selectedDT_Utc, string selectedDT_str);
        private AfterTimeChanged m_AfterTimeChanged = null;
        public AfterTimeChanged AfterTimeChangedDelegate { get => m_AfterTimeChanged; set => m_AfterTimeChanged = value; }

        private void Awake()
        {
            date = DateTime.Now.Date;
            hours = DateTime.Now.Hour;
            minutes = DateTime.Now.Minute;

            InitNames();
            SetData();
        }

        private static void InitNames()
        {
            DayNames = new List<string>();
            DayNames.Add("Вс.");
            DayNames.Add("Пн.");
            DayNames.Add("Вт.");
            DayNames.Add("Ср.");
            DayNames.Add("Чт.");
            DayNames.Add("Пт.");
            DayNames.Add("Сб.");

            MonthNames = new List<string>();
            MonthNames.Add("Янв");
            MonthNames.Add("Фев");
            MonthNames.Add("Мар");
            MonthNames.Add("Апр");
            MonthNames.Add("Май");
            MonthNames.Add("Июн");
            MonthNames.Add("Июл");
            MonthNames.Add("Авг");
            MonthNames.Add("Сен");
            MonthNames.Add("Окт");
            MonthNames.Add("Ноя");
            MonthNames.Add("Дек");
        }

        public static string GetTextFromDate(DateTime targetDate, bool dateOnly = false,  bool showYear = true)
        {
            InitNames();

            string targetDate_str = string.Empty;

            if (!dateOnly)
            {
                targetDate_str = string.Format("{0} {1} {2}, {3} - {4}:{5}",
                                                        DayNames[(int)targetDate.DayOfWeek],
                                                        targetDate.Day,
                                                        MonthNames[targetDate.Month - 1],
                                                        targetDate.Year,
                                                        targetDate.Hour.ToString("D2"),
                                                        targetDate.Minute.ToString("D2"));
            }
            else
            {
                if (showYear)
                {
                    targetDate_str = string.Format("{0} {1} {2}",
                                            targetDate.Day,
                                            MonthNames[targetDate.Month - 1],
                                            targetDate.Year);
                }
                else
                {
                    targetDate_str = string.Format("{0} {1}",
                        targetDate.Day,
                        MonthNames[targetDate.Month - 1]);
                }
            }

            return targetDate_str;
        }

        private void SetData()
        {
            Date_Selected_Day.text = DayNames[(int)date.DayOfWeek];
            Date_Selected_Number.text = date.Day.ToString("D2");
            Date_Selected_Month.text = MonthNames[date.Month - 1];
            Time_H_Selected.text = hours.ToString("D2");
            Time_M_Selected.text = minutes.ToString("D2");

            var dateTime_Prev = date.AddDays(-1);
            Date_Prev_Day.text = DayNames[(int)dateTime_Prev.DayOfWeek];
            Date_Prev_Number.text = dateTime_Prev.Day.ToString("D2");
            Date_Prev_Month.text = MonthNames[dateTime_Prev.Month - 1];
            var dateTime_Prev_2 = date.AddDays(-2);
            Date_Prev_Day_2.text = DayNames[(int)dateTime_Prev_2.DayOfWeek];
            Date_Prev_Number_2.text = dateTime_Prev_2.Day.ToString("D2");
            Date_Prev_Month_2.text = MonthNames[dateTime_Prev_2.Month - 1];
            int hours_prev = (hours - 1) >= 0 ? hours - 1 : 23 ;
            int minutes_prev = (minutes - 1) >= 0 ? minutes - 1 : 59;
            Time_H_Prev.text = hours_prev.ToString("D2");
            Time_M_Prev.text = minutes_prev.ToString("D2");
            int hours_prev_2 = (hours_prev - 1) >= 0 ? hours_prev - 1 : 23;
            int minutes_prev_2 = (minutes_prev - 1) >= 0 ? minutes_prev - 1 : 59;
            Time_H_Prev_2.text = hours_prev_2.ToString("D2");
            Time_M_Prev_2.text = minutes_prev_2.ToString("D2");

            var dateTime_Next = date.AddDays(1);
            Date_Next_Day.text = DayNames[(int)dateTime_Next.DayOfWeek];
            Date_Next_Number.text = dateTime_Next.Day.ToString("D2");
            Date_Next_Month.text = MonthNames[dateTime_Next.Month - 1];
            var dateTime_Next_2 = date.AddDays(2);
            Date_Next_Day_2.text = DayNames[(int)dateTime_Next_2.DayOfWeek];
            Date_Next_Number_2.text = dateTime_Next_2.Day.ToString("D2");
            Date_Next_Month_2.text = MonthNames[dateTime_Next_2.Month - 1];
            int hours_next = (hours + 1) <= 23 ? hours + 1 : 0;
            int minutes_next = (minutes + 1) <= 59 ? minutes + 1 : 0;
            Time_H_Next.text = hours_next.ToString("D2");
            Time_M_Next.text = minutes_next.ToString("D2");
            int hours_next_2 = (hours_next + 1) <= 23 ? hours_next + 1 : 0;
            int minutes_next_2 = (minutes_next + 1) <= 59 ? minutes_next + 1 : 0;
            Time_H_Next_2.text = hours_next_2.ToString("D2");
            Time_M_Next_2.text = minutes_next_2.ToString("D2");
        }

        public void SetData(DateTime selectedDT)
        {
            date = selectedDT.Date;
            hours = selectedDT.Hour;
            minutes = selectedDT.Minute;

            InitNames();
            SetData();
        }

        public void OnClick_ButtonReset()
        {
            date = DateTime.Now;
            SetData();
        }

        public void OnClick_ButtonAdd_Day()
        {
            date = date.AddDays(1);
            SetData();
        }
        public void OnClick_ButtonDeduct_Day()
        {
            date = date.AddDays(-1);
            SetData();
        }

       
        public void OnClick_ButtonAdd_Hour()
        {
            int hours_next = (hours + 1) <= 23 ? hours + 1 : 0;
            hours = hours_next;
            SetData();
        }
        public void OnClick_ButtonDeduct_Hour()
        {
            int hours_prev = (hours - 1) >= 0 ? hours - 1 : 23;
            hours = hours_prev;
            SetData();
        }

        public void OnClick_ButtonAdd_Minute()
        {
            int minutes_next = (minutes + 1) <= 59 ? minutes + 1 : 0;
            minutes = minutes_next;
            SetData();
        }
        public void OnClick_ButtonDeduct_Minute()
        {
            int minutes_prev = (minutes - 1) >= 0 ? minutes - 1 : 59;
            minutes = minutes_prev;
            SetData();
        }

        public void ReturnAndClose()
        {
            try
            {                
                DateTime selectedDT = DateTime.Now.Date.AddDays((date - DateTime.Now.Date).TotalDays);
                selectedDT = selectedDT.AddHours(hours);
                selectedDT = selectedDT.AddMinutes(minutes);

                var selectedDT_Str = string.Format("{0} {1} {2}, {3} - {4}:{5}",
                                                    DayNames[(int)date.DayOfWeek],
                                                    date.Day,
                                                    MonthNames[date.Month - 1],
                                                    date.Year,
                                                    hours.ToString("D2"),
                                                    minutes.ToString("D2"));

                if (m_AfterTimeChanged != null)
                    m_AfterTimeChanged(selectedDT, selectedDT_Str);
                m_thisPopup.Close();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }
    }
}