using System;
using UnityEngine;

public class PianoKey : MonoBehaviour
{
    [SerializeField] private float floatingSpeed = 5f;
    [SerializeField] private float floatingAmount = 0.3f;
    [SerializeField] private float rotateSpeed = 20f;
    
    private Vector3 initialPosition;
    private Transform keyTransform;

    private void Awake()
    {
        keyTransform = transform.GetChild(0);
        initialPosition = keyTransform.position;
    }

    private void Update()
    {
        float offsetY = Mathf.Sin(Time.time * floatingSpeed) * floatingAmount;
        keyTransform.position = new Vector3(initialPosition.x, initialPosition.y + offsetY, initialPosition.z);
        keyTransform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PianoTracker>().OnKeyCollected();
            Destroy(gameObject);
        }
    }
}
