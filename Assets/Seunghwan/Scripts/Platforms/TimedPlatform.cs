using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

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
    private Transform meshTransform;

    private ParticleSystem platformParticleSystem;
    
    private PlatformState currentState = PlatformState.Default;
    
    [SerializeField]
    private float shakeMaxMagnitude = 0.1f;

    [SerializeField]
    private float disappearDelay = 2f;
    [SerializeField]
    private float respawnDelay = 2f;
    
    private Vector3 initialMeshPosition;

    private void Awake()
    {
        platformParticleSystem = GetComponent<ParticleSystem>();
        platformMeshRenderer = GetComponentInChildren<MeshRenderer>();
        platformCollider = GetComponent<Collider>();
        upperSurfaceCollider = transform.GetChild(0).GetComponent<Collider>();
        meshTransform = transform.GetChild(1);
        
        initialMeshPosition = meshTransform.localPosition;
    }

    private void SetState(PlatformState newState)
    {
        currentState = newState;
    }

    // 자식 오브젝트인 upper surface의 OnTriggerEnter가 호출됐을 때 이 함수가 불려요. 플레이어가 발판 위에 착지했을 때에요.
    public void OnUpperSurfaceEnter(Collider other)
    {
        SetState(PlatformState.Triggered);
        StartCoroutine(ShakeCoroutine());
    }

    // disappearDealy 시간동안 플랫폼을 진동시켜요.
    private IEnumerator ShakeCoroutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < disappearDelay)
        {
            elapsedTime += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(elapsedTime / disappearDelay);
            float magnitude = Mathf.Lerp(0f, shakeMaxMagnitude, t);
            float xNoise = Random.Range(-1, 1) * magnitude;
            float zNoise = Random.Range(-1, 1) * magnitude;
            meshTransform.localPosition = initialMeshPosition + new Vector3(xNoise, 0f, zNoise);
            yield return new WaitForFixedUpdate();
        }
        
        Disappear();
    }

    // 플랫폼을 숨기고 비활성화해요.
    private void Disappear()
    {
        SetState(PlatformState.Disappeared);
        platformMeshRenderer.enabled = false;
        platformCollider.enabled = false;
        upperSurfaceCollider.enabled = false;
        meshTransform.localPosition = initialMeshPosition;
        platformParticleSystem.Play();
        
        StartCoroutine(RespawnCoroutine());
    }

    // respawnDelay 시간 이후 플랫폼을 다시 활성화해요.
    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        SetState(PlatformState.Default);
        platformMeshRenderer.enabled = true;
        platformCollider.enabled = true;
        upperSurfaceCollider.enabled = true;
    }
    
    
}
