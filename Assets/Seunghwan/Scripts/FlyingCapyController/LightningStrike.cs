using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class LightningStrike : MonoBehaviour
{

    [SerializeField] private AudioClip preSound;
    [SerializeField] private AudioClip strikeSound;
    [SerializeField] private AudioSource audioSource;
    
    private float strikeCooldown = 7f;
    private float dodgeTimeWindow = 0.2f;
    private bool inCooldown = false;
    private float cooldownTimer = 0f;
    private FlyModeController flyModeController;
    [SerializeField] private ParticleSystem lightningEffect;
    private float effectStartTime = 0;
    private float strikeDelayTime = 0.6f;
    private bool checkInput = false;
    private ColorAdjustments colorAdjustments;
    private float defaultPostExposure;
    private void Awake()
    {
        flyModeController = GetComponent<FlyModeController>();
        FindFirstObjectByType<Volume>().profile.TryGet<ColorAdjustments>(out colorAdjustments);
        defaultPostExposure = colorAdjustments.postExposure.value;

        
    }

    private void OnEnable()
    {
        flyModeController.OnWeatherVolumeTriggerEnter += OnCozyWeatherVolumeTriggered;
        flyModeController.OnJumpKeyPressed += OnDodgePressed;
    }

    private void OnDisable()
    {
        flyModeController.OnWeatherVolumeTriggerEnter -= OnCozyWeatherVolumeTriggered;
        flyModeController.OnJumpKeyPressed -= OnDodgePressed;
    }

    private void Update()
    {
        if (cooldownTimer < strikeCooldown)
        {
            cooldownTimer = Mathf.Min(cooldownTimer + Time.deltaTime, strikeCooldown);
        }
        else
        {
            if (flyModeController.GetCurrentState() == FlyModeController.FlyModeState.Normal)
            {
                cooldownTimer = 0f;
                lightningEffect.Play();
                audioSource.pitch = Random.Range(0.8f, 1.2f);
                audioSource.PlayOneShot(preSound);
                StartCoroutine(PlayStrikeSound());
                effectStartTime = Time.time;
                checkInput = true;
                
            }
        }
        
    }

    private void OnCozyWeatherVolumeTriggered()
    {
        this.enabled = false;
    }

    IEnumerator PlayStrikeSound()
    {
        yield return new WaitForSeconds(strikeDelayTime);
        colorAdjustments.postExposure.value = 3f;
        StartCoroutine(RestorePostExposure());
        DualSenseInputManager.Instance.RumbleControllerForDuration(0.6f, 0.15f);
        audioSource.pitch = Random.Range(0.8f, 1.2f);
        audioSource.PlayOneShot(strikeSound);
    }

    IEnumerator RestorePostExposure()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        colorAdjustments.postExposure.value = defaultPostExposure;
    }
    

    private void OnDodgePressed()
    {
        if (!lightningEffect.isPlaying || !checkInput) return;
        
        checkInput = false;
        
        float actualImpactTime = effectStartTime + strikeDelayTime;
        if (Time.time < actualImpactTime && Time.time > actualImpactTime - dodgeTimeWindow)
        {
            flyModeController.DodgedLightning = true;
            
        }
        else
        {
            flyModeController.HitLightning = true;
        }
    }
    
    
    
}
