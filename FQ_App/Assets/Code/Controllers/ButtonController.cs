using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonController : MonoBehaviour
{
    public PanelController panelController;

    public void OnLoginButtonPressed(GameObject loginPanel)
    {
        var loginPanelObject = loginPanel.GetComponent<LoginPanelObject>();
        if (loginPanelObject != null)
        {
            Debug.Log($"Login: {loginPanelObject.Login} Password: {loginPanelObject.Password}");
        }

        SceneManager.LoadScene("Main");
    }

    public void OnRegisterButtonPressed()
    {
        panelController.Switch("RegPanel");
    }

    public void OnBackFromRegButtonPressed()
    {
        panelController.Switch("LoginPanel");
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
