using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollectGlowingOrb : MonoBehaviour
{
    [SerializeField] private List<ParticleSystem> orbParticles;
    [SerializeField] private float decreaseSpeed = 5;
    [SerializeField] private GameObject puffEffect;
    
    private bool shouldBeDestroyed = false;
    private GameObject puffEffectInstance;

    public UnityAction OnCollected;
        
    protected void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnCollected?.Invoke();
            shouldBeDestroyed = true;
            puffEffectInstance = Instantiate(puffEffect, transform.position, Quaternion.identity);
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
