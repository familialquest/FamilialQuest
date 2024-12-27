using Code.Models;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.ViewControllers.TList
{
    public enum PageType
    {
        Default = 0,
        Tasks,
        Rewards,
        History
    }

    public class TListViewController : MonoBehaviour
    {
        public RectTransform ItemPrefab;

        //TODO: Для показа в верху списка истории кнопки "Показать новые события"
        public RectTransform ItemPrefab_ShowNewItems;

        //TODO: Для показа внизу списка кнопки "Показать ещё"
        public PageType pType;
        public RectTransform ItemPrefab_ShowMoreItems;
        public int baseItemsCount = 2;
        public int currentItemsCount = 2;

        public ScrollRect Scroll;
        public RectTransform Content;
        public TextMeshProUGUI AmountText;

        [Tooltip("Необязательный компонент")]
        public ListFilter ListFilter;

        private List<TListItemController> m_listItems = new List<TListItemController>();
        private List<TListItemController> m_listSelectedItems = new List<TListItemController>();

        bool m_inSelectMode = false;
        bool m_selectall = true;

        /// <summary>
        /// Сколько нужно проскроллить вниз, чтоб запустить обновление списка
        /// </summary>
        public int SwipeDownLength = 200;

        List<Dictionary<string, object>> m_itemsData;
        public List<Dictionary<string, object>> Items { get => m_itemsData; }

        // чтоб не обновлять одновременно
        bool isOnUpdate = false;

        #region Events
        /// <summary>
        /// Событие необходимости выполнить обновление списка
        /// </summary>
        public event EventHandler OnNeedListRefresh;
        protected virtual void NeedListRefresh(EventArgs e)
        {
            //Для показа дефолтного количества элементов списка при обновлении
            currentItemsCount = baseItemsCount;

            EventHandler handler = OnNeedListRefresh;
            handler?.Invoke(this, e);
        }
        /// <summary>
        /// Событие, что список изменен
        /// </summary>
        public event EventHandler OnListChanged;
        protected virtual void ListChanged(EventArgs e)
        {
            EventHandler handler = OnListChanged;
            handler?.Invoke(this, e);
        }
        #endregion

        #region Event handlers
        private void TListItem_ItemChanged(object sender, EventArgs e)
        {
            try
            {
                ListChanged(EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        private void M_taskListFilter_OnFilterChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateList();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        public void OnDataChanged(object sender, Code.Models.ListChangedEventArgs e)
        {
            try
            {
                SetListItems(e.List);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }
        #endregion

        #region MonoBehaviour
        private void Awake()
        {
            try
            {
                if (ListFilter)
                {
                    ListFilter.OnFilterChanged += M_taskListFilter_OnFilterChanged;
                }
                else
                {
                    ListFilter = null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        private void OnDestroy()
        {
            try
            {
                if (ListFilter)
                    ListFilter.OnFilterChanged -= M_taskListFilter_OnFilterChanged;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        private void Update()
        {
            try
            {
                if (!isOnUpdate && Content.transform.position.y < Content.parent.transform.position.y - SwipeDownLength)
                {
                    isOnUpdate = true;
                    NeedListRefresh(EventArgs.Empty);
                }                

                if (Content.transform.position.y >= Content.parent.transform.position.y - SwipeDownLength)
                {
                    isOnUpdate = false;
                }
            }
            catch (Exception ex)
            {                
                Debug.LogError(ex);
                throw;
            }
        }
        #endregion

        #region ListView operations

        public void OnClick_ShowNewItems()
        {
            NeedListRefresh(EventArgs.Empty);
        }

        //Показать следующую партию элементов списка
        public void OnClick_AddMore()
        {
            if (ItemPrefab_ShowMoreItems != null)
            {
                currentItemsCount += baseItemsCount;

                switch(pType)
                {
                    case PageType.Tasks:
                        {
                            DataModel.Instance.Tasks.UpdatePageUI();
                            break;
                        }
                    case PageType.Rewards:
                        {
                            DataModel.Instance.Rewards.UpdatePageUI();
                            break;
                        }
                    case PageType.History:
                        {
                            DataModel.Instance.HistoryEvents.UpdatePageUI();
                            break;
                        }
                }                
            }
        }

        private void UpdateList()
        {
            try
            {
                isOnUpdate = true;

                CleanView();
                if (m_itemsData == null)
                    return;

                //TODO: для показа в верху списка истории кнопки "Показать новые события"
                if (ItemPrefab_ShowNewItems != null &&
                    DataModel.Instance.HistoryEvents.NewPushCounter != null &&
                    DataModel.Instance.HistoryEvents.NewPushCounter.text != string.Empty)
                {
                    CreateTListItem_ShowNewItems();                    
                }

                int filteredItemsCount = 0;

                //флаг, сигнализирующий о том, что все элементы списка не поместились в текущий предел
                bool itemsLimitAchieved = false;

                foreach (var itemData in m_itemsData)
                {
                    if (ListFilter != null && !ListFilter.FilterItem(itemData))
                    {
                        continue;
                    }
                    
                    //показываем только допустимое количество элементов списка на странице
                    if (ItemPrefab_ShowMoreItems != null &&
                        filteredItemsCount >= currentItemsCount)
                    {
                        itemsLimitAchieved = true;
                        break;
                    }

                    CreateTListItem(itemData);
                    filteredItemsCount++;
                } 
                
                //Если поместились не все элементы, показ кнопки "Показать ещё"
                if (ItemPrefab_ShowMoreItems != null && itemsLimitAchieved)
                {
                    CreateTListItem_ShowMoreItems();
                }

                if (AmountText != null)
                {
                    if (filteredItemsCount > 0)
                    {
                        AmountText.text = $"{filteredItemsCount}";
                    }
                    else
                    {
                        AmountText.text = "-";
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
            finally
            {
                isOnUpdate = false;
                // сбросить прокрутку списка
                Content.transform.position = Content.parent.transform.position;
            }
        }

        private void CreateTListItem(Dictionary<string, object> itemData)
        {
            try
            {
                var item = Instantiate(ItemPrefab.transform);
                item.SetParent(Content.transform, false);
                TListItemController tListItem = item.GetComponent<TListItemController>();
                tListItem.SetData(itemData);
                tListItem.ItemChanged += TListItem_ItemChanged;
                m_listItems.Add(tListItem);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        //TODO: для показа в верху списка истории кнопки "Показать новые события"
        private void CreateTListItem_ShowNewItems()
        {
            try
            {
                var item = Instantiate(ItemPrefab_ShowNewItems.transform);
                item.SetParent(Content.transform, false);
                TListItemController tListItem = item.GetComponent<TListItemController>();
                tListItem.gameObject.SetActive(true);
                tListItem.gameObject.GetComponentInChildren<Button>().onClick.AddListener(OnClick_ShowNewItems);

                m_listItems.Add(tListItem);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }            
        }

        //Показ внизу списка кнопки "Показать ещё"
        private void CreateTListItem_ShowMoreItems()
        {
            try
            {
                var item = Instantiate(ItemPrefab_ShowMoreItems.transform);
                item.SetParent(Content.transform, false);
                TListItemController tListItem = item.GetComponent<TListItemController>();
                tListItem.gameObject.SetActive(true);
                tListItem.gameObject.GetComponentInChildren<Button>().onClick.AddListener(OnClick_AddMore);

                m_listItems.Add(tListItem);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        public void CleanView()
        {
            try
            {
                foreach (Transform child in Content)
                {
                    Destroy(child.gameObject);
                }

                m_listItems.Clear();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        public void UpdateItemView(Transform item, Dictionary<string, object> data)
        {
            try
            {
                TListItemController textFiller = item.GetComponent<TListItemController>();
                textFiller.SetData(data);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        public void SetListItems(List<Dictionary<string, object>> newItems)
        {
            try
            {
                m_itemsData = newItems;
                UpdateList();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }
        #endregion

        #region Selection
        public List<TListItemController> GetSelectedItems()
        {
            try
            {
                List<TListItemController> selected = new List<TListItemController>();
                selected.AddRange(m_listItems.FindAll((s) => s.IsSelected));
                return selected;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }

        }

        public void SetSelectMode(bool enable)
        {
            try
            {
                foreach (var item in m_listItems)
                {
                    item.SetSelectMode(enable);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        public void SwitchSelectMode()
        {
            try
            {
                m_inSelectMode = !m_inSelectMode;
                SetSelectMode(m_inSelectMode);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        public void SwitchSelection()
        {
            try
            {
                if (!m_inSelectMode)
                    return;

                m_selectall = !m_selectall;

                foreach (var item in m_listItems)
                {
                    item.SetSelection(m_selectall);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }
        #endregion
    } 
}
