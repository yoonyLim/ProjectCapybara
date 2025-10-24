using UnityEngine;

public class PlayerFootStepSound : MonoBehaviour
{
    [SerializeField] private AudioClip footStepSound;
    private AudioSource footStepAudioSource;
    [SerializeField] private float pitchRandomDeviation = 0.1f;

    private void Awake()
    {
        footStepAudioSource = GetComponent<AudioSource>();
    }

    public void PlayFootStepSound()
    {
        float randomPitch = 1f + Random.Range(-pitchRandomDeviation, pitchRandomDeviation);
        footStepAudioSource.pitch = randomPitch;
        footStepAudioSource.PlayOneShot(footStepSound);
    }
}
