using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class EnemyDistanceDetection : MonoBehaviour
{
    // Editor Values
    [SerializeField] private float m_ColliderDistance;
    [SerializeField] private RectTransform m_CanvasRect;
    [SerializeField] private RectTransform m_WebIcon;
    
    // Private Values
    private GameObject m_ClosestEnemy;
    public GameObject ClosestEnemy => m_ClosestEnemy;

    private SphereCollider m_SphereCollider;
    private List<GameObject> m_EnemiesInRange = new List<GameObject>();
    private PlayerController m_PlayerController;

    void Start()
    {
        m_SphereCollider = GetComponent<SphereCollider>();
        m_SphereCollider.isTrigger = true;
        m_SphereCollider.radius = m_ColliderDistance;
        m_WebIcon.gameObject.SetActive(false);
        m_PlayerController = transform.parent.GetComponent<PlayerController>();
    }

    void Update()
    {
        GameObject closestEnemy = GetClosestEnemy();
        if (closestEnemy && !m_PlayerController.Wrapping)
        {
            Vector2 viewportPos = Camera.main.WorldToViewportPoint(closestEnemy.GetComponent<BasicEnemy>().Mesh.transform.position);
            Vector2 targetPoint = new Vector2(
                viewportPos.x * m_CanvasRect.sizeDelta.x - m_CanvasRect.sizeDelta.x * 0.5f,
                viewportPos.y * m_CanvasRect.sizeDelta.y - m_CanvasRect.sizeDelta.y * 0.5f);

            m_WebIcon.gameObject.SetActive(true);
            m_WebIcon.anchoredPosition = targetPoint;
        } else
        {
            m_WebIcon.gameObject.SetActive(false);
        }

        if (closestEnemy != m_ClosestEnemy)
        {
            m_ClosestEnemy = closestEnemy;
        }
    }

    // Get the closest enemy to the player using the m_EnemiesInRange list
    private GameObject GetClosestEnemy()
    {
        // If there are no enemies in range return null
        if (m_EnemiesInRange.Count == 0) return null;
        // If there is only one enemy in range return that enemy
        if (m_EnemiesInRange.Count == 1)
        {
            if (m_EnemiesInRange[0] == null)
            {
                m_EnemiesInRange.RemoveAt(0);
                return null;
            }
            
            if (m_EnemiesInRange[0].GetComponent<BasicEnemy>().Eaten)
                return null;
            
            return m_EnemiesInRange[0];
        }

        Debug.Log("Getting closest enemy");
        // Set up base values for comparing against all other enemies in range
        GameObject closestEnemy = m_EnemiesInRange[0];
        float shortestDistance = Vector3.Distance(transform.position, closestEnemy.transform.position);
        // Loop through each enemy in range to find the one with shortest distance
        m_EnemiesInRange.RemoveAll(e => e == null);
        foreach (GameObject enemy in m_EnemiesInRange)
        {
            if (enemy.GetComponent<BasicEnemy>().Eaten)
                continue;
            
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
