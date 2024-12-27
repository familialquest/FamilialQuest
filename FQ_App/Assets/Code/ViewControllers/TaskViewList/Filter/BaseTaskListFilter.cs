using System;
using UnityEngine;
using Code.ViewControllers.TList;
using System.Collections.Generic;
using static Code.Models.TaskModel;
using Code.Models.REST.CommonType.Tasks;

namespace Code.ViewControllers
{
    public class BaseTaskListFilter : ListFilter
    {
        public BaseTaskFilter DefaultActiveFilter;

        [HideInInspector]
        public BaseTaskFilter CurrentActiveFilter;

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
                if (CurrentActiveFilter == (BaseTaskFilter)current)
                    return;

                SetSelectedColor(current);

                CurrentActiveFilter = (BaseTaskFilter)current;
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
            try
            {
                if (!(item is Dictionary<string, object>))
                    return false;

                Dictionary<string, object> dict = (Dictionary<string, object>)item;

                BaseTaskStatus itemStatus = Utils.StatusFromString(dict["Status"].ToString());

                if (BaseFilterToStatus[CurrentActiveFilter].Contains(itemStatus))
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }
    }
}