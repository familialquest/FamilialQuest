using Code.Models;
using Code.Models.REST.Users;
using Code.ViewControllers.TList;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Item_GroupListSimple_Controler : MonoBehaviour
{
    private TextFieldsFiller m_textFieldsFiller;

    public GameObject OnlineIcon;
    public GameObject SelectedIcon;

    [HideInInspector]
    public bool readOnlyMode = false;

    void Start()
    {
        try
        {
            m_textFieldsFiller = GetComponent<TextFieldsFiller>();

            //Отображение 
            if (SelectedIcon != null)
            {
                bool selected = Convert.ToBoolean(m_textFieldsFiller.Data["Selected"]);

                if (selected)
                {
                    SelectedIcon.SetActive(true);
                }
                else
                {
                    SelectedIcon.SetActive(false);
                }
            }

            if (OnlineIcon != null)
            {
                if (m_textFieldsFiller.TextData.ContainsKey("LastAction") && Int32.TryParse(m_textFieldsFiller.TextData["LastAction"], out int lastActionMinutesAgo))
                {
                    if (lastActionMinutesAgo >= 0 && lastActionMinutesAgo <= 15)
                    {
                        OnlineIcon.SetActive(true);
                        return;
                    }
                }

                OnlineIcon.SetActive(false);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }

    //Select-методы используются только в Item_UserSelectorBaseList
    public virtual void OnClick_Select(GameObject itemController)
    {
        try
        {
            if (readOnlyMode)
            {
                return;
            }

            var m_textFieldsFiller = GetComponent<TextFieldsFiller>();

            var updatedItemId = Guid.Parse(m_textFieldsFiller.TextData["Id"]);
            var updatedCredentials = DataModel.Instance.Credentials.Users.Where(x => x.Id == updatedItemId).FirstOrDefault();

            if (updatedCredentials != null)
            {
                updatedCredentials.Selected = !updatedCredentials.Selected;
                DataModel.Instance.Credentials.ListChanged(EventArgs.Empty);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }

    public virtual void OnClick_SelectAll()
    {
        try
        {
            if (readOnlyMode)
            {
                return;
            }

            bool selectedAll = true;

            foreach (var user in DataModel.Instance.Credentials.ChildrenUsers)
            {
                if (!user.Selected)
                {
                    selectedAll = false;
                    break;
                }
            }

            foreach (var updatedCredentials in DataModel.Instance.Credentials.ChildrenUsers)
            {
                updatedCredentials.Selected = !selectedAll;
            }

            DataModel.Instance.Credentials.ListChanged(EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }
}
