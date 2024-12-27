using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Button_Main_Add : MonoBehaviour
{
    public GameObject MenuPanel;
    private Animator m_menuPanelAnimator;

    public Button MainButton;

    // Start is called before the first frame update
    void Start()
    {
        m_menuPanelAnimator = MenuPanel.GetComponent<Animator>();        
    }

    public void OnClick_AddButton()
    {
        SwitchMenu();
    }

    public void OnClick_AnyMenuButton()
    {
        SwitchMenu();
    }

    public void SwitchMenu()
    {
        var oldValue = m_menuPanelAnimator.GetBool("Roll");
        m_menuPanelAnimator.SetBool("Roll", !oldValue);
    }

    public void OnDeselect()
    {        
        m_menuPanelAnimator.SetBool("Roll", false);
    }
}
