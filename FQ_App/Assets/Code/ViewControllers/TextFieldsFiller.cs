using Code.ViewControllers.TList;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ITextPresenter))]
public class TextFieldsFiller : MonoBehaviour
{
    public List<TMP_InputField> InputFields;
    public List<TMP_Text> TextFields;
    public Dictionary<string, object> Data;
    public Dictionary<string, string> TextData;

    private bool truncateChechked = false;

    //
    private ITextPresenter m_textPresenter;

    private void Awake()
    {
        try
        {
            m_textPresenter = GetComponent<ITextPresenter>();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }

    public void Update()
    {
        //Требуется выполнить единожды при прогрузе визуала.
        //Тут - потому что при вызове SetData isTextTruncated некорректный, т.к. на скрытых вкладках
        //ширина не обновлена под экран и имеет стандартный размер префаба.
        //Здесь же размер полей пересчитан.
        if (!truncateChechked)
        {
            truncateChechked = true;

            foreach (var field in TextFields)
            {
                //В Text_ActualUserLabel нужно показать либо одного Executor-а (aaa...),
                //либо список целевых пользователей в формате aaa bbb cc... [n]
                if (field.name == "Text_ActualUserLabel" || field.name == "Text_DestinationUserLabel")
                {
                    TruncateAvailableUsers(field, TextFields);
                }
                                
                if (field.name == "Text_Name" ||
                    field.name == "Text_Title" ||
                    field.name == "Text_ExecutorLabel" ||
                    field.name == "Text_StatusLabel" ||
                    field.name == "Text_AvailableFor")
                {
                    //Для пересчета размеров холста
                    Canvas.ForceUpdateCanvases();

                    //Для актуализации isTextTruncated
                    field.ForceMeshUpdate(true);

                    if (field.isTextTruncated)
                    {
                        //Дальше будем работать только с той частью строки, что поместилась в поле
                        field.text = field.text.Substring(0, field.textInfo.characterCount);

                        //Сохраним текущее состояние строки
                        string sourceText = field.text;

                        //Просто затрём нужное кол-во символов и добавим многоточие
                        for (int i = 0; i < sourceText.Length; i++)
                        {
                            field.text = string.Format("{0}...", sourceText.Substring(0, sourceText.Length - i));
                            field.ForceMeshUpdate(true);

                            if (!field.isTextTruncated)
                            {
                                break;
                            }
                        }
                    }
                }
            }            
        }
    }

    public static void TruncateAvailableUsers(TMP_Text field, List<TMP_Text> ListTextFields = null, int usersCount = 0)
    {
        //Для пересчета размеров холста
        Canvas.ForceUpdateCanvases();

        //Для актуализации isTextTruncated
        field.ForceMeshUpdate(true);

        //Если текст сокращен - отформатируем как следует
        if (field.isTextTruncated)
        {
            //Дальше будем работать только с той частью строки, что поместилась в поле
            field.text = field.text.Substring(0, field.textInfo.characterCount);
            
            TMP_Text destinationUsersCountStr = null;
            int destinationUsersCount = 0;

            if (usersCount > 0)
            {
                //Если со страницы создания\редактирования
                destinationUsersCount = usersCount;
            }
            else 
            {
                //Из TaskPresenter (где имелся доступ ко всем полям задачи) прокинуто количество пользователей в ActualUserLabel,
                //чтобы здесь не парсить повторно
                destinationUsersCountStr = ListTextFields.Where(x => x.name == "Text_DestinationUsersCount").FirstOrDefault();

                if (destinationUsersCountStr != null && destinationUsersCountStr.text != string.Empty)
                {
                    Int32.TryParse(destinationUsersCountStr.text, out destinationUsersCount);
                }
            }
            
            //Если несколько
            if (destinationUsersCount > 1)
            {
                //Сохраним текущее состояние строки
                string sourceText = field.text;

                //Обрезаем, пока строка  + ... не уложится в допустимую ширину
                for (int i = 0; i < sourceText.Length; i++)
                {
                    //Прикинем, что получим после удаления
                    //Если на конце пробел - принудительно отсечем
                    string valueAfterEdit = sourceText.Substring(0, sourceText.Length - i);
                    if (valueAfterEdit.EndsWith(" "))
                    {
                        continue;
                    }

                    //Если имя крайнего правого пользователя поместилось полностью, многоточие не требуется
                    if (valueAfterEdit.EndsWith(","))
                    {
                        valueAfterEdit = valueAfterEdit.TrimEnd(',');
                        field.text = valueAfterEdit;
                    }
                    else
                    {
                        //Дописывание ... и проверка, вместилась ли строка в допустимую ширину
                        field.text = string.Format("{0}...", valueAfterEdit);
                    }

                    field.ForceMeshUpdate(true);

                    if (!field.isTextTruncated)
                    {
                        //Вместилось
                        break;
                    }
                }

                //Теперь проверим, уместились ли все пользаки в сокращение
                //Если нет, добавим [кол-во непопавших]

                //Разделяющих пользаков запятых всегда на 1 меньше, чем самих пользователей
                int destinationCommaCount = destinationUsersCount - 1;

                //Сколько запятых в сокращенной ранее строке
                int commaCountFromTruncatedStr = field.text.Count(x => x == ',');

                //Если совпадет - значит, было скоращено имя только последнего пользователя и все попали.
                //В противном случае требуется переформатировать и указать количество не попавших
                if (commaCountFromTruncatedStr != destinationCommaCount)
                {
                    //Поместилось ли полностью имя крайнего правого пользователя
                    bool endWithFullName = !field.text.EndsWith("...");

                    //Удаление ранее добавленное многоточия (если есть) - будет добавлен иной формат концовки
                    field.text = field.text.TrimEnd('.');

                    //Сохраним текущее состояние строки
                    sourceText = field.text;

                    for (int i = 0; i < sourceText.Length; i++)
                    {
                        //Прикинем, что получим после удаления
                        //Если на конце пробел - принудительно отсечем
                        string valueAfterEdit = sourceText.Substring(0, sourceText.Length - i);
                        if (valueAfterEdit.EndsWith(" "))
                        {
                            continue;
                        }

                        //Если имя крайнего правого пользователя поместилось полностью, многоточие не требуется
                        if (valueAfterEdit.EndsWith(",") || endWithFullName)
                        {
                            valueAfterEdit = valueAfterEdit.TrimEnd(',');
                            field.text = valueAfterEdit;

                            //Необходимо проверить количество пользаков
                            commaCountFromTruncatedStr = valueAfterEdit.Count(x => x == ',');
                            int remainderUsersCount = destinationCommaCount - commaCountFromTruncatedStr;

                            //Дописывание и проверка, вместилась ли строка в допустимую ширину
                            field.text = string.Format("{0}, [{1}]", valueAfterEdit, remainderUsersCount);
                        }
                        else
                        {
                            //Необходимо проверить количество пользаков
                            commaCountFromTruncatedStr = valueAfterEdit.Count(x => x == ',');
                            int remainderUsersCount = destinationCommaCount - commaCountFromTruncatedStr;

                            //Дописывание ... и проверка, вместилась ли строка в допустимую ширину
                            field.text = string.Format("{0}..., [{1}]", valueAfterEdit, remainderUsersCount);
                        }

                        field.ForceMeshUpdate(true);

                        if (!field.isTextTruncated)
                        {
                            break;
                        }
                        else
                        {
                            //Если не удалось с первого прохода- значит имя пользователя не будет целым и требуется ...
                            endWithFullName = false;
                        }
                    }
                }

            }
            else
            {
                //Сохраним текущее состояние строки
                string sourceText = field.text;

                //Если один пользователь, и его имя не помещается - просто затрём нужное кол-во символов и добавим многоточие
                for (int i = 0; i < sourceText.Length; i++)
                {
                    field.text = string.Format("{0}...", sourceText.Substring(0, sourceText.Length - i));
                    field.ForceMeshUpdate(true);

                    if (!field.isTextTruncated)
                    {
                        break;
                    }
                }
            }
        }

        //field.text = field.text.Replace(",", "");
    }

    //public void SetData(Dictionary<string, object> data)
    //{
    //    if (data == null)
    //        return;

    //    Data = data;
    //    // TODO: возможно передалать с перебора на получение данных из словаря по имени поля
    //    object value = "";
    //    foreach(var field in TextFields)
    //    {
    //        if (Data.TryGetValue(ClearName(field.name), out value))
    //            field.text = value.ToString();
    //    }
    //    //foreach (string key in Data.Keys)
    //    //{
    //    //    foreach(var field in TextFields)
    //    //    {
    //    //        if (field.name.Equals(key, System.StringComparison.OrdinalIgnoreCase) ||
    //    //            field.name.Equals("Text_" + key, System.StringComparison.OrdinalIgnoreCase) ||
    //    //            field.name.Equals("CText_" + key, System.StringComparison.OrdinalIgnoreCase))
    //    //        {
    //    //            field.text = Data[key].ToString();
    //    //        }
    //    //    }
    //    //}
    //}
    public void SetData(Dictionary<string, object> data)
    {
        try
        {
            if (data == null)
                return;            

            Data = data;

            TextData = new Dictionary<string, string>();

            if (m_textPresenter != null)
                TextData = m_textPresenter.Present(Data);           

            string value = "";
            foreach (var field in TextFields)
            {
                if (TextData.TryGetValue(ClearName(field.name), out value))
                    field.text = value;
            }
            foreach (var field in InputFields)
            {
                if (TextData.TryGetValue(ClearName(field.name), out value))
                    field.text = value;
            }

            //foreach (string key in Data.Keys)
            //{
            //    foreach (var field in TextFields)
            //    {
            //        if (field.name.Equals(key, System.StringComparison.OrdinalIgnoreCase) ||
            //            field.name.Equals("Text_" + key, System.StringComparison.OrdinalIgnoreCase) ||
            //            field.name.Equals("CText_" + key, System.StringComparison.OrdinalIgnoreCase))
            //        {
            //            field.text = displayedText[key];
            //        }
            //    }
            //}            
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }

    private static string ClearName(string fullFieldName)
    {
        string clearName = "";
        int first = fullFieldName.IndexOf('_');
        clearName = fullFieldName.Substring(first + 1);
        return clearName;
    }
}
