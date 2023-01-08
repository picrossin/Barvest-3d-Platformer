using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboarding : MonoBehaviour
{
    [SerializeField] private float m_Speed = 5f;
    [SerializeField] private float m_Height = 0.05f;
    [SerializeField] private bool m_MoveUpAndDown = true;

    private Vector3 m_InitialPosition;

    private void Start()
    {
        m_InitialPosition = transform.position;
    }

    void Update()
    {
        // Every frame, make sure that whatever this script is attached to is pointed towards camera
        Vector3 cameraDirection = Camera.main.transform.forward;
        cameraDirection.y = 0;
        transform.rotation = Quaternion.LookRotation(cameraDirection);

        if (m_MoveUpAndDown)
        {
            // Move the billboarding object up and down slightly
            float newY = Mathf.Sin(Time.time * m_Speed) * m_Height + m_InitialPosition.y;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }
}
