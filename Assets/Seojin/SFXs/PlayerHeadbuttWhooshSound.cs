using UnityEngine;

public class PlayerHeadbuttWhooshSound : MonoBehaviour
{
    [SerializeField] private AudioClip headbuttWhooshSound;
    private AudioSource headbuttWhooshSource;
    [SerializeField] private float pitchRandomDeviation = 0.1f;

    private void Awake()
    {
        headbuttWhooshSource = GetComponent<AudioSource>();
    }

    public void PlayHeadbuttWhooshSound()
    {
        float randomPitch = 1f + Random.Range(-pitchRandomDeviation, pitchRandomDeviation);
        headbuttWhooshSource.pitch = randomPitch;
        headbuttWhooshSource.PlayOneShot(headbuttWhooshSound);
    }
}
