using System;
using UnityEngine;
using static Assets.Code.Models.Reward.BaseReward;

public class Item_RewardListSimple_Controler : MonoBehaviour
{
    public RewardStatusIconController RewardStatus;
    private TextFieldsFiller m_textFieldsFiller;

    void Start()
    {
        m_textFieldsFiller = GetComponent<TextFieldsFiller>();

        try
        {            
            RewardStatus.SetStatus((BaseRewardStatus)Enum.Parse(typeof(BaseRewardStatus), m_textFieldsFiller.TextData["Status"]), m_textFieldsFiller);
        }
        catch(Exception ex)
        {
            Debug.LogError(ex);
        }
    }
}
