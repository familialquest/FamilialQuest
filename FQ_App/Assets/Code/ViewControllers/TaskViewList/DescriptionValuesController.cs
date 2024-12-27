using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DescriptionValuesController : MonoBehaviour
{
    public GameObject RewardGroup;
    public GameObject PenaltyGroup;
    public GameObject AvailableUntilGroup;
    public GameObject SolutionGroup;

    public void SetVisible(bool reward, bool penalty, bool availableUntil, bool solutiontime)
    {
        try
        {
            RewardGroup.SetActive(reward);
            PenaltyGroup.SetActive(penalty);
            AvailableUntilGroup.SetActive(availableUntil);
            SolutionGroup.SetActive(solutiontime);
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
            //RectTransform rect = group.transform.parent.GetComponent<RectTransform>();
            ////group.SetActive(!active);
            //if (!active)
            //    rect.sizeDelta = new Vector2(rect.sizeDelta.x, rect.sizeDelta.y + 70);
            //else
            //    rect.sizeDelta = new Vector2(rect.sizeDelta.x, rect.sizeDelta.y - 70);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }
}
