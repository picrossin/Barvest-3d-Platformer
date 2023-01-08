using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class GrappleDistanceDetection : MonoBehaviour
{
    // Editor Values
    [SerializeField] private float m_ColliderDistance;
    [SerializeField] private Material m_FirstMaterial;
    [SerializeField] private Material m_SecondMaterial;
    [SerializeField] private RectTransform m_CanvasRect;
    [SerializeField] private RectTransform m_WebIcon;

    // Private Values
    private GameObject m_ClosestGrapple;
    public GameObject ClosestGrapple => m_ClosestGrapple;

    private SphereCollider m_SphereCollider;
    private List<GameObject> m_GrapplesInRange = new List<GameObject>();

    void Start()
    {
        m_SphereCollider = GetComponent<SphereCollider>();
        m_SphereCollider.isTrigger = true;
        m_SphereCollider.radius = m_ColliderDistance;
        m_WebIcon.gameObject.SetActive(false);
    }

    void Update()
    {
        GameObject closestGrapple = GetClosestGrapple();
        if (closestGrapple)
        {
            closestGrapple.GetComponent<MeshRenderer>().material = m_SecondMaterial;
            Vector2 viewportPos = Camera.main.WorldToViewportPoint(closestGrapple.transform.position);
            Vector2 targetPoint = new Vector2(
                viewportPos.x * m_CanvasRect.sizeDelta.x - m_CanvasRect.sizeDelta.x * 0.5f,
                viewportPos.y * m_CanvasRect.sizeDelta.y - m_CanvasRect.sizeDelta.y * 0.5f);

            m_WebIcon.gameObject.SetActive(true);
            m_WebIcon.anchoredPosition = targetPoint;
        } else
        {
            m_WebIcon.gameObject.SetActive(false);
        }

        if (closestGrapple != m_ClosestGrapple)
        {
            IndicateClosestGrapple(m_ClosestGrapple, closestGrapple);
            m_ClosestGrapple = closestGrapple;
        }
    }

    private void IndicateClosestGrapple(GameObject oldClosestGrapple, GameObject newClosestGrapple)
    {
        if (oldClosestGrapple)
        {
            oldClosestGrapple.GetComponent<MeshRenderer>().material = m_FirstMaterial;
        }
        if (newClosestGrapple)
        {
            newClosestGrapple.GetComponent<MeshRenderer>().material = m_SecondMaterial;
        }
    }

    // Get the closest grapple to the player using the m_EnemiesInRange list
    private GameObject GetClosestGrapple()
    {
        // If there are no grapples range return null
        if (m_GrapplesInRange.Count == 0) return null;
        // If there is only one grapple in range return that grapple
        if (m_GrapplesInRange.Count == 1) return m_GrapplesInRange[0];

        // Set up base values for comparing against all other enemies in range
        GameObject closestGrapple = m_GrapplesInRange[0];
        float shortestDistance = Vector3.Distance(transform.position, closestGrapple.transform.position);
        // Loop through each enemy in range to find the one with shortest distance
        foreach(GameObject grapple in m_GrapplesInRange)
        {
            float distance = Vector3.Distance(transform.position, grapple.transform.position);
            if (distance < shortestDistance)
            {
                closestGrapple = grapple;
                shortestDistance = distance;
            }
        }

        // Return the enemy closest to the player from the enemies in range list
        return closestGrapple;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Grapple"))
        {
            Vector3 targetPosition = other.transform.position - transform.position;
            Debug.DrawRay(transform.position, targetPosition, Color.yellow);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Grapple"))
        {
            m_GrapplesInRange.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Grapple"))
        {
            m_GrapplesInRange.Remove(other.gameObject);
        }
    }
}
