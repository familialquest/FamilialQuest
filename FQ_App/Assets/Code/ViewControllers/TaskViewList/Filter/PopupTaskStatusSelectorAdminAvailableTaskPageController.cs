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

public class PopupTaskStatusSelectorAdminAvailableTaskPageController : MonoBehaviour
{
    [HideInInspector]
    public bool readOnlyMode = false;

    public GameObject[] SelectedIcons;

    private Popup m_thisPopup;

    public Dictionary<AdminAvailableTaskFilter, bool> selectedStatuses;

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

    public void SetValues(Dictionary<AdminAvailableTaskFilter, bool> statuses)
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
            switch ((AdminAvailableTaskFilter)filter)
            {
                case AdminAvailableTaskFilter.All:
                    {
                        selectedStatuses[AdminAvailableTaskFilter.All] = !selectedStatuses[AdminAvailableTaskFilter.All];
                        selectedStatuses[AdminAvailableTaskFilter.Draft] = selectedStatuses[AdminAvailableTaskFilter.All];
                        selectedStatuses[AdminAvailableTaskFilter.Announced] = selectedStatuses[AdminAvailableTaskFilter.All];

                        break;
                    }
                case AdminAvailableTaskFilter.Draft:
                    {                        
                        selectedStatuses[AdminAvailableTaskFilter.Draft] = !selectedStatuses[AdminAvailableTaskFilter.Draft];

                        if (selectedStatuses[AdminAvailableTaskFilter.Announced] && 
                            selectedStatuses[AdminAvailableTaskFilter.Draft])
                        {
                            selectedStatuses[AdminAvailableTaskFilter.All] = true;
                        }
                        else
                        {
                            selectedStatuses[AdminAvailableTaskFilter.All] = false;
                        }

                        break;
                    }
                case AdminAvailableTaskFilter.Announced:
                    {
                        selectedStatuses[AdminAvailableTaskFilter.Announced] = !selectedStatuses[AdminAvailableTaskFilter.Announced];

                        if (selectedStatuses[AdminAvailableTaskFilter.Announced] && 
                            selectedStatuses[AdminAvailableTaskFilter.Draft])
                        {
                            selectedStatuses[AdminAvailableTaskFilter.All] = true;
                        }
                        else
                        {
                            selectedStatuses[AdminAvailableTaskFilter.All] = false;
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
            if (!selectedStatuses[AdminAvailableTaskFilter.All] && 
                !selectedStatuses[AdminAvailableTaskFilter.Draft] && 
                !selectedStatuses[AdminAvailableTaskFilter.Announced])
            {
                selectedStatuses[AdminAvailableTaskFilter.All] = true;
                selectedStatuses[AdminAvailableTaskFilter.Draft] = true;
                selectedStatuses[AdminAvailableTaskFilter.Announced] = true;
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
