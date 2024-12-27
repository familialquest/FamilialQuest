using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelController : MonoBehaviour
{
    public GameObject[] AvailablePanels;
    public GameObject CurrentAvailablePanel;
    // Start is called before the first frame update
    void Start()
    {
        CurrentAvailablePanel.SetActive(true);
    }

    public void Switch(string panelName)
    {
        if (CurrentAvailablePanel.name == panelName)
            return;
        bool isFound = false;

        foreach(var panel in AvailablePanels)
        {
            if (panel.name == panelName)
            {
                isFound = true;
                CurrentAvailablePanel.SetActive(false);
                CurrentAvailablePanel = panel;
                panel.SetActive(true);
                break;
            }
        }

        if (!isFound)
            Debug.LogError($"Panel {panelName} not found!");
    }
}
