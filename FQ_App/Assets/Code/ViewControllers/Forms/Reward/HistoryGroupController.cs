using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HistoryGroupController : MonoBehaviour
{
    public GameObject[] StatusGroups;

    public void ShowStatus(string name)
    {
        foreach(var status in StatusGroups)
        {
            if (status.name.ToLower().Contains(name.ToLower()))
                status.gameObject.SetActive(true);            
        }
    }
}
