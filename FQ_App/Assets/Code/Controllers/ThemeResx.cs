using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

public class ThemeResx : MonoBehaviour
{
    string m_themePath = "";//$"{Application.persistentDataPath}\\Themes\\";
    // Start is called before the first frame update
    void Awake()
    {
        m_themePath = $"{Application.dataPath}\\Resources\\Themes";

        ThemeConfiguration themeConfiguration = new ThemeConfiguration();
        ConfigurationValue value = new ConfigurationValue() { type = ConfigurationValue.ValueType.FilePath, value = "viewport_bg.png" };
        ComponentConfiguration component = new ComponentConfiguration();
        component.ConfigurationValues.Add("Sprite", value);
        value = new ConfigurationValue() { type = ConfigurationValue.ValueType.Text, value = Color.white.ToString() };
        component.ConfigurationValues.Add("Color", value);
        ElementConfigration element = new ElementConfigration();
        element.Components.Add("Image", component);
        themeConfiguration.Elements.Add("Viewport", element);

        value = new ConfigurationValue() { type = ConfigurationValue.ValueType.Text, value = "Получить задачи" };
        component = new ComponentConfiguration();
        component.ConfigurationValues.Add("Text", value);
        element = new ElementConfigration();
        element.Components.Add("TextMeshProUGUI", component);
        themeConfiguration.Elements.Add("GetTaskButton", element);

        string jsonConfig = Newtonsoft.Json.JsonConvert.SerializeObject(themeConfiguration);

        SetTheme("DefaultTheme");
    }


    public void SetTheme(string themeName)
    {
        // load new theme data...
        var newTheme = LoadTheme(themeName);
        if (newTheme != null)
            CurrentThemeConfiguration = newTheme;
    }    

    /// <summary>
    /// Get image and color from theme resource
    /// </summary>
    /// <param name="elementName"></param>
    /// <param name="currentImage"></param>
    public void GetImage(string elementName, Image currentImage)
    {
        ConfigurationValue value;
        var configurationValues = CurrentThemeConfiguration.Elements[elementName].Components["Image"].ConfigurationValues;
        if (configurationValues.TryGetValue("Sprite", out value))
        {
            // image
            Texture2D texture = new Texture2D(1, 1);
            byte[] fileBytes = value.GetFileBytes(CurrentThemeConfiguration.Location);
            texture.LoadImage(fileBytes);
            Rect rect = new Rect(currentImage.sprite.rect.x, currentImage.sprite.rect.y, texture.width, texture.height);
            currentImage.sprite = Sprite.Create(texture, rect, currentImage.sprite.pivot, currentImage.sprite.pixelsPerUnit);
        }
        if (configurationValues.TryGetValue("Color", out value))
        {
            // color
            currentImage.color = ParseColor(value.GetText());
        }
    }

