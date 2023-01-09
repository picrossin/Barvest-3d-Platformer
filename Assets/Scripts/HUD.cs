using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField] private GameObject m_HUD;

    private RectTransform m_WebIcon;
    private TextMeshProUGUI m_Timer;
    private Image m_Stomach;

    void Start()
    {
        m_WebIcon = transform.Find("WebIcon").GetComponent<RectTransform>();
        m_Timer = m_HUD.transform.Find("Timer").GetComponent<TextMeshProUGUI>();
        m_Stomach = m_HUD.transform.Find("Stomach/StomachFill").GetComponent<Image>();
    }

    public void ShowHUD(bool active)
    {
        m_HUD.SetActive(active);
    }

    public void SetTimer(int seconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(seconds);
        m_Timer.SetText(time.ToString(@"mm\:ss"));
    }

    public void SetStomachFill(float fill)
    {
        m_Stomach.fillAmount = fill;
    }
}
