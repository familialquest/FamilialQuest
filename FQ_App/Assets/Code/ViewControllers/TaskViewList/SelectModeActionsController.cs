using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectModeActionsController : MonoBehaviour
{
    public void Show()
    {
        this.gameObject.SetActive(true);

    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public void Switch()
    {
        this.gameObject.SetActive(!this.gameObject.activeInHierarchy);

    }
}
