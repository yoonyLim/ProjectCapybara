using System;
using System.Collections;
using UnityEngine;

public class TimedPlatform : MonoBehaviour
{
    enum PlatformState
    {
        Default,
        Triggered,
        Disappeared
    }
    
    private Collider platformCollider;
    private MeshRenderer platformMeshRenderer;
    private Collider upperSurfaceCollider;

    private ParticleSystem platformParticleSystem;
    
    private PlatformState currentState = PlatformState.Default;

    [SerializeField]
    private float disappearDelay = 2f;
    
    [SerializeField]
    private float respawnDelay = 2f;

    private void Awake()
    {
        platformParticleSystem = GetComponent<ParticleSystem>();
        platformMeshRenderer = GetComponent<MeshRenderer>();
        platformCollider = GetComponent<Collider>();
        upperSurfaceCollider = transform.GetChild(0).GetComponent<Collider>();
    }

    private void SetState(PlatformState newState)
    {
        currentState = newState;
    }

    public void OnUpperSurfaceEnter(Collider other)
    {
        SetState(PlatformState.Triggered);
        StartCoroutine(DisappearCoroutine());
    }

    private IEnumerator DisappearCoroutine()
    {
        yield return new WaitForSeconds(disappearDelay);
        SetState(PlatformState.Disappeared);
        platformMeshRenderer.enabled = false;
        platformCollider.enabled = false;
        upperSurfaceCollider.enabled = false;
        platformParticleSystem.Play();
        
        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        SetState(PlatformState.Default);
        platformMeshRenderer.enabled = true;
        platformCollider.enabled = true;
        upperSurfaceCollider.enabled = true;
    }
    
    
}
