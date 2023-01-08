using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance { get; set; }

    private RespawnManager m_RespawnManager;
    public RespawnManager Respawn => m_RespawnManager;

    private void Awake()
    {
        Instance = this;

        m_RespawnManager = GetComponent<RespawnManager>();
    }
}
