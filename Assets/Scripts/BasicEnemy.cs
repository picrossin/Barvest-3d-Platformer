using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BasicEnemy : MonoBehaviour
{
    private NavMeshAgent m_Enemy;
    [SerializeField] private Transform m_Target;
    [SerializeField] private Vector3[] M_corners;

    // Start is called before the first frame update
    void Start()
    {
        m_Enemy = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        m_Enemy.SetDestination(m_Target.position);
    }
}
