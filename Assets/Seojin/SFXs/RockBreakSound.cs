using UnityEngine;

public class RockBreakSound : MonoBehaviour
{
    [SerializeField] private AudioClip rockBreakSound;
    private AudioSource rockBreakSource;
    [SerializeField] private float pitchRandomDeviation = 0.1f;

    private void Awake()
    {
        rockBreakSource = GetComponent<AudioSource>();
    }

    public void PlayRockBreakSound()
    {
        float randomPitch = 1f + Random.Range(-pitchRandomDeviation, pitchRandomDeviation);
        rockBreakSource.pitch = randomPitch;
        rockBreakSource.PlayOneShot(rockBreakSound);
    }
}
