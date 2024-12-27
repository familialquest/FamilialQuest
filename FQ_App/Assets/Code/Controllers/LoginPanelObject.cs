using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoginPanelObject : MonoBehaviour
{
    private string login;
    private string password;

    public string Password { get => password; set => password = value; }
    public string Login { get => login; set => login = value; }

    // Start is called before the first frame update
    void Start()
    {
        
    }
}
