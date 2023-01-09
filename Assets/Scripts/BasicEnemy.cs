using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

[RequireComponent(typeof(NavMeshAgent))]
public class BasicEnemy : MonoBehaviour
{
    // Editor values
    [SerializeField] private List<Vector3> m_PathPositions;
    [SerializeField] private float m_MoveSpeed = 3.5f;
    [SerializeField] private float m_UnderAttackSpeed = 0.5f;
    
    // Private values
    [SerializeField] private bool m_UnderAttack = false;
    public bool UnderAttack
    {
        get => m_UnderAttack;
        set => m_UnderAttack = value;
    }

    [SerializeField] private GameObject m_MeshRenderer;
    public GameObject Mesh => m_MeshRenderer;

    [SerializeField] private GameObject m_Cocoon;
    public GameObject Cocoon => m_Cocoon;
    
    [SerializeField] private Image m_CollectionImage;
    public Image CollectionImage => m_CollectionImage;

    [SerializeField] private bool m_Boss;
    
    private bool m_Wrapped;
    public bool Wrapped
    {
        get => m_Wrapped;
        set => m_Wrapped = value;
    }
    
    private bool m_Eaten;
    public bool Eaten
    {
        get => m_Eaten;
        set => m_Eaten = value;
    }

    private NavMeshAgent m_Enemy;
    private int m_CurrentDestinationIndex;
    private bool m_Turning = false;
    private float m_OldSpeed;
    private Transform m_LookAtPlayer;

    // AddPositionToPath is used by an editor script to populate the m_PathPositions List
    public void AddPositionToPath()
    {
        m_PathPositions.Add(transform.position);
    }

    void Start()
    {
        if (!m_Boss)
        {
            m_Enemy = GetComponent<NavMeshAgent>();
            m_Enemy.destination = m_PathPositions[m_CurrentDestinationIndex];
            m_Enemy.speed = m_MoveSpeed;
            m_Enemy.updatePosition = false;
        }
    }

    void Update()
    {
        if (m_Boss)
            return;
        
        if (m_Wrapped)
        {
            m_Enemy.speed = 0f;
            return;
        }

        if (GameplayManager.Instance.BookOpen)
        {
            if (m_OldSpeed == 0)
            {
                m_OldSpeed = m_Enemy.speed;
            }
            
            m_Enemy.speed = 0;
            return;
        }
        
        if (m_OldSpeed != 0)
        {
            m_Enemy.speed = m_OldSpeed;
            m_OldSpeed = 0;
        }
        
        // Draw Debug Ray
        Vector3 forward = transform.TransformDirection(Vector3.forward) * 1;
        Debug.DrawRay(transform.position, forward, Color.green);

        // Check if under attack
        if (m_UnderAttack && m_Enemy.speed == m_MoveSpeed)
        {
            Debug.Log("Under Attack!!!");
            m_Enemy.speed = m_UnderAttackSpeed;
        } 
        else if (!m_UnderAttack && m_Enemy.speed == m_UnderAttackSpeed)
        {
            Debug.Log("No Longer Under Attack, Yippee!!!");
            m_Enemy.speed = m_MoveSpeed;
        }

        // Move
        MoveEnemy();
    }

    private void FixedUpdate()
    {
        if (GameplayManager.Instance.BookOpen)
            return;
        
        if (m_Boss)
        {
            if (m_LookAtPlayer == null)
            {
                GameObject p = GameObject.FindGameObjectWithTag("Player");

                if (p != null)
                    m_LookAtPlayer = p.transform;
            }

            if (m_LookAtPlayer != null)
            {
                Vector3 playerXZ = new Vector3(m_LookAtPlayer.position.x, transform.position.y, m_LookAtPlayer.position.z);
                transform.rotation = quaternion.LookRotation(playerXZ - transform.position, Vector3.up);
            }
        }
        else
        {
            transform.position = m_Enemy.nextPosition;
        }
    }

    void MoveEnemy()
    {
        // Check if we've reached the destination
        // https://answers.unity.com/questions/324589/how-can-i-tell-when-a-navmesh-has-reached-its-dest.html
        if (m_Turning) return;
        if (m_Enemy.pathPending) return;
        if (m_Enemy.remainingDistance > m_Enemy.stoppingDistance) return;
        if (m_Enemy.hasPath || m_Enemy.velocity.sqrMagnitude != 0f) return;
        if (m_Boss) return;

        StartCoroutine(SmoothTurn());
    }

    IEnumerator SmoothTurn()
    {
        // Start turning
        m_Turning = true;

        // Update destination index and get the next destination position
        m_CurrentDestinationIndex = (m_CurrentDestinationIndex + 1) % m_PathPositions.Count;
        Vector3 nextDestination = m_PathPositions[m_CurrentDestinationIndex];

        // Get the starting and target rotation for the slerp
        Quaternion startingRotation = transform.rotation;
        Vector3 direction = (nextDestination - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        float timeTaken = 0f;
        float timeToTake = .5f;
        while (timeTaken < timeToTake)
        {
            transform.rotation = Quaternion.Slerp(startingRotation, targetRotation, timeTaken / timeToTake);
            timeTaken += Time.deltaTime;
            yield return null;
        }

        // Set the nav mesh agent destination
        m_Enemy.destination = nextDestination;
        
        // Stop turning
        m_Turning = false;
    }
}
