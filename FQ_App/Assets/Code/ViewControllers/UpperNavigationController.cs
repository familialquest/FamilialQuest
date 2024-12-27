using Code.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI.ScrollSnaps;

public class UpperNavigationController : MonoBehaviour
{
    //public GameObject ProgressBarLocker;

    public GameObject[] Pages;

    public GameObject NewPushCounterBG = null;
    public TextMeshProUGUI NewPushCounter = null;

    public DirectionalScrollSnap ScrollSnap;
    public Int32 InitFlag = 0;
    bool isInit = false;

    private FlagController[] m_flags;

    private int currentPage = 0;

    private void Awake()
    {
        m_flags = GetComponentsInChildren<FlagController>();

        try
        {
            //Прокинем PushCounter в модель, чтобы плюсовать его из любой точки
            NewPushCounter.gameObject.SetActive(false);
            NewPushCounterBG.SetActive(false);
            DataModel.Instance.HistoryEvents.NewPushCounterBG = NewPushCounterBG;
            DataModel.Instance.HistoryEvents.NewPushCounter = NewPushCounter;
        }
        catch
        {
            ;
        }
    }

    private void OnGUI()
    {
        try
        {
            if (!isInit)
            {
                DownFlag(InitFlag);
                isInit = true;
            }
        }
        catch
        {

        }
    }

    public void LockFlags()
    {
        //ProgressBarLocker.SetActive(true);
    }

    public void UnlockFlags()
    {
        //ProgressBarLocker.SetActive(false);
    }
       
    public void DownFlag(FlagController flag)
    {
        try
        {
            UpAllFlag();
            flag.Down();
        }
        catch
        {

        }
    }

    public void DownFlag(Int32 flagNumber)
    {
        try
        {
            UpAllFlag();
            m_flags[flagNumber].Down();
        }
        catch
        {

        }
    }

    public void UpAllFlag()
    {
        foreach (var flag in m_flags)
        {
            try
            {
                flag.BGNotActive.SetActive(true);
                flag.IconNotActive.SetActive(true);
                flag.IconActive.SetActive(false);
                flag.Up();
            }
            catch
            {

            }
        }
    }

    public void GoToScreen(Int32 screenNumber)
    {
        try
        {
            for (int i = 0; i < Pages.Length; i++)
            {
                if (i == screenNumber)
                {
                    Pages[i].SetActive(true);
                }
                else
                {
                    Pages[i].SetActive(false);
                }
            }

            DownFlag(screenNumber);

            currentPage = screenNumber;
        }
        catch
        {

        }
    }

    public void GoToScreen_Next()
    {
        try
        {
            int nextPage = currentPage + 1;

            if (nextPage == Pages.Length)
            {
                nextPage = 0;
            }

            GoToScreen(nextPage);
        }
        catch
        {

        }
    }

    public void GoToScreen_Prev()
    {
        try
        {
            int prevPage = currentPage - 1;

            if (prevPage < 0)
            {
                prevPage = Pages.Length - 1;
            }

            GoToScreen(prevPage);
        }
        catch
        {

        }
    }

}
