using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider))]
public class ElementCollider : MonoBehaviour
{
    [SerializeField] private float m_TrampolineForce = 7.5f;
    [SerializeField] private Vector3 _respawnCoordinates;
    [SerializeField] private GameObject m_CoinSound;
    [SerializeField] private GameObject m_JumpSound;
    [SerializeField] private GameObject m_OuchSound;
    [SerializeField] private GameObject m_CheckpointSound;
    [SerializeField] private GameObject m_CoinExplosion;

    private bool m_Died = false;
    private bool m_SpeedChanged = false;
    private bool m_ApplyingTrampolineForce = false;
    private float m_OriginalMaxSpeed;

    void Start()
    {
        m_OriginalMaxSpeed = GetComponentInParent<PlayerController>().MaxSpeed;
        GameplayManager.Instance.Respawn.SetRespawnPoint(transform.position);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown("joystick button 6"))
        {
            Instantiate(m_OuchSound);

            GameplayManager.Instance.Respawn.Respawn(transform.parent);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (GameplayManager.Instance.BookOpen)
            return;

        // Ouch Layer collider
        if (other.gameObject.layer == 6 && !m_Died)
        {
            // m_Died = true;
            Instantiate(m_OuchSound);

            GameplayManager.Instance.Respawn.Respawn(transform.parent);
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

            Instantiate(m_JumpSound, transform.position, Quaternion.identity);

            float force = other.gameObject.name == "SuperTramp" ? m_TrampolineForce * 3f : m_TrampolineForce;
            
            parentRigidbody.AddForce(Vector3.up * force, ForceMode.Impulse);
            GetComponentInParent<PlayerController>().DoubleJumped = false;
        }

        // Collectible layer collider
        if (other.gameObject.layer == 9)
        {
            Debug.Log("Increase Collectible Score!");
            Instantiate(m_CoinSound, other.transform.position, Quaternion.identity);
            Instantiate(m_CoinExplosion, other.transform.position, Quaternion.identity);
            GameplayManager.Instance.CollectCoin();
            Destroy(other.gameObject);
        }

        // Checkpoint layer collider
        if (other.gameObject.layer == 10)
        {
            Debug.Log("Checkpoint!");
            GameplayManager.Instance.Respawn.SetRespawnPoint(
               other.transform.position);
            Instantiate(m_CheckpointSound, transform.position, Quaternion.identity);
            other.GetComponent<Animation>().Play();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (GameplayManager.Instance.BookOpen)
            return;
        
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
