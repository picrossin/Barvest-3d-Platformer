using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BasicEnemy : MonoBehaviour
{
    // Editor values
    [SerializeField] private List<Vector3> m_PathPositions;
    [SerializeField] private float m_MoveSpeed = 3.5f;
    [SerializeField] private float m_UnderAttackSpeed = 0.5f;

    // Private values
    private NavMeshAgent m_Enemy;
    private int m_CurrentDestinationIndex;
    private bool m_Turning = false;
    [SerializeField] private bool m_UnderAttack = false;

    // AddPositionToPath is used by an editor script to populate the m_PathPositions List
    public void AddPositionToPath()
    {
        m_PathPositions.Add(transform.position);
    }

    void Start()
    {
        m_Enemy = GetComponent<NavMeshAgent>();
        m_Enemy.destination = m_PathPositions[m_CurrentDestinationIndex];
    }

    void Update()
    {
        // Draw Debug Ray
        Vector3 forward = transform.TransformDirection(Vector3.forward) * 1;
        Debug.DrawRay(transform.position, forward, Color.green);

        // Check if under attack
        if (m_UnderAttack && m_Enemy.speed == m_MoveSpeed)
        {
            Debug.Log("Under Attack!!!");
            m_Enemy.speed = m_UnderAttackSpeed;
        } else if (!m_UnderAttack && m_Enemy.speed == m_UnderAttackSpeed)
        {
            Debug.Log("No Longer Under Attack, Yippee!!!");
            m_Enemy.speed = m_MoveSpeed;
        }

        // Move
        MoveEnemy();
    }

    void MoveEnemy()
    {
        // Check if we've reached the destination
        // https://answers.unity.com/questions/324589/how-can-i-tell-when-a-navmesh-has-reached-its-dest.html
        if (m_Turning) return;
        if (m_Enemy.pathPending) return;
        if (m_Enemy.remainingDistance > m_Enemy.stoppingDistance) return;
        if (m_Enemy.hasPath || m_Enemy.velocity.sqrMagnitude != 0f) return;

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
