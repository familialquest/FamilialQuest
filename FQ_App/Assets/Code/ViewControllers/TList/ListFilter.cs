using Code.Models.REST;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.ViewControllers.TList
{
    public class ListFilter : MonoBehaviour
    {
        public GameObject[] ObjectToggles;

        private Color defaultToggleColor;
        private Color selectedToggleColor;

        private Color selectedBGColor;
        private Color unselectedBGColor;

        private Color selectedLineShadowColor;
        private Color unselectedLineShadowColor;

        //Для тогглов
        public void InitializeColors(int defaultToggleFilterNumber = 0)
        {
            defaultToggleColor = Color.HSVToRGB(0.111f, 0.39f, 0.60f);

            defaultToggleColor.a = 0.33f;

            selectedToggleColor = Color.HSVToRGB(0.1166f, 0.29f, 0.36f); //50-60
            selectedToggleColor.a = 1f;

            unselectedBGColor = Color.HSVToRGB(0.11f, 0.53f, 0.80f);
            //unselectedBGColor = Color.HSVToRGB(0.11f, 0.38f, 0.50f);
            unselectedBGColor.a = 0.15f; //мб вернуть 0.2
            selectedBGColor = Color.HSVToRGB(0f, 0f, 0f);
            selectedBGColor.a = 0f;

            unselectedLineShadowColor = Color.HSVToRGB(0.108f, 0.38f, 0.50f);
            unselectedLineShadowColor.a = 0.20f;
            selectedLineShadowColor = Color.HSVToRGB(0f, 0f, 0f);
            selectedLineShadowColor.a = 0f;

            SetSelectedColor(defaultToggleFilterNumber);            
        }

        public void SetSelectedColor(int current)
        {
            int i = 0;

            foreach (var toggle in ObjectToggles)
            {
                var toggleName = toggle.GetComponentsInChildren<TMP_Text>()[0];
                toggleName.color = defaultToggleColor;

                try
                {
                    var toggleIcon = toggle.GetComponentsInChildren<Image>().Where(x => x.name.Contains("ToggleIcon")).FirstOrDefault();

                    if (toggleIcon != null)
                    {
                        toggleIcon.color = defaultToggleColor;
                    }
                    
                    try
                    {
                        var img_BGUnselectedTab = toggle.GetComponentsInChildren<Image>().Where(x => x.name.Contains("BGUnselectedTab")).FirstOrDefault();
                        var img_LineShadow_Horizontal_Bot = toggle.GetComponentsInChildren<Image>().Where(x => x.name.Contains("LineShadow_Horizontal_B")).FirstOrDefault();
                        var img_LineShadow_Horizontal_Top = toggle.GetComponentsInChildren<Image>().Where(x => x.name.Contains("LineShadow_Horizontal_T")).FirstOrDefault();
                        var img_LineShadow_Vertical_Left = toggle.GetComponentsInChildren<Image>().Where(x => x.name.Contains("LineShadow_Vertical_L")).FirstOrDefault();
                        var img_LineShadow_Vertical_Right = toggle.GetComponentsInChildren<Image>().Where(x => x.name.Contains("LineShadow_Vertical_R")).FirstOrDefault();
                        var cimg_LineVertical_Left = toggle.GetComponentsInChildren<Image>().Where(x => x.name.Contains("LineVerticalL")).FirstOrDefault();
                        var cimg_LineVertical_Right = toggle.GetComponentsInChildren<Image>().Where(x => x.name.Contains("LineVerticalR")).FirstOrDefault();
                        var cimg_BGSelectedTab = toggle.GetComponentsInChildren<Image>().Where(x => x.name.Contains("BGSelectedTab")).FirstOrDefault();

                        img_BGUnselectedTab.color = unselectedBGColor;

                        img_LineShadow_Horizontal_Bot.color = unselectedLineShadowColor;

                        if (img_LineShadow_Horizontal_Top != null)
                        {
                            img_LineShadow_Horizontal_Top.color = unselectedLineShadowColor;
                        }

                        if (img_LineShadow_Vertical_Left != null)
                        {
                            img_LineShadow_Vertical_Left.color = selectedLineShadowColor;
                        }

                        if (img_LineShadow_Vertical_Right != null)
                        {
                            img_LineShadow_Vertical_Right.color = selectedLineShadowColor;
                        }

                        if (cimg_LineVertical_Left != null)
                        {
                            var currentColor = cimg_LineVertical_Left.color;
                            currentColor.a = 0.35f;

                            cimg_LineVertical_Left.color = currentColor;
                        }

                        if (cimg_LineVertical_Right != null)
                        {
                            var currentColor = cimg_LineVertical_Right.color;
                            currentColor.a = 0.35f;

                            cimg_LineVertical_Right.color = currentColor;
                        }

                        if (cimg_BGSelectedTab != null)
                        {
                            var currentColor = cimg_BGSelectedTab.color;
                            currentColor.a = 0f;

                            cimg_BGSelectedTab.color = currentColor;
                        }
                    }
                    catch
                    {

                    }
                }
                catch
                {
                    ;
                }

                i++;
            }

            var selectedToggleName = ObjectToggles[current].GetComponentsInChildren<TMP_Text>()[0];
            selectedToggleName.color = selectedToggleColor;

            try
            {
                var toggleIcon = ObjectToggles[current].GetComponentsInChildren<Image>().Where(x => x.name.Contains("ToggleIcon")).FirstOrDefault();

                if (toggleIcon != null)
                {
                    toggleIcon.color = selectedToggleColor;
                }

                try
                {
                    var img_BGUnselectedTab = ObjectToggles[current].GetComponentsInChildren<Image>().Where(x => x.name.Contains("BGUnselectedTab")).FirstOrDefault();
                    var img_LineShadow_Horizontal_Bot = ObjectToggles[current].GetComponentsInChildren<Image>().Where(x => x.name.Contains("LineShadow_Horizontal_B")).FirstOrDefault();
                    var img_LineShadow_Horizontal_Top = ObjectToggles[current].GetComponentsInChildren<Image>().Where(x => x.name.Contains("LineShadow_Horizontal_T")).FirstOrDefault();
                    var img_LineShadow_Vertical_Left = ObjectToggles[current].GetComponentsInChildren<Image>().Where(x => x.name.Contains("LineShadow_Vertical_L")).FirstOrDefault();
                    var img_LineShadow_Vertical_Right = ObjectToggles[current].GetComponentsInChildren<Image>().Where(x => x.name.Contains("LineShadow_Vertical_R")).FirstOrDefault();
                    var cimg_LineVertical_Left = ObjectToggles[current].GetComponentsInChildren<Image>().Where(x => x.name.Contains("LineVerticalL")).FirstOrDefault();
                    var cimg_LineVertical_Right = ObjectToggles[current].GetComponentsInChildren<Image>().Where(x => x.name.Contains("LineVerticalR")).FirstOrDefault();
                    var cimg_BGSelectedTab = ObjectToggles[current].GetComponentsInChildren<Image>().Where(x => x.name.Contains("BGSelectedTab")).FirstOrDefault();


                    img_BGUnselectedTab.color = selectedBGColor;

                    img_LineShadow_Horizontal_Bot.color = selectedLineShadowColor;

                    if (img_LineShadow_Horizontal_Top != null)
                    {
                        img_LineShadow_Horizontal_Top.color = selectedLineShadowColor;
                    }

                    if (img_LineShadow_Vertical_Left != null)
                    {
                        img_LineShadow_Vertical_Left.color = selectedLineShadowColor;
                    }

                    if (img_LineShadow_Vertical_Right != null)
                    {
                        img_LineShadow_Vertical_Right.color = selectedLineShadowColor;
                    }

                    if (cimg_LineVertical_Left != null)
                    {
                        var currentColor = cimg_LineVertical_Left.color;
                        currentColor.a = 0f;

                        cimg_LineVertical_Left.color = currentColor;
                    }

                    if (cimg_LineVertical_Right != null)
                    {
                        var currentColor = cimg_LineVertical_Right.color;
                        currentColor.a = 0f;

                        cimg_LineVertical_Right.color = currentColor;
                    }

                    if (cimg_BGSelectedTab != null)
                    {
                        var currentColor = cimg_BGSelectedTab.color;
                        currentColor.a = 0.08f;

                        cimg_BGSelectedTab.color = currentColor;
                    }

                    //Теперь необходимо включить тень у соседних табов  
                    var leftValue = current - 1;
                    var rightValue = current + 1;

                    //TODO: На железе RoleApplicator в этом случае отрабатывает после этого кода, и условие ниже отрабатывает некорректно.
                    if (CredentialHandler.Instance.CurrentUser.Role == Models.RoleModel.RoleTypes.User)
                    {
                        var subscrToggle = ObjectToggles.Where(x => x.name.Contains("Subscription")).FirstOrDefault();

                        if (subscrToggle != null)
                        {
                            subscrToggle.SetActive(false);
                        }
                    }
                    else
                    {
                        var subscrToggle = ObjectToggles.Where(x => x.name.Contains("Subscription")).FirstOrDefault();

                        if (subscrToggle != null)
                        {
                            subscrToggle.SetActive(true);
                        }
                    }

                    //Для табов настроек, где Премиум-доступ скрыт для юзеров
                    if (CredentialHandler.Instance.CurrentUser.Role == Models.RoleModel.RoleTypes.User &&
                    (ObjectToggles.Length == 3 && ObjectToggles.Where(x => x.active).Count() != 3))
                    {
                        if (current == 2) 
                        {
                            leftValue = 0;
                        }

                        if (current == 0)
                        {
                            rightValue = 2;
                        }
                    }



                    if (leftValue >= 0)
                    {
                        img_LineShadow_Vertical_Left = ObjectToggles[leftValue].GetComponentsInChildren<Image>().Where(x => x.name.Contains("LineShadow_Vertical_L")).FirstOrDefault();
                        img_LineShadow_Vertical_Right = ObjectToggles[leftValue].GetComponentsInChildren<Image>().Where(x => x.name.Contains("LineShadow_Vertical_R")).FirstOrDefault();
                        cimg_LineVertical_Right = ObjectToggles[leftValue].GetComponentsInChildren<Image>().Where(x => x.name.Contains("LineVerticalR")).FirstOrDefault();

                        if (img_LineShadow_Vertical_Left != null)
                        {
                            img_LineShadow_Vertical_Left.color = selectedLineShadowColor;
                        }

                        if (img_LineShadow_Vertical_Right != null)
                        {
                            img_LineShadow_Vertical_Right.color = unselectedLineShadowColor;
                        }

                        if (cimg_LineVertical_Right != null)
                        {
                            var currentColor = cimg_LineVertical_Right.color;
                            currentColor.a = 0f;

                            cimg_LineVertical_Right.color = currentColor;
                        }
                    }

                    if (rightValue <= (ObjectToggles.Length - 1))
                    {
                        img_LineShadow_Vertical_Left = ObjectToggles[rightValue].GetComponentsInChildren<Image>().Where(x => x.name.Contains("LineShadow_Vertical_L")).FirstOrDefault();
                        img_LineShadow_Vertical_Right = ObjectToggles[rightValue].GetComponentsInChildren<Image>().Where(x => x.name.Contains("LineShadow_Vertical_R")).FirstOrDefault();
                        cimg_LineVertical_Left = ObjectToggles[rightValue].GetComponentsInChildren<Image>().Where(x => x.name.Contains("LineVerticalL")).FirstOrDefault();

                        if (img_LineShadow_Vertical_Left != null)
                        {
                            img_LineShadow_Vertical_Left.color = unselectedLineShadowColor;
                        }

                        if (img_LineShadow_Vertical_Right != null)
                        {
                            img_LineShadow_Vertical_Right.color = selectedLineShadowColor;
                        }

                        if (cimg_LineVertical_Left != null)
                        {
                            var currentColor = cimg_LineVertical_Left.color;
                            currentColor.a = 0f;

                            cimg_LineVertical_Left.color = currentColor;
                        }
                    }                                        
                }
                catch
                {

                }
            }
            catch
            {
                ;
            }
        }
        //--

        /// <summary>
        /// Событие необходимости выполнить обновление списка
        /// </summary>
        public event EventHandler OnFilterChanged;
        protected virtual void FilterChanged(EventArgs e)
        {
            EventHandler handler = OnFilterChanged;
            handler?.Invoke(this, e);
        }

        public virtual bool FilterItem(object item)
        {
            throw new NotImplementedException();
        }
    }
}
