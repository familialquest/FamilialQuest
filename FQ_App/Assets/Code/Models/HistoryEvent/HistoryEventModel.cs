using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Code.Controllers;
using Code.Controllers.MessageBox;
using Code.Models.REST;
using Code.Models.REST.HistoryEvent;
using Code.Models.REST.Rewards;
using Code.Models.RoleModel;
using Newtonsoft.Json;
using Proyecto26;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Models
{
    public class HistoryEventModel
    {
        public GameObject NewPushCounterBG = null;
        public TextMeshProUGUI NewPushCounter = null;

        public List<GameObject> ShowNewItemsButtons = new List<GameObject>();

        public enum BaseHistoryEventFilter
        {
            All = 0,
            Task,
            Reward,
            User
        }       

        public event EventHandler<EventArgs> OnListChanged;

        protected virtual void ListChanged(EventArgs e)
        {
            EventHandler<EventArgs> handler = OnListChanged;
            handler?.Invoke(this, e);
        }

        //Метод для принудительного обновления UI страницы извне
        public void UpdatePageUI()
        {
            ListChanged(EventArgs.Empty);
        }

        public HistoryEventModel() { }
        
        private List<HistoryEvent> _historyEvents = null;
        public List<HistoryEvent> HistoryEvents { get => _historyEvents; set => _historyEvents = value; }

        public List<HistoryEvent> TaskHistoryEvents
        {
            get
            {
                if (HistoryEvents != null)
                {
                    return HistoryEvents.FindAll((item) => (item.ItemType == HistoryEvent.ItemTypeEnum.Task));
                }
                else
                {
                    return new List<HistoryEvent>();
                }
            }
        }

        public List<HistoryEvent> RewardHistoryEvents
        {
            get
            {
                if (HistoryEvents != null)
                {
                    return HistoryEvents.FindAll((item) => (item.ItemType == HistoryEvent.ItemTypeEnum.Reward));
                }
                else
                {
                    return new List<HistoryEvent>();
                }
            }
        }

        public List<HistoryEvent> UserHistoryEvents
        {
            get
            {
                if (HistoryEvents != null)
                {
                    return HistoryEvents.FindAll((item) => (item.ItemType == HistoryEvent.ItemTypeEnum.User));
                }
                else
                {
                    return new List<HistoryEvent>();
                }
            }
        }

        /// <summary>
        /// Функция получает список наград списком диктов. Нужно для отображения в списке. 
        /// Списки мои заточены под списки диктов.
        /// </summary>
        /// <param name="HistoryEvent">HistoryEvent</param>
        /// <returns></returns>
        public static List<Dictionary<string, object>> ToListOfDictionary<T>(List<T> objectList)
        {
            if (objectList == null)
            {
                return new List<Dictionary<string, object>>();
            }

            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            foreach (var item in objectList)
            {
                list.Add(item.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(item, null)));
            }
            return list;
        }

        public RSG.IPromise<DataModelOperationResult> GetHistoryEvents(
            HistoryEvent conditionHistoryEvent = null,
            int count = 0,
            DateTime? toDate = null)
        {
            GetHistoryEventsRequest req = new GetHistoryEventsRequest(conditionHistoryEvent, count, toDate);

            var prom = RestClientEx.PostEx(req.request)
                .Then((res) => DataModelOperationResult.Wrap(res.RawResponse, new GetHistoryEventsResponse(res.RawResponse)))
                .Catch((ex) => DataModelOperationResult.Wrap(ex));

            return prom;                        
        }
     
        public RSG.IPromise<DataModelOperationResult> UpdateHistoryEventsItems()
        {
            var prom = GetHistoryEvents(null, 100)
                .Then((res) =>
                {
                    if (res.result)
                    {
                        ClearPushCount();

                        Debug.Log(((GetHistoryEventsResponse)res.ParsedResponse).HistoryEvents?.Count);
                        HistoryEvents = ((GetHistoryEventsResponse)res.ParsedResponse).HistoryEvents;
                        ListChanged(EventArgs.Empty);
                    }                    

                    return res;
                });

            return prom;
        }

        public void AddPushCount(int value)
        {
            try
            {
                if (NewPushCounter != null)
                {
                    NewPushCounter.gameObject.SetActive(true);
                    NewPushCounterBG.SetActive(true);

                    int currentValue = 0;

                    if (!string.IsNullOrEmpty(NewPushCounter.text))
                    {
                        Int32.TryParse(NewPushCounter.text, out currentValue);
                    }

                    currentValue += value;

                    if (currentValue > 99)
                    {
                        currentValue = 99;
                    }

                    NewPushCounter.text = string.Format("{0}", currentValue);

                    foreach (var button in ShowNewItemsButtons)
                    {
                        if (button != null)
                        {
                            button.SetActive(true);
                        }
                    }
                }
            }
            catch
            {
                //не так критично, чтобы выкидывать ошибку   
            }
        }

        public void ClearPushCount()
        {
            try
            {
                if (NewPushCounter != null)
                {
                    NewPushCounter.text = string.Empty;
                    NewPushCounter.gameObject.SetActive(false);
                    NewPushCounterBG.SetActive(false);
                }

                foreach (var button in ShowNewItemsButtons)
                {
                    if (button != null)
                    {
                        button.SetActive(false);
                    }
                }
            }
            catch
            {
                //не так критично, чтобы выкидывать ошибку   
            }
        }
    }
}
