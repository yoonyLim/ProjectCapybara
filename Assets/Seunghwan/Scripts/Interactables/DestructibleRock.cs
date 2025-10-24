using System;
using Unity.VisualScripting;
using UnityEngine;

public class DestructibleRock : MonoBehaviour, IDestructible
{
    private ParticleSystem rockParticleSystem;
    private Collider rockCollider;
    private Rigidbody rockRigidbody;
    private MeshRenderer[] meshRenderers;

    private RockBreakSound rockBreakSFX;
    
    private bool gotHit = false;

    private void Awake()
    {
        rockParticleSystem = GetComponent<ParticleSystem>();
        rockCollider = GetComponent<Collider>();
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        rockRigidbody = GetComponent<Rigidbody>();
        rockBreakSFX = GetComponent<RockBreakSound>();
    }

    private void Update()
    {
        if (!rockParticleSystem.isPlaying && gotHit)
        {
            Destroy(gameObject);
        }
    }

    public void Hit()
    {
        gotHit = true;
        
        rockRigidbody.isKinematic = true;
        rockParticleSystem.Play();
        rockCollider.enabled = false;
        rockBreakSFX.PlayRockBreakSound();
        foreach (var meshRenderer in meshRenderers)
        {
            meshRenderer.enabled = false;
        }
    }

    
}
