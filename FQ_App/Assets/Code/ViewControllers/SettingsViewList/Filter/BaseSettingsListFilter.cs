using Code.ViewControllers.TList;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Code.Models.RewardModel;

namespace Code.ViewControllers
{
    public enum BaseSettingsFilter
    {
        Info = 0,
        Subscription,
        Params
    }

    public class BaseSettingsListFilter : ListFilter
    {
        public BaseSettingsFilter DefaultActiveFilter;

        [HideInInspector]
        public BaseSettingsFilter CurrentActiveFilter;

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
                if (CurrentActiveFilter == (BaseSettingsFilter)current)
                    return;

                SetSelectedColor(current);

                CurrentActiveFilter = (BaseSettingsFilter)current;
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