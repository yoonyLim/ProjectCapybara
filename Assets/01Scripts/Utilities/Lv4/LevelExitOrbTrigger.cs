using System.Collections.Generic;
using Capybara;
using UnityEngine;
using UnityEngine.Events;

public class LevelExitOrbTrigger : MonoBehaviour
{
    [SerializeField] private List<ParticleSystem> orbParticles;
    [SerializeField] private float decreaseSpeed = 5;
    [SerializeField] private GameObject puffEffect;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip enterSound;
    
    private bool shouldBeDestroyed = false;
    private GameObject puffEffectInstance;
        
    protected void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DualSenseInputManager.Instance.RumbleControllerShort(1);
            
            audioSource.clip = enterSound;
            audioSource.Play();
            
            shouldBeDestroyed = true;
            puffEffectInstance = Instantiate(puffEffect, transform.position, Quaternion.identity);
            
            other.GetComponent<CapybaraControllerSnow>().LevelCompleted();
        }
    }

    protected virtual void Update()
    {
        if (shouldBeDestroyed)
        {
            foreach (var particle in orbParticles)
            {
                var mainParticleSystem = particle.main;
                mainParticleSystem.startSize = CapyHelperLibrary.FInterpTo(mainParticleSystem.startSize.constant, 0, Time.deltaTime, decreaseSpeed);

                if (mainParticleSystem.startSize.constant <= 0)
                {
                    Destroy(puffEffectInstance);
                    Destroy(gameObject);
                }
            }
        }
    }
}
