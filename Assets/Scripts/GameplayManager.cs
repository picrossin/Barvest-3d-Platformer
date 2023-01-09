using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance { get; set; }

    [SerializeField] private Animation m_CanvasAnimation;
    [SerializeField] private HUD m_Hud;
    [SerializeField] private MusicManager m_MusicManager;
    [SerializeField] private GameObject m_ButtonHoverSFX;
    [SerializeField] private GameObject m_ButtonClickSFX;
    [SerializeField] private GameObject m_BookSFX;
    [SerializeField] private GameObject m_PillowCam;
    [SerializeField] private GameObject m_SecretPillow;
    
    private bool m_Started;
    public bool Started => m_Started;

    private bool m_BookOpen;
    public bool BookOpen => m_BookOpen;

    private int m_TotalEnemies;
    private int m_EnemiesCollected;

    private int m_TotalCoins;
    private int m_CoinsCollected;

    private bool m_CanCloseBook;
    private bool m_GameCompleted;
    
    private RespawnManager m_RespawnManager;
    public RespawnManager Respawn => m_RespawnManager;

    private Stopwatch m_Stopwatch;
    public Stopwatch Stopwatch => m_Stopwatch;

    private float m_Volume = 0.7f;
    public float Volume => m_Volume;

    private void Awake()
    {
        Instance = this;
        
        m_RespawnManager = GetComponent<RespawnManager>();
        m_Stopwatch = GetComponent<Stopwatch>();

        m_TotalEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;
        m_TotalCoins = GameObject.FindGameObjectsWithTag("Coin").Length;

        m_Hud.ShowHUD(false);
    }

    private void Update()
    {
        m_Hud.SetTimer(m_Stopwatch.GetSeconds());
    }

    public void CollectCoin()
    {
        m_CoinsCollected++;
        m_Hud.SetCoinCount(m_CoinsCollected, m_TotalCoins);
        Debug.Log($"{m_CoinsCollected} / {m_TotalCoins}");
    }

    public void CollectEnemy()
    {
        m_EnemiesCollected++;
        
        m_Hud.SetStomachFill((float)m_EnemiesCollected / m_TotalEnemies);

        if (m_EnemiesCollected == m_TotalEnemies - 1) // All but the boss
        {
            StartCoroutine(PillowCutscene());
        }
        
        if (m_EnemiesCollected >= m_TotalEnemies)
        {
            m_GameCompleted = true;
            m_Hud.GameOver();
            OpenBook();
            Debug.Log("YOU WIN!!!!!!");
        }
    }

    public void StartGame()
    {
        m_Stopwatch.Begin();
        m_Started = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        m_CanvasAnimation.Play("StartGame");
        m_Hud.ShowHUD(true);
        m_Hud.SetCoinCount(0, m_TotalCoins);
        m_MusicManager.StartLevelMusic();
    }

    public void OpenBook()
    {
        m_Stopwatch.Pause();
        m_CanvasAnimation.Play("BookIn");
        m_BookOpen = true;
        Instantiate(m_BookSFX);
        StartCoroutine(CloseBookWait());
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }
    
    public void CloseBook()
    {
        if (!m_CanCloseBook || m_GameCompleted)
            return;
        
        m_Stopwatch.Unpause();
        m_CanvasAnimation.Play("BookOut");
        m_BookOpen = false;
        Instantiate(m_BookSFX).GetComponent<AudioSource>().pitch -= 0.07f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene(0);
    }

    public void PlayHoverSound()
    {
        Instantiate(m_ButtonHoverSFX);
    }
    
    public void PlayClickSound()
    {
        Instantiate(m_ButtonClickSFX);
    }

    public void SetVolume(Slider vol)
    {
        m_Volume = vol.value;
    }

    private IEnumerator CloseBookWait()
    {
        m_CanCloseBook = false;
        yield return new WaitForSeconds(0.5f);
        m_CanCloseBook = true;
    }

    private IEnumerator PillowCutscene()
    {
        m_PillowCam.SetActive(true);
        m_SecretPillow.SetActive(true);
        yield return new WaitForSeconds(4);
        m_PillowCam.SetActive(false);
    }
}
