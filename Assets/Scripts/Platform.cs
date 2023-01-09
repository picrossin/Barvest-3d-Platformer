using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    [SerializeField] private List<Vector3> m_Waypoints;
    [SerializeField] private float m_Speed;

    // Variables for time passed
    private float m_ElapsedTime;
    private float m_TimeToNextPosition;

    // Variables for setting previous and next positions
    private Vector3 m_PreviousPosition;
    private int m_NextPositionIndex;
    private Vector3 m_NextPosition;

    public void AddPositionToPath()
    {
        m_Waypoints.Add(transform.position);
    }

    // Start is called before the first frame update
    void Start()
    {
        SetNextPosition();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (GameplayManager.Instance.BookOpen)
            return;
        
        m_ElapsedTime += Time.deltaTime;

        float elapsedPercentage = m_ElapsedTime / m_TimeToNextPosition;
        elapsedPercentage = Mathf.SmoothStep(0, 1, elapsedPercentage);
        transform.position = Vector3.Lerp(m_PreviousPosition, m_NextPosition, elapsedPercentage);

        if (elapsedPercentage >= 1)
        {
            SetNextPosition();
        }
    }

    private void SetNextPosition()
    {
        m_PreviousPosition = m_Waypoints[m_NextPositionIndex];
        m_NextPositionIndex = (m_NextPositionIndex + 1) % m_Waypoints.Count;
        m_NextPosition = m_Waypoints[m_NextPositionIndex];

        m_ElapsedTime = 0;

        float distanceToWaypoint = Vector3.Distance(m_PreviousPosition, m_NextPosition);
        m_TimeToNextPosition = distanceToWaypoint / m_Speed;
    }
}
