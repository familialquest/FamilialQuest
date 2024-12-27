using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonRewardGroupController : MonoBehaviour
{
    public GameObject[] ButtonsGroups;

    public void ShowButton(string name)
    {
        try
        {
            foreach (var button in ButtonsGroups)
            {
                if (button.name.ToLower().Contains(name.ToLower()))
                {
                    button.gameObject.SetActive(true);
                }
                else
                {
                    button.gameObject.SetActive(false);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }
}
