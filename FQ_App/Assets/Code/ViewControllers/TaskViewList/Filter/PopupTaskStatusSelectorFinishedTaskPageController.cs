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

public class PopupTaskStatusSelectorFinishedTaskPageController : MonoBehaviour
{
    [HideInInspector]
    public bool readOnlyMode = false;

    public GameObject[] SelectedIcons;

    private Popup m_thisPopup;

    public Dictionary<FinishedTaskFilter, bool> selectedStatuses;

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

    public void SetValues(Dictionary<FinishedTaskFilter, bool> statuses)
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
            switch ((FinishedTaskFilter)filter)
            {
                case FinishedTaskFilter.All:
                    {
                        selectedStatuses[FinishedTaskFilter.All] = !selectedStatuses[FinishedTaskFilter.All];
                        selectedStatuses[FinishedTaskFilter.Successed] = selectedStatuses[FinishedTaskFilter.All];
                        selectedStatuses[FinishedTaskFilter.Failed] = selectedStatuses[FinishedTaskFilter.All];
                        selectedStatuses[FinishedTaskFilter.SolutionTimeOver] = selectedStatuses[FinishedTaskFilter.All];
                        selectedStatuses[FinishedTaskFilter.Declined] = selectedStatuses[FinishedTaskFilter.All];
                        selectedStatuses[FinishedTaskFilter.Canceled] = selectedStatuses[FinishedTaskFilter.All];
                        selectedStatuses[FinishedTaskFilter.AvailableUntilPassed] = selectedStatuses[FinishedTaskFilter.All];

                        break;
                    }
                case FinishedTaskFilter.Successed:
                    {                        
                        selectedStatuses[FinishedTaskFilter.Successed] = !selectedStatuses[FinishedTaskFilter.Successed];

                        if (selectedStatuses[FinishedTaskFilter.Successed] && 
                            selectedStatuses[FinishedTaskFilter.Failed] &&
                            selectedStatuses[FinishedTaskFilter.SolutionTimeOver] &&
                            selectedStatuses[FinishedTaskFilter.Declined] &&
                            selectedStatuses[FinishedTaskFilter.Canceled] &&
                            selectedStatuses[FinishedTaskFilter.AvailableUntilPassed])
                        {
                            selectedStatuses[FinishedTaskFilter.All] = true;
                        }
                        else
                        {
                            selectedStatuses[FinishedTaskFilter.All] = false;
                        }

                        break;
                    }
                case FinishedTaskFilter.Failed:
                    {
                        selectedStatuses[FinishedTaskFilter.Failed] = !selectedStatuses[FinishedTaskFilter.Failed];

                        if (selectedStatuses[FinishedTaskFilter.Successed] &&
                            selectedStatuses[FinishedTaskFilter.Failed] &&
                            selectedStatuses[FinishedTaskFilter.SolutionTimeOver] &&
                            selectedStatuses[FinishedTaskFilter.Declined] &&
                            selectedStatuses[FinishedTaskFilter.Canceled] &&
                            selectedStatuses[FinishedTaskFilter.AvailableUntilPassed])
                        {
                            selectedStatuses[FinishedTaskFilter.All] = true;
                        }
                        else
                        {
                            selectedStatuses[FinishedTaskFilter.All] = false;
                        }

                        break;
                    }
                case FinishedTaskFilter.SolutionTimeOver:
                    {
                        selectedStatuses[FinishedTaskFilter.SolutionTimeOver] = !selectedStatuses[FinishedTaskFilter.SolutionTimeOver];

                        if (selectedStatuses[FinishedTaskFilter.Successed] &&
                            selectedStatuses[FinishedTaskFilter.Failed] &&
                            selectedStatuses[FinishedTaskFilter.SolutionTimeOver] &&
                            selectedStatuses[FinishedTaskFilter.Declined] &&
                            selectedStatuses[FinishedTaskFilter.Canceled] &&
                            selectedStatuses[FinishedTaskFilter.AvailableUntilPassed])
                        {
                            selectedStatuses[FinishedTaskFilter.All] = true;
                        }
                        else
                        {
                            selectedStatuses[FinishedTaskFilter.All] = false;
                        }

                        break;
                    }
                case FinishedTaskFilter.Declined:
                    {
                        selectedStatuses[FinishedTaskFilter.Declined] = !selectedStatuses[FinishedTaskFilter.Declined];

                        if (selectedStatuses[FinishedTaskFilter.Successed] &&
                            selectedStatuses[FinishedTaskFilter.Failed] &&
                            selectedStatuses[FinishedTaskFilter.SolutionTimeOver] &&
                            selectedStatuses[FinishedTaskFilter.Declined] &&
                            selectedStatuses[FinishedTaskFilter.Canceled] &&
                            selectedStatuses[FinishedTaskFilter.AvailableUntilPassed])
                        {
                            selectedStatuses[FinishedTaskFilter.All] = true;
                        }
                        else
                        {
                            selectedStatuses[FinishedTaskFilter.All] = false;
                        }

                        break;
                    }
                case FinishedTaskFilter.Canceled:
                    {
                        selectedStatuses[FinishedTaskFilter.Canceled] = !selectedStatuses[FinishedTaskFilter.Canceled];

                        if (selectedStatuses[FinishedTaskFilter.Successed] &&
                            selectedStatuses[FinishedTaskFilter.Failed] &&
                            selectedStatuses[FinishedTaskFilter.SolutionTimeOver] &&
                            selectedStatuses[FinishedTaskFilter.Declined] &&
                            selectedStatuses[FinishedTaskFilter.Canceled] &&
                            selectedStatuses[FinishedTaskFilter.AvailableUntilPassed])
                        {
                            selectedStatuses[FinishedTaskFilter.All] = true;
                        }
                        else
                        {
                            selectedStatuses[FinishedTaskFilter.All] = false;
                        }

                        break;
                    }
                case FinishedTaskFilter.AvailableUntilPassed:
                    {
                        selectedStatuses[FinishedTaskFilter.AvailableUntilPassed] = !selectedStatuses[FinishedTaskFilter.AvailableUntilPassed];

                        if (selectedStatuses[FinishedTaskFilter.Successed] &&
                            selectedStatuses[FinishedTaskFilter.Failed] &&
                            selectedStatuses[FinishedTaskFilter.SolutionTimeOver] &&
                            selectedStatuses[FinishedTaskFilter.Declined] &&
                            selectedStatuses[FinishedTaskFilter.Canceled] &&
                            selectedStatuses[FinishedTaskFilter.AvailableUntilPassed])
                        {
                            selectedStatuses[FinishedTaskFilter.All] = true;
                        }
                        else
                        {
                            selectedStatuses[FinishedTaskFilter.All] = false;
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
            if (!selectedStatuses[FinishedTaskFilter.All] && 
                !selectedStatuses[FinishedTaskFilter.Successed] && 
                !selectedStatuses[FinishedTaskFilter.Failed] &&
                !selectedStatuses[FinishedTaskFilter.SolutionTimeOver] &&
                !selectedStatuses[FinishedTaskFilter.Declined] &&
                !selectedStatuses[FinishedTaskFilter.Canceled] &&
                !selectedStatuses[FinishedTaskFilter.AvailableUntilPassed])
            {
                selectedStatuses[FinishedTaskFilter.All] = true;
                selectedStatuses[FinishedTaskFilter.Successed] = true;
                selectedStatuses[FinishedTaskFilter.Failed] = true;
                selectedStatuses[FinishedTaskFilter.SolutionTimeOver] = true;
                selectedStatuses[FinishedTaskFilter.Declined] = true;
                selectedStatuses[FinishedTaskFilter.Canceled] = true;
                selectedStatuses[FinishedTaskFilter.AvailableUntilPassed] = true;
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
