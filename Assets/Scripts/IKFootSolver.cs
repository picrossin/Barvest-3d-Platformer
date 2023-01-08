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

    Vector3 newPosition;
    Vector3 currentPosition;
    Vector3 oldPosition;
    float lerp;

    // Start is called before the first frame update
    void Start()
    {
        newPosition = transform.position;
        currentPosition = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = currentPosition;

        Ray ray = new Ray(body.position + (body.right * footSpacing) + (body.forward * footOffset) + (body.up * 0.05f), Vector3.down);
        Debug.DrawRay(ray.origin, ray.direction * 10, Color.red);
        
        if (Physics.Raycast(ray, out RaycastHit info, 10, terrainLayer))
        {
            Debug.Log("hit ground");
            if (Vector3.Distance(newPosition, info.point) > stepDistance)
            {
                Debug.Log($"found new position, distance:{Vector3.Distance(newPosition, info.point)}");
                lerp = 0;
                newPosition = info.point;
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
            oldPosition = newPosition;
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
