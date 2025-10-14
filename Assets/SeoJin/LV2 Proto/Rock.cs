using System;
using FlatKit;
using UnityEngine;

public class Rock : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float maxSpeed = 30f;
    [SerializeField] private float pushForce = 100f;
    [SerializeField] private float autoDestroyTime = 50f;
    
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        rb.maxLinearVelocity = maxSpeed;
        Destroy(gameObject, autoDestroyTime);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("RockDestroyer"))
        {
            Debug.Log("try to destory rock");
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject player = collision.gameObject;
        
        if (player.CompareTag("Player"))
        {
            Vector3 meanNormal = Vector3.zero;

            foreach (var contact in collision.contacts)
            {
                meanNormal += contact.normal;
            }
            
            meanNormal = (meanNormal / collision.contactCount).normalized;
            player.GetComponent<Rigidbody>().AddForce(meanNormal * pushForce, ForceMode.Impulse);
        }
    }
}
