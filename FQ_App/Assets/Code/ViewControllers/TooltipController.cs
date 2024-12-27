using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject TooltipPrefab;
    private GameObject m_tooltip;
    public int MaximumSymbols = 122;

    [HideInInspector]
    public bool IsActive = false;

    public void Show(string text)
    {
        if (text.Length > MaximumSymbols)
            Debug.LogWarning("Слишком длинное сообщения для тултипа");

        if (m_tooltip != null)
            Hide();

        m_tooltip = Instantiate(TooltipPrefab);
        m_tooltip.transform.SetParent(this.transform, false);
        var textTransform = m_tooltip.transform.Find("Text_Tooltip");
        textTransform.GetComponent<TextMeshProUGUI>().SetText(text);
        IsActive = true;
    }

    public void Hide()
    {
        IsActive = false;
        Destroy(m_tooltip);
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        Hide();
    }
}
