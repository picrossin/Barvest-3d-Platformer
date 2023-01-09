using UnityEngine;

public class VolumeSetter : MonoBehaviour
{
    private float m_OriginalVolume;
    private AudioSource m_AudioSource;
    
    private void Start()
    {
        m_AudioSource = GetComponent<AudioSource>();
        m_OriginalVolume = m_AudioSource.volume;
    }

    private void Update()
    {
        m_AudioSource.volume = m_OriginalVolume * GameplayManager.Instance.Volume;
    }
}
