using UnityEngine;

public class DestroyAfterSeconds : MonoBehaviour
{
    [SerializeField] private float m_Secs = 3f;
    
    private void Start()
    {
        Destroy(gameObject, m_Secs);
    }
}
