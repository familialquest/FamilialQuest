using Code.ViewControllers.TList;
using Ricimi;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Code.ViewControllers
{
    public class CredentialPopupOpener : PopupOpener
    {
        public virtual void OpenPopup(GameObject itemController)
        {
            try
            {
                // базовый метод, чтоб открыть префаб
                base.OpenPopup(out GameObject popup);

                // выполняем дополнительные действия для открытого префаба
                TListItemController listItemController = itemController.GetComponent<TListItemController>();
                var item = popup.GetComponent<TextFieldsFiller>();
                item.SetData(listItemController.Data);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }
    } 
}
