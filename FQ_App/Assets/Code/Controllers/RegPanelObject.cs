using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegPanelObject : MonoBehaviour
{
    private string login;
    private string password;
    private string email;

    public string Password { get => password; set => password = value; }
    public string Login { get => login; set => login = value; }
    public string Email { get => email; set => email = value; }

    // Start is called before the first frame update
    void Start()
    {

    }
}
