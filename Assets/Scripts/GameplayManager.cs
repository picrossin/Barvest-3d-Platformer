using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance { get; set; }

    private bool m_Started;
    public bool Started => m_Started;

    private int m_TotalEnemies;
    private int m_EnemiesCollected;

    private int m_TotalCoins;
    private int m_CoinsCollected;
    
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
            Debug.Log("YOU WIN!!!!!!");
        }
    }

    public void StartGame()
    {
        m_Started = true;
    }
}
