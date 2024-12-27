using System;
using UnityEngine;
using UnityEngine.UI;

public class GroupElementController : MonoBehaviour
{
    public GameObject[] ObjectGroups;

    public void ShowOnlyGroup(string name)
    {
        try
        {
            foreach (var button in ObjectGroups)
            {
                if (button.name.ToLower().Contains(name.ToLower()))
                    button.gameObject.SetActive(true);
                else
                    button.gameObject.SetActive(false);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }

    public void HideGroup(string name)
    {
        try
        {
            foreach (var button in ObjectGroups)
            {
                if (button.name.ToLower().Contains(name.ToLower()))
                {
                    button.gameObject.SetActive(false);
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }

    public void ShowGroup(string name)
    {
        try
        {
            foreach (var button in ObjectGroups)
            {
                if (button.name.ToLower().Contains(name.ToLower()))
                {
                    button.gameObject.SetActive(true);
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }

    public void SetVisible(string name, bool visible)
    {
        try
        {
            if (visible)
                ShowGroup(name);
            else
                HideGroup(name);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }

    public void SwitchVisible(string name)
    {
        try
        {
            foreach (var button in ObjectGroups)
            {
                if (button.name.ToLower().Contains(name.ToLower()))
                {
                    button.gameObject.SetActive(!button.gameObject.activeSelf);
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }

    public void SwitchVisible(GameObject group)
    {
        try
        {
            bool active = group.activeSelf;
            group.SetActive(!active);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }

    public void TryDisableInteract(string name)
    {
        try
        {
            TrySetEnabled(name, false);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }
    public void TryEnableInteract(string name)
    {
        try
        {
            TrySetEnabled(name, true);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }

    public void TrySetEnabled(string name, bool state)
    {
        try
        {
            foreach (var group in ObjectGroups)
            {
                if (group.name.ToLower().Contains(name.ToLower()))
                {
                    var selectables = group.GetComponentsInChildren<Selectable>();
                    if (selectables.Length != 0)
                    {
                        foreach (var selectable in selectables)
                            selectable.enabled = state;

                        return;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }
}
