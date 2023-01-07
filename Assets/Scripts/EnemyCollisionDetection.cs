using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class EnemyCollisionDetection : MonoBehaviour
{
    // Editor Values
    [SerializeField] private float m_ColliderDistance;
    [SerializeField] private Material m_FirstMaterial;
    [SerializeField] private Material m_SecondMaterial;

    // Private Values
    private SphereCollider m_SphereCollider;
    private List<GameObject> m_EnemiesInRange = new List<GameObject>();
    private GameObject m_ClosestEnemy;

    void Start()
    {
        m_SphereCollider = GetComponent<SphereCollider>();
        m_SphereCollider.isTrigger = true;
        m_SphereCollider.radius = m_ColliderDistance;
    }

    void Update()
    {
        GameObject closestEnemy = GetClosestEnemy();
        if (closestEnemy != m_ClosestEnemy)
        {
            Debug.Log("Closest enemy changed");
            IndicateClosestEnemy(m_ClosestEnemy, closestEnemy);
            m_ClosestEnemy = closestEnemy;
        }
    }

    private void IndicateClosestEnemy(GameObject oldClosestEnemy, GameObject newClosestEnemy)
    {
        if (oldClosestEnemy)
        {
            oldClosestEnemy.GetComponent<MeshRenderer>().material = m_FirstMaterial;
        }
        if (newClosestEnemy)
        {
            newClosestEnemy.GetComponent<MeshRenderer>().material = m_SecondMaterial;
        }
    }

    // Get the closest enemy to the player using the m_EnemiesInRange list
    private GameObject GetClosestEnemy()
    {
        // If there are no enemies in range return null
        if (m_EnemiesInRange.Count == 0) return null;
        // If there is only one enemy in range return that enemy
        if (m_EnemiesInRange.Count == 1) return m_EnemiesInRange[0];

        Debug.Log("Getting closest enemy");
        // Set up base values for comparing against all other enemies in range
        GameObject closestEnemy = m_EnemiesInRange[0];
        float shortestDistance = Vector3.Distance(transform.position, closestEnemy.transform.position);
        // Loop through each enemy in range to find the one with shortest distance
        foreach(GameObject enemy in m_EnemiesInRange)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < shortestDistance)
            {
                closestEnemy = enemy;
                shortestDistance = distance;
            }
        }

        // Return the enemy closest to the player from the enemies in range list
        return closestEnemy;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Vector3 targetPosition = other.transform.position - transform.position;
            Debug.DrawRay(transform.position, targetPosition, Color.red);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            m_EnemiesInRange.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            m_EnemiesInRange.Remove(other.gameObject);
        }
    }
}
