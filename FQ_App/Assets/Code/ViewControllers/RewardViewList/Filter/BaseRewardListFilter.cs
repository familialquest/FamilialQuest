using Code.ViewControllers.TList;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Code.Models.RewardModel;

namespace Code.ViewControllers
{
    public class BaseRewardListFilter : ListFilter
    {
        public BaseRewardFilter DefaultActiveFilter;

        [HideInInspector]
        public BaseRewardFilter CurrentActiveFilter;

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
                if (CurrentActiveFilter == (BaseRewardFilter)current)
                    return;

                SetSelectedColor(current);

                CurrentActiveFilter = (BaseRewardFilter)current;
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