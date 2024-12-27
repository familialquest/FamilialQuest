using Code.ViewControllers.TList;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Code.Models.RewardModel;

namespace Code.ViewControllers
{
    public enum BaseSubscriptionFilter
    {
        m12 = 0,
        m3,
        m1
    }

    public class BaseSubscriptionsListFilter : ListFilter
    {
        public BaseSubscriptionFilter DefaultActiveFilter;

        [HideInInspector]
        public BaseSubscriptionFilter CurrentActiveFilter;

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
                if (CurrentActiveFilter == (BaseSubscriptionFilter)current)
                    return;

                SetSelectedColor(current);

                CurrentActiveFilter = (BaseSubscriptionFilter)current;
                FilterChanged(EventArgs.Empty);

            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                CurrentActiveFilter = DefaultActiveFilter;
            }
        }

        public override bool FilterItem(object item)
        {
            throw new NotImplementedException();
        }
    }

}