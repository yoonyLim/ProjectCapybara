
using UnityEngine;
using UnityEngine.Pool;

public class EnemyProjectile : MonoBehaviour
{
    private IObjectPool<EnemyProjectile> pool;
    [SerializeField] private float projectileSpeed = 3f;
    [SerializeField] private float projectileLifeTime = 2f;
    [SerializeField] private float projectileForceAmount = 5f;
    private Rigidbody rigidBody;
    private float spawnTime;

    // Boolean value to check if the object is released to the pool
    private bool isReleased;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.useGravity = false;
    }

    private void OnEnable()
    {
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
        if (other.gameObject.CompareTag("Player"))
        {
            Vector3 forceDir = Vector3.Normalize(other.transform.position - transform.position);
            other.gameObject.GetComponent<Rigidbody>().AddForce(forceDir * projectileForceAmount, ForceMode.Impulse);
        }

        if (!isReleased)
        {
            isReleased = true;
            pool.Release(this);
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
        rigidBody.linearVelocity = transform.forward * projectileSpeed;
    }
}
