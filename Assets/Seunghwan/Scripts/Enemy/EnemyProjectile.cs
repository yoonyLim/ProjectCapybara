
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using UnityEngine.Pool;

public class EnemyProjectile : MonoBehaviour
{
    private ParticleSystem[] hitParticleSystems;
    private IObjectPool<EnemyProjectile> pool;
    [SerializeField] private ParticleSystem[] projectileParticleSystems;
    [SerializeField] private Transform hitEffect;
    [SerializeField] private float projectileSpeed = 3f;
    [SerializeField] private float projectileLifeTime = 2f;
    [SerializeField] private float projectileForceAmount = 5f;
    private Rigidbody projectileRigidbody;
    private Collider projectileCollider;
    private float spawnTime;

    // Boolean value to check if the object is released to the pool
    private bool isReleased;

    private void Awake()
    {
        projectileCollider = GetComponent<Collider>();
        projectileRigidbody = GetComponent<Rigidbody>();
        projectileRigidbody.useGravity = false;

        hitParticleSystems = new ParticleSystem[hitEffect.childCount];
        for (int i = 0; i < hitEffect.childCount; i++)
        {
            hitParticleSystems[i] = hitEffect.GetChild(i).GetComponent<ParticleSystem>();
        }
    }

    private void OnEnable()
    {
        foreach (var projectileParticleSystem in projectileParticleSystems)
        {
            projectileParticleSystem.Play(true);
        }
        GetComponent<Collider>().enabled = true;
        spawnTime = Time.time;
        isReleased = false;
    }
    

    private void Update()
    {
        if (Time.time - spawnTime >= projectileLifeTime)
        {
            if (!isReleased)
            {
                isReleased = true;
                pool.Release(this);
            }
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        GetComponent<Collider>().enabled = false;
        foreach (var projectileParticleSystem in projectileParticleSystems)
        {
            projectileParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        
        if (other.gameObject.CompareTag("Player"))
        {
            Vector3 forceDir = Vector3.Normalize(other.transform.position - transform.position);
            other.gameObject.GetComponent<Rigidbody>().AddForce(forceDir * projectileForceAmount, ForceMode.Impulse);
        }

        if (!isReleased)
        {
            isReleased = true;
            PlayHitParticles();
            Invoke(nameof(ReleaseToPool), hitParticleSystems[0].main.duration);
        }
    }

    private void ReleaseToPool()
    {
        pool.Release(this);
    }

    void PlayHitParticles()
    {
        foreach (var hitParticleSystem in hitParticleSystems)
        {
            hitParticleSystem.Play();
        } 
    }

    /// <summary>
    /// Setter for projectile pool. It is used by ProjectilePool class in order to release this projectile instance back
    /// to the correct pool.
    /// </summary>
    /// <param name="inPool">The object pool to set for this projectile's pool reference</param>
    public void SetProjectilePool(IObjectPool<EnemyProjectile> inPool)
    {
        pool = inPool;
    }

    /// <summary>
    /// Initializes the projectile's position, rotation, velocity.
    /// </summary>
    /// <param name="position">Position to set</param>
    /// <param name="rotation">Rotation to set</param>
    public void Initialize(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
        projectileRigidbody.linearVelocity = transform.forward * projectileSpeed;
    }
}
