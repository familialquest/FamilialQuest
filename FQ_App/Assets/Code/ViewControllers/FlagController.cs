using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagController : MonoBehaviour
{
    public GameObject IconActive;
    public GameObject IconNotActive;
    public GameObject BGActive;
    public GameObject BGNotActive;
    private RectTransform m_rect;
    private Vector3 m_start;
    private bool isUp = true;

    private const float VERTICAL_MOVE = 44;
    private void Start()
    {
        m_rect = GetComponent<RectTransform>();
    }
    public void Up()
    {
        if (isUp)
            return;

        BGNotActive.SetActive(true);
        BGActive.SetActive(false);
        IconNotActive.SetActive(true);
        IconActive.SetActive(false);

        //Для пересчета размеров холста
        Canvas.ForceUpdateCanvases();

        isUp = true;
        m_rect.anchoredPosition = new Vector2(m_rect.anchoredPosition.x, m_rect.anchoredPosition.y + VERTICAL_MOVE);
    }

    public void Down()
    {
        if (!isUp)
            return;

        BGActive.SetActive(true);
        BGNotActive.SetActive(false);
        IconNotActive.SetActive(false);
        IconActive.SetActive(true);

        //Для пересчета размеров холста
        Canvas.ForceUpdateCanvases();

        isUp = false;
        m_rect.anchoredPosition = new Vector2(m_rect.anchoredPosition.x, m_rect.anchoredPosition.y - VERTICAL_MOVE);
    }
}
