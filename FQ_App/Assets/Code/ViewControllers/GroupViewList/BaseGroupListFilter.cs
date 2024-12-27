using Code.Models.RoleModel;
using Code.ViewControllers.TList;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Code.Models.CredentialsModel;

namespace Code.ViewControllers
{
    public class BaseGroupListFilter : ListFilter
    {
        public RoleTypes DefaultActiveFilter;

        [HideInInspector]
        public RoleTypes CurrentActiveFilter;

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
                if (CurrentActiveFilter == (RoleTypes)current)
                    return;

                SetSelectedColor(current);

                CurrentActiveFilter = (RoleTypes)current;
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