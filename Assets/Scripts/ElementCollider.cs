using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider))]
public class ElementCollider : MonoBehaviour
{
    [SerializeField] private float m_TrampolineForce = 7.5f;

    private bool m_Died = false;
    private bool m_SpeedChanged = false;
    private bool m_ApplyingTrampolineForce = false;
    private float m_OriginalMaxSpeed;

    void Start()
    {
        m_OriginalMaxSpeed = GetComponentInParent<PlayerController>().MaxSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ouch Layer collider
        if (other.gameObject.layer == 6 && !m_Died)
        {
            m_Died = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        // Puddle Layer collider
        if (other.gameObject.layer == 4 && !m_SpeedChanged)
        {
            Debug.Log("Halve Speed!");
            m_SpeedChanged = true;
            GetComponentInParent<PlayerController>().MaxSpeed = m_OriginalMaxSpeed / 2;
        }

        // Cobweb Layer collider
        if (other.gameObject.layer == 7 && !m_SpeedChanged)
        {
            Debug.Log("Double Speed!");
            m_SpeedChanged = true;
            GetComponentInParent<PlayerController>().MaxSpeed = m_OriginalMaxSpeed * 2;
        }

        // Trampoline layer collider
        if (other.gameObject.layer == 8 && !m_ApplyingTrampolineForce)
        {
            m_ApplyingTrampolineForce = true;
            Rigidbody parentRigidbody = GetComponentInParent<Rigidbody>();
            parentRigidbody.AddForce(Vector3.up * m_TrampolineForce, ForceMode.Impulse);
            GetComponentInParent<PlayerController>().DoubleJumped = false;
        }

        // Collectible layer collider
        if (other.gameObject.layer == 9)
        {
            Debug.Log("Increase Collectible Score!");
            Destroy(other.gameObject);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 7 || other.gameObject.layer == 4)
        {
            Debug.Log("Reset Speed!");
            m_SpeedChanged = false;
            GetComponentInParent<PlayerController>().MaxSpeed = m_OriginalMaxSpeed;
        }

        if (other.gameObject.layer == 8)
        {
            m_ApplyingTrampolineForce = false;
        }
    }
}
