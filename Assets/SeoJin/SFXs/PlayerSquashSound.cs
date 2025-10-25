using UnityEngine;

public class PlayerSquashSound : MonoBehaviour
{
    [SerializeField] private AudioClip squashSound;
    public AudioSource squashAudioSource;
    [SerializeField] private float pitchRandomDeviation = 0.1f;

    private void Awake()
    {
        squashAudioSource = GetComponent<AudioSource>();
    }

    public void PlaySquashSound()
    {
        float randomPitch = 1f + Random.Range(-pitchRandomDeviation, pitchRandomDeviation);
        squashAudioSource.pitch = randomPitch;
        squashAudioSource.PlayOneShot(squashSound);
    }
}
