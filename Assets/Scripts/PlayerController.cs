using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.AI;

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
    [SerializeField] private Material m_FullCocoonMat;
    
    [Header("Settings")]
    [SerializeField] private float m_MaxSpeed = 2f;
    public float MaxSpeed
    {
        get => m_MaxSpeed;
        set => m_MaxSpeed = value;
    }
    
    [SerializeField] private float m_Accel = 2f;
    [SerializeField] private float m_TurnAnimSpeed = 2f;
    [SerializeField] private float m_TurnAnimSpeedFast = 2f;
    [SerializeField] private float m_JumpForce = 30f;
    [SerializeField] private float m_DoubleJumpForce = 5f;
    [SerializeField] private float m_Friction = 1f;
    [SerializeField] private float m_CameraFollowDistance = 3f;
    [SerializeField] private float m_GrappleExtraForce = 3f;
    [SerializeField] private float m_EnemySwallowSpeed = 0.05f;
    
    [Header("Audio")]
    [SerializeField] private GameObject m_WebSplatSFX;
    [SerializeField] private GameObject m_ChompSFX;
    [SerializeField] private GameObject m_JumpSFX;
    [SerializeField] private GameObject m_WrappedSFX;

    private Rigidbody m_Rigidbody;
    private Collider m_Collider;
    private EnemyDistanceDetection m_EnemyDistanceDetection;
    private GrappleDistanceDetection m_GrappleDistanceDetection;
    private Cinemachine3rdPersonFollow m_ThirdPersonCam;
    private LineRenderer m_WebLineRenderer;

    private Quaternion m_LookRotation = Quaternion.identity;
    private Vector2 m_Input;
    private Vector3 m_SlopeNormal;

    private bool m_DoubleJumped;
    public bool DoubleJumped
    {
        get => m_DoubleJumped;
        set => m_DoubleJumped = value;
    }

    private bool m_Wrapping;
    private BasicEnemy m_WrappingEnemy;
    private float m_WrapStartAngle;
    private bool m_WrapDirectionChosen;
    private bool m_WrappingClockwise;
    private float m_WrappedAmount;

    private bool m_Grappling;
    private GameObject m_GrapplePoint;
    private SpringJoint m_GrappleJoint;

    private void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Collider = GetComponent<Collider>();
        m_EnemyDistanceDetection = GetComponentInChildren<EnemyDistanceDetection>();
        m_GrappleDistanceDetection = GetComponentInChildren<GrappleDistanceDetection>();
        m_ThirdPersonCam = m_ThirdPersonVirtualCam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        m_ThirdPersonCam.CameraDistance = m_CameraFollowDistance;
    }
    
    private void Update()
    {
        if (GameplayManager.Instance.Started)
            m_Input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        else
            return;

        // Menu opening
        if (Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameplayManager.Instance.BookOpen)
            {
                GameplayManager.Instance.CloseBook();
            }
            else
            {
                GameplayManager.Instance.OpenBook();
            }
        }

        if (GameplayManager.Instance.BookOpen)
            return;
        
        // Jumping
        if (Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, out RaycastHit _, 0.3f,
            m_GroundLayerMask))
        {
            m_DoubleJumped = false;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, out RaycastHit hit, 0.3f, m_GroundLayerMask))
            {
                m_Rigidbody.AddForce(Vector3.up * m_JumpForce, ForceMode.Impulse);
                Instantiate(m_JumpSFX, transform.position, Quaternion.identity);

                Vector3 oldVel = m_Rigidbody.velocity;
                m_Rigidbody.velocity = new Vector3(oldVel.x, 0f, oldVel.z);
            }
            else if (!m_DoubleJumped)
            {
                m_Rigidbody.AddForce(Vector3.up * m_DoubleJumpForce, ForceMode.Impulse);
                m_DoubleJumped = true;
                Instantiate(m_JumpSFX, transform.position, Quaternion.identity).GetComponent<AudioSource>().pitch += 0.1f;

                Vector3 oldVel = m_Rigidbody.velocity;
                m_Rigidbody.velocity = new Vector3(oldVel.x, 0f, oldVel.z);
            }
        }

        // Shoot web
        if (m_EnemyDistanceDetection.ClosestEnemy != null && Input.GetMouseButtonDown(0) && !m_Wrapping)
        {
            m_WrappingEnemy = m_EnemyDistanceDetection.ClosestEnemy.GetComponent<BasicEnemy>();

            Instantiate(m_WebSplatSFX, m_WrappingEnemy.transform.position, Quaternion.identity);

            if (m_WrappingEnemy.Wrapped)
            {
                Debug.Log("Enemy eaten");
                m_WrappingEnemy.Eaten = true;
                StartCoroutine(EatEnemy(m_WrappingEnemy.gameObject));
            }
            else
            {
                m_WrappingEnemy.UnderAttack = true;
                m_Wrapping = true;
                m_CameraRig.FollowObject = m_EnemyDistanceDetection.ClosestEnemy.transform;
            
                m_WrappingEnemy.WrapSound(true);
                
                Vector3 diff = transform.position - m_WrappingEnemy.transform.position;
                m_WrapStartAngle = Mathf.Atan2(diff.x, diff.z);

                m_WebLineRenderer = Instantiate(m_WebLine, Vector3.zero, Quaternion.identity).GetComponent<LineRenderer>();
                
                m_WrappingEnemy.Cocoon.SetActive(true);
                m_WrappingEnemy.Cocoon.GetComponent<MeshRenderer>().material.SetFloat("_CutoffHeight", -0.5f);
            }
        }

        // Wrap web
        if (m_Wrapping)
        {
            // Lock camera
            m_ThirdPersonCam.CameraDistance =
                Vector3.Distance(transform.position, m_WrappingEnemy.transform.position) + m_CameraFollowDistance;
            
            // Draw line
            m_WebLineRenderer.SetPositions(new[] {transform.position + Vector3.up * 0.2f, m_WrappingEnemy.Mesh.transform.position});
            
            // Track wrapping amount
            Vector3 diff = transform.position - m_WrappingEnemy.transform.position;
            float currentDegree = (m_WrapStartAngle - Mathf.Atan2(diff.x, diff.z)) * Mathf.Rad2Deg;

            // 1. Detect direction of wrap
            if (!m_WrapDirectionChosen)
            {
                if (currentDegree > 30f) // Wrapping counter-clockwise ("right")
                {
                    m_WrapDirectionChosen = true;
                    m_WrappingClockwise = false;
                }
                else if (currentDegree < -30f) // Wrapping clockwise ("left")
                {
                    m_WrapDirectionChosen = true;
                    m_WrappingClockwise = true;
                }
            }
            else
            {
                // 2. Track wrapping amount around chosen direction
                if (!m_WrappingClockwise)
                {
                    // Ensure angles are between 0 - 360 degrees
                    if (currentDegree < 0f)
                    {
                        currentDegree += 360f;
                    }
                }
                else
                {
                    // Ensure angles are between -1 - -360 degrees
                    if (currentDegree > 0f)
                    {
                        currentDegree -= 360f;
                    }

                    currentDegree = -currentDegree; // Flip sign to be positive
                }
                
                m_WrappedAmount = currentDegree / 340f;

                m_WrappingEnemy.Cocoon.GetComponent<MeshRenderer>().material.SetFloat("_CutoffHeight", Mathf.Lerp(-0.5f, 1.7f, m_WrappedAmount));
                // Debug.Log(Vector3.Distance(transform.position, m_WrappingEnemy.transform.position));
                
                if (currentDegree > 340f)
                {
                    m_WrappingEnemy.Wrapped = true;
                    Instantiate(m_WrappedSFX, m_WrappingEnemy.transform.position, Quaternion.identity);
                    ResetWrapped();
                } 
                else if (currentDegree < 20f)
                {
                    ResetWrapped();
                } 
                else if (Vector3.Distance(transform.position, m_WrappingEnemy.transform.position) > 7f)
                {
                    ResetWrapped();
                }
                else if (Physics.Raycast(transform.position + Vector3.up * 0.2f, 
                    m_WrappingEnemy.transform.position - transform.position + Vector3.up * 0.2f, out RaycastHit hit, 
                    Vector3.Distance(transform.position + Vector3.up * 0.2f, m_WrappingEnemy.transform.position),
                    m_GroundLayerMask))
                {
                    ResetWrapped();
                }
            }
            
            if (Input.GetMouseButtonUp(0))
            {
                ResetWrapped();
            }
        }
        
        // Shoot grapple web
        if (m_GrappleDistanceDetection.ClosestGrapple != null && Input.GetMouseButtonDown(0) && !m_Grappling)
        {
            m_Grappling = true;
            m_GrapplePoint = m_GrappleDistanceDetection.ClosestGrapple;
            
            Instantiate(m_WebSplatSFX, m_GrapplePoint.transform.position, Quaternion.identity);

            m_WebLineRenderer = Instantiate(m_WebLine, Vector3.zero, Quaternion.identity).GetComponent<LineRenderer>();

            m_GrappleJoint = gameObject.AddComponent<SpringJoint>();
            m_GrappleJoint.autoConfigureConnectedAnchor = false;
            m_GrappleJoint.connectedAnchor = m_GrapplePoint.transform.position;

            float distanceFromPoint = Vector3.Distance(transform.position, m_GrapplePoint.transform.position);

            m_GrappleJoint.maxDistance = distanceFromPoint * 0.6f;
            m_GrappleJoint.minDistance = distanceFromPoint * 0.25f;

            m_GrappleJoint.spring = 4.5f;
            m_GrappleJoint.damper = 7f;
            m_GrappleJoint.massScale = 4.5f;
        }

        if (m_Grappling)
        {
            m_WebLineRenderer.SetPositions(new[] {transform.position + Vector3.up * 0.2f, m_GrapplePoint.transform.position});

            if (Input.GetMouseButtonUp(0) || Input.GetKeyDown(KeyCode.Space))
            {
                m_Grappling = false;
                m_GrapplePoint = null;
                Destroy(m_GrappleJoint);
                Destroy(m_WebLineRenderer.gameObject);
                m_Rigidbody.AddForce(m_Rigidbody.velocity.normalized * m_GrappleExtraForce, ForceMode.Impulse);
            }
        }
    }
    
    private void FixedUpdate()
    {
        m_Rigidbody.isKinematic = GameplayManager.Instance.BookOpen;

        if (GameplayManager.Instance.BookOpen)
            return;
        
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
        float desiredSpeed = MaxSpeed;

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

    public bool Grounded()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, out RaycastHit hit,
            0.3f, m_GroundLayerMask);
    }

    private void ResetWrapped()
    {
        m_Wrapping = false;
        m_WrapDirectionChosen = false;
        m_CameraRig.FollowObject = null;

        m_WrappingEnemy.WrapSound(false);
        m_WrappingEnemy.Cocoon.GetComponent<MeshRenderer>().material.SetFloat("_CutoffHeight", 0f);

        if (m_WrappingEnemy.Wrapped)
        {
            m_WrappingEnemy.Cocoon.GetComponent<MeshRenderer>().material = m_FullCocoonMat;
        }
        else
        {
            m_WrappingEnemy.Cocoon.GetComponent<MeshRenderer>().material.SetFloat("_CutoffHeight", 0f);
            m_WrappingEnemy.Cocoon.SetActive(false);
        }
        
        m_WrappingEnemy.UnderAttack = false;
        m_WrappingEnemy = null;
        m_ThirdPersonCam.CameraDistance = m_CameraFollowDistance;
        Destroy(m_WebLineRenderer.gameObject);
    }

    private IEnumerator EatEnemy(GameObject enemy)
    {
        enemy.GetComponent<NavMeshAgent>().enabled = false;
        enemy.GetComponent<BoxCollider>().enabled = false;

        m_WebLineRenderer = Instantiate(m_WebLine, Vector3.zero, Quaternion.identity).GetComponent<LineRenderer>();

        while (Vector3.Distance(enemy.transform.position, transform.position) > 0.5f)
        {
            m_WebLineRenderer.SetPositions(new[] {transform.position + Vector3.up * 0.2f, enemy.GetComponent<BasicEnemy>().Mesh.transform.position});

            enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, transform.position, m_EnemySwallowSpeed);
            yield return new WaitForEndOfFrame();
        }
        
        enemy.GetComponent<BasicEnemy>().CollectionImage.color = Color.white;

        Instantiate(m_ChompSFX, transform.position, Quaternion.identity);
        Destroy(m_WebLineRenderer.gameObject);
        Destroy(enemy);
        
        GameplayManager.Instance.CollectEnemy();
    }
}
