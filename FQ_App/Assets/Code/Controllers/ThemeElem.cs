using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ThemeElem : MonoBehaviour
{
    private ThemeManager m_themeManager;
    // Start is called before the first frame update
    void Start()
    {
        m_themeManager = ThemeManager.Instance;
        m_themeManager.ThemeChanged += OnThemeChanged;

        ApplyTheme();
    }

    void OnThemeChanged(object sender, EventArgs e)
    {
        ApplyTheme();
    }

    private void ApplyTheme()
    {
        // image
        Image image = GetComponent<Image>();
        if (image != null)
        {
            m_themeManager.CurrentTheme.GetImage(this.name, image);
        }
        // tmp ugui comp
        TextMeshProUGUI tmpUI = GetComponent<TextMeshProUGUI>();
        if (tmpUI != null)
        {
            m_themeManager.CurrentTheme.GetText(this.name, tmpUI);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
