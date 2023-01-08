using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class EnemyDistanceDetection : MonoBehaviour
{
    // Editor Values
    [SerializeField] private float m_ColliderDistance;
    [SerializeField] private Material m_FirstMaterial;
    [SerializeField] private Material m_SecondMaterial;
    [SerializeField] private RectTransform m_CanvasRect;
    [SerializeField] private RectTransform m_WebIcon;

    // Private Values
    private GameObject m_ClosestEnemy;
    public GameObject ClosestEnemy => m_ClosestEnemy;

    private SphereCollider m_SphereCollider;
    private List<GameObject> m_EnemiesInRange = new List<GameObject>();

    void Start()
    {
        m_SphereCollider = GetComponent<SphereCollider>();
        m_SphereCollider.isTrigger = true;
        m_SphereCollider.radius = m_ColliderDistance;
        m_WebIcon.gameObject.SetActive(false);
    }

    void Update()
    {
        GameObject closestEnemy = GetClosestEnemy();
        if (closestEnemy)
        {
            closestEnemy.GetComponent<MeshRenderer>().material = m_SecondMaterial;
            Vector2 viewportPos = Camera.main.WorldToViewportPoint(closestEnemy.transform.position);
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
