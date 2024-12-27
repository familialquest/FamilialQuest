using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ParameterValueEdit_Controller : MonoBehaviour
{
    public GameObject InputField;
    public Toggle Toggle;

    public int MinSize = 100;
    public int MaxSize = 190;

    public RectTransform Rect;

    public LayoutElement Element;
    private void Awake()
    {
    }
    public void ToggleChange()
    {
        InputField.SetActive(Toggle.isOn);
        if (Toggle.isOn)
            Element.minHeight = MaxSize; //m_rect.sizeDelta = new Vector2(m_rect.sizeDelta.x, MaxSize);
        else
            Element.minHeight = MinSize;  //m_rect.sizeDelta = new Vector2(m_rect.sizeDelta.x, MinSize);
    }
}
