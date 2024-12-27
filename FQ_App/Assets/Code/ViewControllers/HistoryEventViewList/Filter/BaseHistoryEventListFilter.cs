using Code.ViewControllers.TList;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Code.Models.HistoryEventModel;
using static Code.Models.RewardModel;

namespace Code.ViewControllers
{
    public class BaseHistoryEventListFilter : ListFilter
    {
        public BaseHistoryEventFilter DefaultActiveFilter;

        [HideInInspector]
        public BaseHistoryEventFilter CurrentActiveFilter;

        // Start is called before the first frame update
        void Awake()
        {
            try
            {
                InitializeColors();
                CurrentActiveFilter = DefaultActiveFilter;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        public void OnChangeFilter(int current)
        {
            try
            {
                try
                {
                    if (CurrentActiveFilter == (BaseHistoryEventFilter)current)
                    {
                        return;
                    }

                    SetSelectedColor(current);

                    CurrentActiveFilter = (BaseHistoryEventFilter)current;
                    FilterChanged(EventArgs.Empty);

                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    CurrentActiveFilter = DefaultActiveFilter;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        public override bool FilterItem(object item)
        {
            throw new NotImplementedException();
        }
    }

}