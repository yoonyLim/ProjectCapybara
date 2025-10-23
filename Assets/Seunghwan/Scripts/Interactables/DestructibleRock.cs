using System;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;

public class DestructibleRock : MonoBehaviour, IDestructible
{
    private ParticleSystem rockParticleSystem;
    private Collider rockCollider;
    private Rigidbody rockRigidbody;
    private MeshRenderer rockMeshRenderer;

    private void Awake()
    {
        rockParticleSystem = GetComponent<ParticleSystem>();
        rockCollider = GetComponent<Collider>();
        rockMeshRenderer = GetComponent<MeshRenderer>();
        rockRigidbody = GetComponent<Rigidbody>();
    }

    public void Hit()
    {
        
        Debug.Log("Hit");
        rockRigidbody.isKinematic = true;
        rockParticleSystem.Play();
        rockCollider.enabled = false;
        rockMeshRenderer.enabled = false;
    }

    
}
