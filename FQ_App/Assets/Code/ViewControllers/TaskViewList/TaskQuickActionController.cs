using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TaskQuickActionController : MonoBehaviour
{
    public Button EditButton;
    public Button DeleteButton;
    public Button StatusButton;

    public GameObject StatusWindow;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClickButton_Edit()
    {
        Debug.Log("OnClickButton_Edit");
    }

    public void OnClickButton_Delete()
    {
        Debug.Log("OnClickButton_Delete");

    }

    public void OnClickButton_Status()
    {
        Debug.Log("OnClickButton_Status");

        try
        {
            StatusWindow.SetActive(!StatusWindow.activeInHierarchy);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }

    }

    public void OnClickButton_ChangeStatus(int statusCode)
    {
        Debug.Log($"OnClickButton_ChangeStatus {statusCode}");

    }
}
