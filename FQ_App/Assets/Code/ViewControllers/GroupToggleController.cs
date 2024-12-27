using System;
using UnityEngine;
using UnityEngine.UI;

public class GroupToggleController : MonoBehaviour
{
    public Toggle[] Toggles;

    public void ToggleOnOnly(string name)
    {
        try
        {
            foreach (var t in Toggles)
            {
                if (t.name.ToLower().Contains(name.ToLower()))
                    t.isOn = true;
                else
                    t.isOn = false;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }

    public void ToggleOffOnly(string name)
    {
        try
        {
            foreach (var t in Toggles)
            {
                if (t.name.ToLower().Contains(name.ToLower()))
                    t.isOn = false;
                else
                    t.isOn = true;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }

    public void ToggleSet(string name, bool state)
    {
        try
        {
            foreach (var t in Toggles)
            {
                if (t.name.ToLower().Contains(name.ToLower()))
                    t.isOn = state;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }

    public void ToggleOn(string name)
    {
        try
        {
            ToggleSet(name, true);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }

    public void ToggleOff(string name)
    {
        try
        {
            ToggleSet(name, false);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }
}
