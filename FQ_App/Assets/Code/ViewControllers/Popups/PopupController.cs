using Ricimi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupController : ScriptableObject
{
    private static PopupController _instance = null;

    public static PopupController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = ScriptableObject.CreateInstance<PopupController>();
            }
            return _instance;
        }
    }
    private Stack<GameObject> m_popupStack;

    public void Awake()
    {
        m_popupStack = new Stack<GameObject>();
    }

    public void Push(GameObject popup, bool hidePrevious, bool closePrevious)
    {
        if (m_popupStack.Count != 0 && closePrevious)
        {
            m_popupStack.Pop().GetComponent<Popup>().Close();
        }
        else if (m_popupStack.Count != 0 && hidePrevious)
        {
            m_popupStack.Peek().SetActive(false);
        }
        m_popupStack.Push(popup);
    }

    public void Pop()
    {
        try
        {
            if (m_popupStack.Count != 0)
            {
                m_popupStack.Pop();
            }
            if (m_popupStack.Count != 0)
            {
                m_popupStack.Peek().SetActive(true);
            }
        }
        catch
        {
            
        }
    }
}
