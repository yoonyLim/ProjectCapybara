using System;
using UnityEngine;

public class PianoKey : MonoBehaviour
{
    [SerializeField] private float floatingSpeed = 5f;
    [SerializeField] private float floatingAmount = 0.3f;
    [SerializeField] private float rotateSpeed = 20f;

    [SerializeField] private ParticleSystem collectEffect;
    [SerializeField] private ParticleSystem groundEffect;
    
    private Vector3 initialPosition;
    private Transform keyTransform;

    private bool collected = false;

    private void Awake()
    {
        keyTransform = transform.GetChild(0);
        initialPosition = keyTransform.position;
    }

    private void Update()
    {
        if (keyTransform.gameObject.activeInHierarchy)
        {
            float offsetY = Mathf.Sin(Time.time * floatingSpeed) * floatingAmount;
            keyTransform.position = new Vector3(initialPosition.x, initialPosition.y + offsetY, initialPosition.z);
            keyTransform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !collected)
        {
            collected = true;
            other.GetComponent<PianoTracker>().OnKeyCollected();
            keyTransform.gameObject.SetActive(false);
            groundEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            collectEffect.Play();
            Destroy(gameObject, 3f);
        }
    }
}
