using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Transform m_CameraRig;
    
    [Header("Settings")]
    [SerializeField] private float m_Speed = 2;
    [SerializeField] private float m_TurnAnimSpeed = 2f;

    private Rigidbody m_Rigidbody;
    
    private Quaternion m_LookRotation = Quaternion.identity;
    private Vector2 m_Input;

    private void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }
    
    private void Update()
    {
        m_Input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
    }
    
    private void FixedUpdate()
    {
        Vector3 relativeInput = Quaternion.Euler(0, m_CameraRig.eulerAngles.y, 0) * new Vector3(m_Input.x, 0f, m_Input.y);

        if (m_Input != Vector2.zero)
        {
            m_LookRotation = Quaternion.LookRotation(relativeInput, transform.up);
        }
        
        m_Rigidbody.AddForce(relativeInput * m_Speed);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, m_LookRotation, m_TurnAnimSpeed);
    }
}
