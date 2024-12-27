using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ThemeManager : MonoBehaviour
{
    private static ThemeManager _instance;

    public static ThemeManager Instance
    {
        get
        {
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(this);
        }
    }

    public ThemeResx CurrentTheme;

    public event EventHandler ThemeChanged;


    protected virtual void OnThemeChanged(EventArgs e)
    {
        EventHandler handler = ThemeChanged;
        handler?.Invoke(this, e);
    }

    public void ChangeTheme(string themeName)
    {
        CurrentTheme.SetTheme(themeName);

        OnThemeChanged(EventArgs.Empty);
    }

}
