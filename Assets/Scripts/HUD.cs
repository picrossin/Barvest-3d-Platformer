using System;
using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    [SerializeField] private GameObject m_HUD;

    private RectTransform m_WebIcon;
    private TextMeshProUGUI m_Timer;

    void Start()
    {
        m_WebIcon = transform.Find("WebIcon").GetComponent<RectTransform>();
        m_Timer = m_HUD.transform.Find("Timer").GetComponent<TextMeshProUGUI>();
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
}
