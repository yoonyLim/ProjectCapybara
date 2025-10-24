using UnityEngine;

public class PlayerJumpSound : MonoBehaviour
{
    [SerializeField] private AudioClip jumpSound;
    private AudioSource jumpAudioSource;
    [SerializeField] private float pitchRandomDeviation = 0.1f;

    private void Awake()
    {
        jumpAudioSource = GetComponent<AudioSource>();
    }

    public void PlayJumpSound()
    {
        float randomPitch = 1f + Random.Range(-pitchRandomDeviation, pitchRandomDeviation);
        jumpAudioSource.pitch = randomPitch;
        jumpAudioSource.PlayOneShot(jumpSound);
    }
}
