using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class SpawnPointManager : MonoBehaviour
{
    public static event Action OnPlayerRespawned;
    
    [SerializeField] private float screenFadeDuration = 0.75f;
    [SerializeField] private float completeBlackDelay = 0.35f;
    [SerializeField] private List<AudioClip> fallSounds;
    
    private Image blackImage;
    private AudioSource audioSource;
    
    private Color transparentColor = new Color(0, 0, 0, 0);
    private Color blackColor = new Color(0, 0, 0, 1);
    
    private bool isRunning = false;

    [SerializeField] private GameObject playerGO;

    private Vector3 currentSpawnPoint;
    
    
    public void UpdateCurrentSpawnPoint(Vector3 position)
    {
        currentSpawnPoint = position;
    }
    
    private void Awake()
    {
        //playerGO = GameObject.FindGameObjectWithTag("Player");
        audioSource = GetComponent<AudioSource>();
        blackImage = GetComponentInChildren<Image>();
    }

    private void OnEnable()
    {
        FallTriggerBox.OnPlayerFallTriggerEnter += StartRespawn;
        SpawnPointDetector.OnSpawnPointTriggerEnter += UpdateCurrentSpawnPoint;
    }

    private void OnDisable()
    {
        FallTriggerBox.OnPlayerFallTriggerEnter -= StartRespawn;
        SpawnPointDetector.OnSpawnPointTriggerEnter -= UpdateCurrentSpawnPoint;
    }

    private void StartRespawn()
    {
        if (isRunning) return;
        isRunning = true;
        DualSenseInputManager.Instance.RumbleControllerForDuration(1f, 0.2f);
        PlayFallSound();
        StartCoroutine(FadeAndMovePlayerToSpawnPoint());
    }

    private IEnumerator FadeAndMovePlayerToSpawnPoint()
    {
        blackImage.color = transparentColor;
        blackImage.enabled = true;
        
        float fadeOutTime = 0;
        while (fadeOutTime < screenFadeDuration)
        {
            fadeOutTime += Time.deltaTime;
            blackImage.color = Color.Lerp(transparentColor, blackColor, fadeOutTime / screenFadeDuration);
            yield return null;
        }
        blackImage.color = blackColor;

        // Moves the player to the spawn point
        playerGO.transform.position = currentSpawnPoint;
        playerGO.GetComponentInChildren<Rigidbody>().isKinematic = true;
        OnPlayerRespawned?.Invoke();
        
        yield return new WaitForSeconds(completeBlackDelay);
        
        playerGO.GetComponentInChildren<Rigidbody>().isKinematic = false;
        
        float fadeInTime = 0;
        while (fadeInTime < screenFadeDuration)
        {
            fadeInTime += Time.deltaTime;
            blackImage.color = Color.Lerp(blackColor, transparentColor, fadeInTime / screenFadeDuration);
            yield return null;
        }
        
        blackImage.color = transparentColor;
        blackImage.enabled = false;

        isRunning = false;
    }

    private void PlayFallSound()
    {
        if (fallSounds.Count == 0) return;
        
        audioSource.PlayOneShot(fallSounds[Random.Range(0, fallSounds.Count)]);
    }
}
