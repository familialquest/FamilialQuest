using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Code.ViewControllers.InputField
{
    class FQInputField : MonoBehaviour
    {
        public GameObject InputBg;
        public GameObject InputStyle;

        public void OnClick_StartEdit()
        {
            if (Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.IPhonePlayer)
            {
                //ToucScreenKeyboard

                try
                {
                    InputBg.SetActive(true);
                }
                catch
                {

                }


                var textAreas = gameObject.GetComponentsInChildren<TMPro.TMP_Text>();
                foreach (var textArea in textAreas)
                {
                    var defaultColor = textArea.color;
                    defaultColor.a = 0f;

                    textArea.color = defaultColor;
                }

                try
                {                    
                    InputStyle.SetActive(true);
                }
                catch
                {

                }
            }
        }

        public void OnClick_EndEdit()
        {
            if (Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.IPhonePlayer)
            {
                try
                {
                    InputStyle.SetActive(false);
                }
                catch
                {

                }

                var textAreas = gameObject.GetComponentsInChildren<TMPro.TMP_Text>();

                foreach (var textArea in textAreas)
                {
                    var defaultColor = textArea.color;

                    if (textArea.name.Contains("Placeholder"))
                    {                        
                        defaultColor.a = 0.5f;
                    }
                    else
                    {
                        defaultColor.a = 1f;
                    }

                    textArea.color = defaultColor;
                }

                try
                {
                    InputBg.SetActive(false);
                }
                catch
                {

                }
            }
        }
    }
}
