using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKFootSolver : MonoBehaviour
{
    [SerializeField] Transform body;
    [SerializeField] float footSpacing;
    [SerializeField] float footOffset;
    [SerializeField] LayerMask terrainLayer;
    [SerializeField] float stepDistance;
    [SerializeField] float speed;
    [SerializeField] float stepHeight;
    [SerializeField] float airSmoothing = 1f;
    [SerializeField] private GameObject m_FootstepSFX;

    Vector3 newPosition;
    Vector3 currentPosition;
    Vector3 oldPosition;
    float lerp;
    bool initializedAir;
    Vector3 airPosition;

    PlayerController player;
    Platform platform;
    private Vector3 platformOffset;
    
    // Start is called before the first frame update
    void Start()
    {
        newPosition = transform.position;
        currentPosition = transform.position;
        player = body.GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (player == null || player.Grounded())
        {
            if (platform != null)
            {
                newPosition = platform.transform.position + platformOffset;
            }
            
            transform.position = currentPosition;

            Ray ray = new Ray(body.position + (body.right * footSpacing) + (body.forward * footOffset) + (body.up * 0.1f), Vector3.down);
            Debug.DrawRay(ray.origin, ray.direction * 10, Color.red);
        
            if (Physics.Raycast(ray, out RaycastHit info, 10, terrainLayer))
            {
                if (Vector3.Distance(newPosition, info.point) > stepDistance)
                {
                    // Play footstep sound
                    if (m_FootstepSFX)
                        Instantiate(m_FootstepSFX, currentPosition, Quaternion.identity).GetComponent<AudioSource>().pitch += Random.Range(-0.05f, 0.05f);
                    
                    lerp = 0;
                    newPosition = info.point;

                    platform = null;

                    if (info.transform.TryGetComponent(out Platform newPlatform))
                    {
                        platform = newPlatform;
                        platformOffset = newPosition - platform.transform.position;
                    }
                }
            }
            
            if (lerp < 1)
            {
                Vector3 footPosition = Vector3.Lerp(oldPosition, newPosition, lerp);
                footPosition.y += Mathf.Sin(lerp * Mathf.PI) * stepHeight;

                currentPosition = footPosition;
                lerp += speed;
            }
            else
            {
                currentPosition = newPosition;
                oldPosition = newPosition;
            }
            
            initializedAir = false;
        }
        else
        {
            if (!initializedAir)
            {
                airPosition = transform.position;
                initializedAir = true;
            }
            
            airPosition = Vector3.Lerp(airPosition, body.position + (body.right * footSpacing) + (body.forward * footOffset), airSmoothing);
            transform.position = airPosition;
        }
    }

    void OnDrawGizmos()
    {        
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(newPosition, 0.05f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(currentPosition, 0.05f);
    }
}
