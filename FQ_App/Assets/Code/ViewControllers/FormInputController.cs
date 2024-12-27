using Assets.Code.Models.REST.CommonTypes.Common;
using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_InputField))]
public class FormInputController : MonoBehaviour
{
    public enum FormInputType : int
    {
        String = 0,
        DateTime,
        TimeSpan,
        Int,
        Username,
        Usernames
    }
    public FormInputType InputType;
    private TMP_InputField m_inputField;
    private void Awake()
    {
        m_inputField = GetComponent<TMP_InputField>();
    }


    public object GetValue()
    {
        switch (InputType)
        {
            case FormInputType.String:
                return GetString();

            case FormInputType.DateTime:
                return GetDatetime();

            case FormInputType.TimeSpan:
                return GetTimespan();

            case FormInputType.Int:
                return GetInt();

            case FormInputType.Username:
                throw new NotImplementedException();

            case FormInputType.Usernames: 
                throw new NotImplementedException();
        }

        return null;
    }

    private string GetString()
    {
        if (m_inputField == null || !m_inputField.isActiveAndEnabled)
            return "";
        else
            return m_inputField.text;
    }

    private DateTime GetDatetime()
    {
        DateTime dateTime;
        if (m_inputField == null || !m_inputField.isActiveAndEnabled || string.IsNullOrWhiteSpace(m_inputField.text))
            dateTime = CommonData.dateTime_FQDB_MinValue;
        else
        {
            DateTime.TryParse(m_inputField.text, out dateTime);
        }
        return dateTime;
    }

    private TimeSpan GetTimespan()
    {
        TimeSpan timeSpan;
        if (m_inputField == null || !m_inputField.isActiveAndEnabled || string.IsNullOrWhiteSpace(m_inputField.text))
            timeSpan = TimeSpan.MinValue;
        else
        {
            TimeSpan.TryParse(m_inputField.text, out timeSpan);
        }
        return timeSpan;
    }

    private int GetInt()
    {
        int intValue;
        if (m_inputField == null || !m_inputField.isActiveAndEnabled || string.IsNullOrWhiteSpace(m_inputField.text))
            intValue = int.MinValue;
        else
        {
            int.TryParse(m_inputField.text, out intValue);
        }
        return intValue;
    }

}