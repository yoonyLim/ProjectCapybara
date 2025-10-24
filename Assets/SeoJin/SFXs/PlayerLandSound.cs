using UnityEngine;

public class PlayerLandSound : MonoBehaviour
{
    [SerializeField] private AudioClip landSound;
    private AudioSource landAudioSource;
    [SerializeField] private float pitchRandomDeviation = 0.1f;

    private void Awake()
    {
        landAudioSource = GetComponent<AudioSource>();
    }

    public void PlayFootStepSound()
    {
        float randomPitch = 1f + Random.Range(-pitchRandomDeviation, pitchRandomDeviation);
        landAudioSource.pitch = randomPitch;
        landAudioSource.PlayOneShot(landSound);
    }
}