    private Color ParseColor(string colorString)
    {
        try
        {
            var afterFirstBracePos = colorString.IndexOf("(") + 1;
            var beforeLastBracePos = colorString.IndexOf(")");
            string inBrace = colorString.Substring(afterFirstBracePos, beforeLastBracePos - afterFirstBracePos);
            string[] rgba = inBrace.Split(new char[] { ',' });
            float r = float.Parse(rgba[0]);
            float g = float.Parse(rgba[1]);
            float b = float.Parse(rgba[2]);
            float a = float.Parse(rgba[3]);

            return new Color(r, g, b, a);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex);
            return new Color(0, 0, 0, 0);
        }
    }


    public void GetText(string elementName, TextMeshProUGUI textMeshProUGUI)
    {
        ConfigurationValue value;
        var configurationValues = CurrentThemeConfiguration.Elements[elementName].Components["TextMeshProUGUI"].ConfigurationValues;
        if (configurationValues.TryGetValue("Text", out value))
        {
            // text
            textMeshProUGUI.text = value.GetText();
        }
        if (configurationValues.TryGetValue("Color", out value))
        {
            // text
            textMeshProUGUI.color = ParseColor(value.GetText());
        }
    }

    ThemeConfiguration CurrentThemeConfiguration;

    ThemeConfiguration LoadTheme(string themeName)
    {
        string themeConfigurationFolderPath = $"{m_themePath}\\{themeName}";
        string themeConfigurationFilePath = $"{themeConfigurationFolderPath}\\{themeName}.json";
        if (false == File.Exists(themeConfigurationFilePath))
        {
            Debug.LogError($"File {themeConfigurationFilePath} not exist!");
            return null; // TODO: показать ошибку
        }

        string jsonConfiguration = File.ReadAllText(themeConfigurationFilePath);
        ThemeConfiguration theme = JsonConvert.DeserializeObject<ThemeConfiguration>(jsonConfiguration);
        if (theme == null)
        {
            Debug.LogError($"Can't deserialize {themeConfigurationFilePath} to theme configuration!");
            return null; // TODO: показать ошибку
        }
        theme.Location = themeConfigurationFolderPath;

        if (theme.Validate())
        {
            Debug.LogError("Theme is incorrect!");
            return null;
        }

        return theme;
    }

    public class ThemeConfiguration
    {
        public Dictionary<string, ElementConfigration> Elements; // пара "имя элемента" - "конфигурация элемента"

        public string Location = "";

        public bool Validate()
        {
            bool result = false;
            foreach (var elementConfiguration in Elements)
            {
                result &= elementConfiguration.Value.Validate();
            }

            return result;
        }
        public ThemeConfiguration()
        {
            Elements = new Dictionary<string, ElementConfigration>();
        }
    }
    public class ElementConfigration
    {
        public Dictionary<string, ComponentConfiguration> Components; // пара "имя компонента" - "конфигурация компонента"
        public bool Validate()
        {
            bool result = false;
            foreach (var elementConfiguration in Components)
            {
                result &= elementConfiguration.Value.Validate();
            }

            return result;
        }

        public ElementConfigration()
        {
            Components = new Dictionary<string, ComponentConfiguration>();
        }

    }

    public class ComponentConfiguration
    {
        public Dictionary<string, ConfigurationValue> ConfigurationValues; // пара "имя свойства компонента" - "значение свойства"

        public bool Validate()
        {
            bool result = false;
            foreach (var configuration in ConfigurationValues)
            {
                result &= configuration.Value.Validate();
            }

            return result;
        }

        public ComponentConfiguration()
        {
            ConfigurationValues = new Dictionary<string, ConfigurationValue>();
        }
    }

    public class ConfigurationValue
    {
        public enum ValueType
        {
            Text,
            FilePath,
            JSON,
            Unknown
        }

        public ValueType type;
        public object value;

        public ConfigurationValue()
        {
            type = ValueType.Unknown;
            value = "";
        }

        public string GetText()
        {
            if (type == ValueType.Text)
                return value.ToString();

            throw new System.TypeLoadException("Value type is not text.");
        }
        public FileStream GetFile(string locationPrePath)
        {
            if (type == ValueType.FilePath)
            {
                string fullpath = $"{locationPrePath}\\{value.ToString()}";
                if (File.Exists(fullpath))
                    return File.OpenRead(fullpath);
            }
            throw new System.TypeLoadException("Value type is not file path.");
        }
        public byte[] GetFileBytes(string locationPrePath)
        {
            var file = GetFile(locationPrePath);
            byte[] fileBytes = new byte[file.Length];
            file.Read(fileBytes, 0, (int)file.Length);
            return fileBytes;
        }
        public T GetObjectFromJSON<T>()
        {
            if (type == ValueType.JSON)
            {
                return JsonConvert.DeserializeObject<T>(value.ToString());
            }
            throw new System.TypeLoadException("Value type is not JSON.");
        }

        public bool Validate()
        {
            bool result = false;

            switch (type)
            {
                case ValueType.FilePath:
                    if (File.Exists(value.ToString()))
                        result = true;
                    else
                        result = false;

                    break;

                case ValueType.JSON:

                    // TODO: validate JSON scheme
                    result = false;

                    break;

                case ValueType.Text:
                    result = true;

                    break;

                default:
                    Debug.LogError("Unknown type");
                    result = false;

                    break;
            }

            return result;
        }
    }
}
