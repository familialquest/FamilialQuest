using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.ViewControllers
{
    class ParamsTabController : MonoBehaviour
    {
        public Image SoundIcon;
        public Image PushIcon;

        private bool soundStatus;
        private bool pushStatus;

        private Color defaultIconColor;
        private Color selectedIconColor;

        private void Awake()
        {
            InitializeColors();

            soundStatus = true;
            pushStatus = false;
        }

        public void OnButton_SoundParamClick()
        {
            if (soundStatus)
            {
                soundStatus = false;
                SoundIcon.color = defaultIconColor;
            }
            else
            {
                soundStatus = true;
                SoundIcon.color = selectedIconColor;
            }
        }

        public void OnButton_PushParamClick()
        {
            if (pushStatus)
            {
                pushStatus = false;
                PushIcon.color = defaultIconColor;
            }
            else
            {
                pushStatus = true;
                PushIcon.color = selectedIconColor;
            }
        }

        public void InitializeColors()
        {
            defaultIconColor = Color.HSVToRGB(0f, 0f, 0.33f);
            defaultIconColor.a = 0.33f;

            selectedIconColor = Color.HSVToRGB(0f, 0f, 0.33f);
            selectedIconColor.a = 1f;              
        }
    }
}
