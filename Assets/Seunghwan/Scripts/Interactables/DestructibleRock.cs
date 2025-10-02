using System;
using UnityEngine;

public class DestructibleRock : MonoBehaviour, IDestructible
{
    private ParticleSystem rockParticleSystem;
    private Collider rockCollider;
    private MeshRenderer rockMeshRenderer;
    

    private void Awake()
    {
        rockParticleSystem = GetComponent<ParticleSystem>();
        rockCollider = GetComponent<Collider>();
        rockMeshRenderer = GetComponent<MeshRenderer>();
    }

    public void Hit()
    {
        rockCollider.enabled = false;
        rockMeshRenderer.enabled = false;
        rockParticleSystem.Play();
        transform.GetChild(0).gameObject.SetActive(false);
    }

    
}
