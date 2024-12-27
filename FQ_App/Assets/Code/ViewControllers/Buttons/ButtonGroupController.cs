using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonGroupController : MonoBehaviour
{
    public GameObject[] ButtonsGroups;

    public void ShowButton(string name)
    {
        foreach(var button in ButtonsGroups)
        {
            if (button.name.ToLower().Contains(name.ToLower()))
                button.gameObject.SetActive(true);
            else
                button.gameObject.SetActive(false);
        }
    }
}
