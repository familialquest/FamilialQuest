using Ricimi;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Code.Controllers.MessageBox;
using Assets.Code.Controllers;
using Code.Models.REST;
using Code.Models.REST.Rewards;
using System;
using Code.Models;
using System.Linq;
using Code.Models.REST.Users;
using Code.Controllers;
using Code.ViewControllers.TList;
using static Code.Models.TaskModel;
using static Code.Models.RewardModel;
using static Code.Models.CredentialsModel;

public class PopupGroupStatusSelectorOnlineGroupPageController : MonoBehaviour
{
    [HideInInspector]
    public bool readOnlyMode = false;

    public GameObject[] SelectedIcons;

    private Popup m_thisPopup;

    public Dictionary<OnlineStatusFilter, bool> selectedStatuses;

    public delegate void AfterCredentialsSelected();
    private AfterCredentialsSelected m_afterTaskStatusSelectedDelegate = null;
    public AfterCredentialsSelected AfterTaskStatusSelectedDelegate { get => m_afterTaskStatusSelectedDelegate; set => m_afterTaskStatusSelectedDelegate = value; }

    void Start()
    {
        try
        {
            m_thisPopup = GetComponent<Popup>();

            //selectedStatuses = new Dictionary<AdminAvailableTaskFilter, bool>();

            //selectedStatuses.Add(AdminAvailableTaskFilter.All, false);
            //selectedStatuses.Add(AdminAvailableTaskFilter.Draft, false);
            //selectedStatuses.Add(AdminAvailableTaskFilter.Announced, false);

        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }

    public void SetValues(Dictionary<OnlineStatusFilter, bool> statuses)
    {
        selectedStatuses = statuses;

        foreach (var status in statuses)
        {            
            SelectedIcons[(int)status.Key].SetActive(status.Value);
        }
    }

    public void OnClick_ButtonSelectTaskStatus(int filter)//(AdminAvailableTaskFilter filter)
    {
        try
        {
            switch ((OnlineStatusFilter)filter)
            {
                case OnlineStatusFilter.All:
                    {
                        selectedStatuses[OnlineStatusFilter.All] = !selectedStatuses[OnlineStatusFilter.All];
                        selectedStatuses[OnlineStatusFilter.Online] = selectedStatuses[OnlineStatusFilter.All];
                        selectedStatuses[OnlineStatusFilter.NotOnline] = selectedStatuses[OnlineStatusFilter.All];

                        break;
                    }
                case OnlineStatusFilter.Online:
                    {                        
                        selectedStatuses[OnlineStatusFilter.Online] = !selectedStatuses[OnlineStatusFilter.Online];

                        if (selectedStatuses[OnlineStatusFilter.Online] && 
                            selectedStatuses[OnlineStatusFilter.NotOnline])
                        {
                            selectedStatuses[OnlineStatusFilter.All] = true;
                        }
                        else
                        {
                            selectedStatuses[OnlineStatusFilter.All] = false;
                        }

                        break;
                    }
                case OnlineStatusFilter.NotOnline:
                    {
                        selectedStatuses[OnlineStatusFilter.NotOnline] = !selectedStatuses[OnlineStatusFilter.NotOnline];

                        if (selectedStatuses[OnlineStatusFilter.Online] &&
                            selectedStatuses[OnlineStatusFilter.NotOnline])
                        {
                            selectedStatuses[OnlineStatusFilter.All] = true;
                        }
                        else
                        {
                            selectedStatuses[OnlineStatusFilter.All] = false;
                        }

                        break;
                    }
            }

            foreach (var status in selectedStatuses)
            {
                SelectedIcons[(int)status.Key].SetActive(status.Value);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }

    public void OnClick_ButtonChange()
    {
        try
        {
            if (!selectedStatuses[OnlineStatusFilter.All] && 
                !selectedStatuses[OnlineStatusFilter.Online] && 
                !selectedStatuses[OnlineStatusFilter.NotOnline])
            {
                selectedStatuses[OnlineStatusFilter.All] = true;
                selectedStatuses[OnlineStatusFilter.Online] = true;
                selectedStatuses[OnlineStatusFilter.NotOnline] = true;
            }

            ReturnAndClose();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }
    
    private void ReturnAndClose()
    {
        try
        {
            if (m_afterTaskStatusSelectedDelegate != null)
                m_afterTaskStatusSelectedDelegate();
            m_thisPopup.Close();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }
}
