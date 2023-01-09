using System.Collections;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance { get; set; }

    [SerializeField] private Animation m_CanvasAnimation;
    
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

    private void Awake()
    {
        Instance = this;

        m_RespawnManager = GetComponent<RespawnManager>();

        m_TotalEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;
        m_TotalCoins = GameObject.FindGameObjectsWithTag("Coin").Length;
    }

    public void CollectCoin()
    {
        m_CoinsCollected++;
        Debug.Log($"{m_CoinsCollected} / {m_TotalCoins}");
    }

    public void CollectEnemy()
    {
        m_EnemiesCollected++;
        Debug.Log($"{m_EnemiesCollected} / {m_TotalEnemies}");

        if (m_EnemiesCollected >= m_TotalEnemies)
        {
            m_GameCompleted = true;
            OpenBook();
            Debug.Log("YOU WIN!!!!!!");
        }
    }

    public void StartGame()
    {
        m_Started = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        m_CanvasAnimation.Play("StartGame");
    }

    public void OpenBook()
    {
        m_CanvasAnimation.Play("BookIn");
        m_BookOpen = true;
        StartCoroutine(CloseBookWait());
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }
    
    public void CloseBook()
    {
        if (!m_CanCloseBook || m_GameCompleted)
            return;
        
        m_CanvasAnimation.Play("BookOut");
        m_BookOpen = false;
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private IEnumerator CloseBookWait()
    {
        m_CanCloseBook = false;
        yield return new WaitForSeconds(0.5f);
        m_CanCloseBook = true;
    }
}
