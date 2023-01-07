using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Transform m_CameraRig;
    [SerializeField] private LayerMask m_GroundLayerMask;
    [SerializeField] private PhysicMaterial m_noFriction;
    [SerializeField] private PhysicMaterial m_HighFriction;
    
    [Header("Settings")]
    [SerializeField] private float m_MaxSpeed = 2f;
    [SerializeField] private float m_Accel = 2f;
    [SerializeField] private float m_TurnAnimSpeed = 2f;
    [SerializeField] private float m_TurnAnimSpeedFast = 2f;
    [SerializeField] private float m_JumpForce = 30f;
    [SerializeField] private float m_DoubleJumpForce = 5f;
    [SerializeField] private float m_Friction = 1f;

    private Rigidbody m_Rigidbody;
    private Collider m_Collider;
    
    private Quaternion m_LookRotation = Quaternion.identity;
    private Vector2 m_Input;
    private Vector3 m_SlopeNormal;

    private bool m_DoubleJumped;

    private void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Collider = GetComponent<Collider>();
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
                
                Vector3 oldVel = m_Rigidbody.velocity;
                m_Rigidbody.velocity = new Vector3(oldVel.x, 0f, oldVel.z);
            }
            else if (!m_DoubleJumped)
            {
                m_Rigidbody.AddForce(Vector3.up * m_DoubleJumpForce, ForceMode.Impulse);
                m_DoubleJumped = true;
                
                Vector3 oldVel = m_Rigidbody.velocity;
                m_Rigidbody.velocity = new Vector3(oldVel.x, 0f, oldVel.z);
            }
        }
    }
    
    private void FixedUpdate()
    {
        // Running
        Vector3 relativeInput = Quaternion.Euler(0, m_CameraRig.eulerAngles.y, 0) * new Vector3(m_Input.x, 0f, m_Input.y);

        // Handle slopes
        bool grounded = false;
        if (Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, out RaycastHit hit, 0.3f, m_GroundLayerMask))
        {
            grounded = true;
            transform.rotation =
                Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, hit.normal), hit.normal);
            relativeInput = Vector3.ProjectOnPlane(relativeInput, hit.normal);
            
            Debug.DrawRay(transform.position, relativeInput, Color.magenta);

            if (hit.normal != Vector3.up)
            {
                m_Collider.material = m_Input.magnitude <= Mathf.Epsilon ? m_HighFriction : m_noFriction;
            }
            
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

        if (rbXZVelocity.magnitude > 0f)
        {
            if (Quaternion.Angle(transform.rotation, m_LookRotation) < 100f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, m_LookRotation, m_TurnAnimSpeed);
            }
            else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, m_LookRotation, m_TurnAnimSpeedFast);
            }
        }
        
        // Friction
        if (grounded && rbXZVelocity.magnitude > Mathf.Epsilon && m_Input.magnitude <= Mathf.Epsilon)
        {
            m_Rigidbody.AddForce(-rbXZVelocity * m_Friction);
        }
    }
}
