using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private CameraController m_CameraRig;
    [SerializeField] private LayerMask m_GroundLayerMask;
    [SerializeField] private PhysicMaterial m_noFriction;
    [SerializeField] private PhysicMaterial m_HighFriction;
    [SerializeField] private CinemachineVirtualCamera m_ThirdPersonVirtualCam;
    [SerializeField] private GameObject m_WebLine;
    
    [Header("Settings")]
    [SerializeField] private float m_MaxSpeed = 2f;
    [SerializeField] private float m_Accel = 2f;
    [SerializeField] private float m_TurnAnimSpeed = 2f;
    [SerializeField] private float m_TurnAnimSpeedFast = 2f;
    [SerializeField] private float m_JumpForce = 30f;
    [SerializeField] private float m_DoubleJumpForce = 5f;
    [SerializeField] private float m_Friction = 1f;
    [SerializeField] private float m_CameraFollowDistance = 3f;

    private Rigidbody m_Rigidbody;
    private Collider m_Collider;
    private EnemyCollisionDetection m_EnemyCollisionDetection;
    private Cinemachine3rdPersonFollow m_ThirdPersonCam;
    private LineRenderer m_WebLineRenderer;

    private Quaternion m_LookRotation = Quaternion.identity;
    private Vector2 m_Input;
    private Vector3 m_SlopeNormal;

    private bool m_DoubleJumped;
    
    private bool m_Wrapping;
    private BasicEnemy m_WrappingEnemy;
    private float m_WrapStartAngle;
    private float[] m_WrapAngles = { 45f, 90f, 135f, 180f, -135f, -90f, -45f, 0f, 45f, 90f, 135f, 180f, -135f, -90f, -45f};
    private int m_CurrentAngle = 7;

    private void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Collider = GetComponent<Collider>();
        m_EnemyCollisionDetection = GetComponent<EnemyCollisionDetection>();
        m_ThirdPersonCam = m_ThirdPersonVirtualCam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        m_ThirdPersonCam.CameraDistance = m_CameraFollowDistance;
    }
    
    private void Update()
    {
        m_Input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        // Jumping
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
        
        // Shoot web
        if (m_EnemyCollisionDetection.ClosestEnemy != null && Input.GetMouseButtonDown(0) && !m_Wrapping)
        {
            m_Wrapping = true;
            m_CameraRig.FollowObject = m_EnemyCollisionDetection.ClosestEnemy.transform;
            m_WrappingEnemy = m_EnemyCollisionDetection.ClosestEnemy.GetComponent<BasicEnemy>();
            m_WrappingEnemy.UnderAttack = true;
            
            Vector3 diff = transform.position - m_WrappingEnemy.transform.position;
            m_WrapStartAngle = Mathf.Atan2(diff.x, diff.z);

            m_WebLineRenderer = Instantiate(m_WebLine, Vector3.zero, Quaternion.identity).GetComponent<LineRenderer>();
        }

        // Wrap web
        if (m_Wrapping)
        {
            // Lock camera
            m_ThirdPersonCam.CameraDistance =
                Vector3.Distance(transform.position, m_WrappingEnemy.transform.position) + m_CameraFollowDistance;
            
            // Draw line
            m_WebLineRenderer.SetPositions(new[] {transform.position + Vector3.up * 0.2f, m_WrappingEnemy.transform.position});
            
            // Track wrapping amount
            Vector3 diff = transform.position - m_WrappingEnemy.transform.position;
            float currentDegree = (m_WrapStartAngle - Mathf.Atan2(diff.x, diff.z)) * Mathf.Rad2Deg;

            if (Mathf.Abs(m_WrapAngles[m_CurrentAngle + 1] - currentDegree) <= 7f)
            {
                m_CurrentAngle++;
                if (m_CurrentAngle == m_WrapAngles.Length - 1)
                {
                    Wrapped();
                }
            }
            else if (Mathf.Abs(m_WrapAngles[m_CurrentAngle - 1] - currentDegree) <= 7f)
            {
                m_CurrentAngle--;
                if (m_CurrentAngle == 0)
                {
                    Wrapped();
                }
            } 
            else if (Input.GetMouseButtonUp(0))
            {
                Wrapped();
            }
        }
    }
    
    private void FixedUpdate()
    {
        // Running
        Vector3 relativeInput = Quaternion.Euler(0, m_CameraRig.transform.eulerAngles.y, 0) * new Vector3(m_Input.x, 0f, m_Input.y);

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

    private void Wrapped()
    {
        m_CurrentAngle = 7;
        m_Wrapping = false;
        m_CameraRig.FollowObject = null;
        m_WrappingEnemy.UnderAttack = false;
        m_WrappingEnemy = null;
        m_ThirdPersonCam.CameraDistance = m_CameraFollowDistance;
        Destroy(m_WebLineRenderer.gameObject);
    }
}
