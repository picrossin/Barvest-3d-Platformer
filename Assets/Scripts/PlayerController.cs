using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Transform m_CameraRig;
    [SerializeField] private LayerMask m_GroundLayerMask;
    
    [Header("Settings")]
    [SerializeField] private float m_MaxSpeed = 2f;
    [SerializeField] private float m_Accel = 2f;
    [SerializeField] private float m_TurnAnimSpeed = 2f;
    [SerializeField] private float m_JumpForce = 30f;
    [SerializeField] private float m_DoubleJumpForce = 5f;

    private Rigidbody m_Rigidbody;
    
    private Quaternion m_LookRotation = Quaternion.identity;
    private Vector2 m_Input;
    private Vector3 m_SlopeNormal;

    private bool m_DoubleJumped;

    private void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }
    
    private void Update()
    {
        m_Input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, out RaycastHit hit, 0.3f, m_GroundLayerMask))
            {
                m_Rigidbody.AddForce(Vector3.up * m_JumpForce, ForceMode.Impulse);
                m_DoubleJumped = false;
            }
            else if (!m_DoubleJumped)
            {
                m_Rigidbody.AddForce(Vector3.up * m_DoubleJumpForce, ForceMode.Impulse);
                m_DoubleJumped = true;
            }
        }
    }
    
    private void FixedUpdate()
    {
        // Running
        Vector3 relativeInput = Quaternion.Euler(0, m_CameraRig.eulerAngles.y, 0) * new Vector3(m_Input.x, 0f, m_Input.y);

        // Handle slopes
        if (Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, out RaycastHit hit, 0.3f, m_GroundLayerMask))
        {
            transform.rotation =
                Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, hit.normal), hit.normal);
            relativeInput = Vector3.ProjectOnPlane(relativeInput, hit.normal);
            Debug.DrawRay(transform.position + Vector3.up * 0.2f, Vector3.down * 0.3f, Color.red);
        }

        // Clamp movement speed
        float desiredSpeed = m_MaxSpeed;

        Vector3 desiredVelocity = relativeInput * desiredSpeed;
        Vector3 rbVel = m_Rigidbody.velocity;
        Vector3 rbXZVelocity = new Vector3(rbVel.x, 0f, rbVel.z);
        Vector3 movementDifference = desiredVelocity - rbXZVelocity;

        float accel = desiredVelocity.magnitude > rbXZVelocity.magnitude ? m_Accel : m_Accel / 2;

        Vector3 forceThisFrame = movementDifference * accel;
        
        m_Rigidbody.AddForce(forceThisFrame);
        
        // Turning model
        if (m_Input.magnitude > 0.05f)
            m_LookRotation = Quaternion.LookRotation(relativeInput, transform.up); 
        
        if (m_Rigidbody.velocity.magnitude > 1f)
            transform.rotation = Quaternion.RotateTowards(transform.rotation, m_LookRotation, m_TurnAnimSpeed);
    }
}
