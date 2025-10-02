using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class DestructibleTree : MonoBehaviour, IDestructible
{
    private int requiredHitCount = 3;
    [SerializeField] private int currentHitCount = 0;
    [SerializeField] private bool isDestructible = true;
    
    private Vector3 startPosition;

    private Animator animator;
    
    private float shakeDuration = 0.2f;
    private float shakeMagnitude = 0.1f;

    private void Awake()
    {
        startPosition = transform.position;
        animator = GetComponent<Animator>();
    }


    public void Hit()
    {
        if (!isDestructible) return;

        currentHitCount++;
        if (currentHitCount >= requiredHitCount)
        {
            Destruct();
        }
        else
        {
            StartCoroutine(ShakeCoroutine());
        }
    }

    private void Destruct()
    {
        isDestructible = false;
        currentHitCount = 0;
        
        animator.Play("TreeFall");

    }

    private IEnumerator ShakeCoroutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            elapsedTime += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(elapsedTime / shakeDuration);
            float xNoise = Random.Range(-1, 1) * shakeMagnitude;
            float zNoise = Random.Range(-1, 1) * shakeMagnitude;
            transform.localPosition = startPosition + new Vector3(xNoise, 0f, zNoise);
            yield return new WaitForFixedUpdate();
        }

        transform.localPosition = startPosition;
    }
}