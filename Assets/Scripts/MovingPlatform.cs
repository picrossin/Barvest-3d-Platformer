using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    private Transform m_PreviousParent;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            m_PreviousParent = other.transform.parent;
            other.transform.parent = transform;
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.parent = m_PreviousParent;
        }
    }
}
