using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private AudioClip m_LevelMusic;

    private AudioSource m_AudioSource;

    private void Start()
    {
        m_AudioSource = GetComponent<AudioSource>();
    }

    public void StartLevelMusic()
    {
        m_AudioSource.clip = m_LevelMusic;
        m_AudioSource.Play();
    }
}
