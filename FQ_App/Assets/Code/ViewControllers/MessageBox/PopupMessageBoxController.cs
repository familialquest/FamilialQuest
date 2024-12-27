using TMPro;
using UnityEngine;

public class PopupMessageBoxController : MonoBehaviour
{
    public TextMeshProUGUI Caption;
    public int MaximumCaptionLength = 50;
    public TextMeshProUGUI Text;
    public int MaximumMessageLength = 78;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void SetCaption(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            text = "<текст заголовка не задан>";

        if (text.Length > MaximumCaptionLength)
            Debug.LogWarning("Слишком большой текст в заголовке сообщения!");

        Caption.text = text;
    }

    public void SetText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            text = "<текст сообщения не задан>";

        if (text.Length > MaximumMessageLength)
            Debug.LogWarning("Слишком большой текст в окне сообщения!");

        Text.text = text;
    }
}
