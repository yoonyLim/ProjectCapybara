using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance;

    [SerializeField] private EnemyProjectile projectilePrefab;
    private IObjectPool<EnemyProjectile> projectilePool;

    /// <summary>
    /// Public function wrapper for getting a projectile from projectile pool.
    /// </summary>
    /// <returns>The projectile from projectile pool.</returns>
    public EnemyProjectile GetProjectile()
    {
        return projectilePool.Get();
    }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        projectilePool = new ObjectPool<EnemyProjectile>(CreateProjectile, OnProjectileGet,
            OnProjectileRelease, OnProjectileDestroy);
    }

    private EnemyProjectile CreateProjectile()
    {
        EnemyProjectile projectile = Instantiate(projectilePrefab);
        projectile.SetProjectilePool(projectilePool);
        return projectile;
    }

    private void OnProjectileGet(EnemyProjectile projectile)
    {
        projectile.gameObject.SetActive(true);
    }

    private void OnProjectileRelease(EnemyProjectile projectile)
    {
        projectile.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        projectile.gameObject.SetActive(false);
    }

    private void OnProjectileDestroy(EnemyProjectile projectile)
    {
        Destroy(projectile.gameObject);
    }
}
