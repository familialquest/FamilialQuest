using Code.Models.REST.CommonType.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Assets.Code.Models.Reward.BaseReward;

public class RewardStatusIconController : MonoBehaviour
{
    public GameObject NewSign;

    public GameObject CreatedStatus_CanPurchase_Yes;
    public GameObject CreatedStatus_CanPurchase_No;

    public GameObject CreatedStatus_Purchased;
    public GameObject CreatedStatus_Handed;


    // Start is called before the first frame update
    void Start()
    {
        //SwitchOffAllStatus();
    }

    private void SwitchOffAllStatus()
    {
        try
        {
            NewSign.SetActive(false);
            CreatedStatus_CanPurchase_Yes.SetActive(false);
            CreatedStatus_CanPurchase_No.SetActive(false);
            CreatedStatus_Purchased.SetActive(false);
            CreatedStatus_Handed.SetActive(false);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    private void SwitchOnStatus(GameObject statusObject)
    {
        try
        {
            SwitchOffAllStatus();
            statusObject.SetActive(true);
            return;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }

        //if (statusObject == InProgressStatus)
        //{
        //    InProgressStatus.SetActive(true);
        //    CompletedStatus.SetActive(false);
        //    FailedStatus.SetActive(false);
        //}
        //else if (statusObject == CompletedStatus)
        //{
        //    InProgressStatus.SetActive(false);
        //    CompletedStatus.SetActive(true);
        //    FailedStatus.SetActive(false);
        //}
        //else if (statusObject == FailedStatus)
        //{
        //    InProgressStatus.SetActive(false);
        //    CompletedStatus.SetActive(false);
        //    FailedStatus.SetActive(true);
        //}
    }

    public void SetStatus(BaseRewardStatus currentStatus, TextFieldsFiller m_textFieldsFiller = null)
    {
        try
        {
            switch (currentStatus)
            {
                case BaseRewardStatus.Registered:
                    if (CanPurchaseFromString(m_textFieldsFiller.TextData["CanPurchase"].ToString()))
                    {
                        CreatedStatus_CanPurchase_Yes.SetActive(true);
                    }
                    else
                    {
                        CreatedStatus_CanPurchase_No.SetActive(true);
                    }
                    break;
                case BaseRewardStatus.Purchase:
                    CreatedStatus_Purchased.SetActive(true);
                    break;
                case BaseRewardStatus.Handed:
                    CreatedStatus_Handed.SetActive(true);
                    break;
                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }
}
