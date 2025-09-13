using System;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float projectileSpeed = 3f;
    [SerializeField] private float projectileLifeTime = 2f;
    [SerializeField] private float projectileForceAmount = 5f;
    private Rigidbody rigidBody;
    private float spawnTime;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.useGravity = false;
        rigidBody.linearVelocity = transform.forward * projectileSpeed;
    }

    private void Start()
    {
        spawnTime = Time.time;
    }

    private void Update()
    {
        if (Time.time - spawnTime >= projectileLifeTime)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Vector3 forceDir = Vector3.Normalize(other.transform.position - transform.position);
            other.gameObject.GetComponent<Rigidbody>().AddForce(forceDir * projectileForceAmount, ForceMode.Impulse);
        }

        Destroy(gameObject);
    }
}
