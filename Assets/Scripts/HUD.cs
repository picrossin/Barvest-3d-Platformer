using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField] private GameObject m_HUD;

    private RectTransform m_WebIcon;
    private TextMeshProUGUI m_Timer;
    private TextMeshProUGUI m_CoinCount;
    private TextMeshProUGUI m_FinalCoinCount;
    private Image m_Stomach;
    private GameObject m_Options;
    private GameObject m_GameOver;

    void Start()
    {
        m_WebIcon = transform.Find("WebIcon").GetComponent<RectTransform>();
        m_Timer = m_HUD.transform.Find("Timer").GetComponent<TextMeshProUGUI>();
        m_CoinCount = m_HUD.transform.Find("CoinsCount").GetComponent<TextMeshProUGUI>();
        m_FinalCoinCount = m_HUD.transform.Find("Book/GameOver/CoinsCount").GetComponent<TextMeshProUGUI>();
        m_Stomach = m_HUD.transform.Find("Stomach/StomachFill").GetComponent<Image>();
        m_Options = m_HUD.transform.Find("Book/Options").gameObject;
        m_GameOver = m_HUD.transform.Find("Book/GameOver").gameObject;
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

    public void GameOver()
    {
        m_Options.SetActive(false);
        m_GameOver.SetActive(true);
        m_FinalCoinCount.SetText(m_CoinCount.text);
    }

    public void SetCoinCount(int count, int max)
    {
        m_CoinCount.SetText($"{count:D2}/{max:D2}");
    }
}
