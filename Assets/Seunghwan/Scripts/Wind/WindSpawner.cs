using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

public enum WindType
{
    Normal,
    Snowy
}

[Serializable]
public class WindInfo
{
    public WindType Type;
    public ParticleSystem Prefab;
}

public class WindSpawner : MonoBehaviour
{
    [SerializeField]
    private WindType currentWindType;
    
    [SerializeField]
    private List<WindInfo> winds;

    private Dictionary<WindType, IObjectPool<ParticleSystem>> windPools;

    private IObjectPool<ParticleSystem> normalWindPool;
    private IObjectPool<ParticleSystem> snowyWindPool;

    [SerializeField] private float spawnInterval = 5f;

    private bool shouldSpawn = true;
    
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
        windPools = new Dictionary<WindType, IObjectPool<ParticleSystem>>();
        
        foreach (WindInfo wind in winds)
        {
            if (!windPools.ContainsKey(wind.Type))
            {
                windPools[wind.Type] = CreateWindPool(wind.Prefab);
            }
        }
    }

    private void Start()
    {
        StartCoroutine(SpawnCoroutine());
    }

    private IObjectPool<ParticleSystem> CreateWindPool(ParticleSystem prefab)
    {
        IObjectPool<ParticleSystem> pool = null;

        pool = new ObjectPool<ParticleSystem>(
            () =>
            {
                ParticleSystem ps = Instantiate(prefab, transform);
                ps.GetComponent<ReturnToPool>().SetPool(pool);
                return ps;
            },
            (ps) => ps.gameObject.SetActive(true),
            (ps) => ps.gameObject.SetActive(false),
            (ps) => Destroy(ps.gameObject),
            true,
            10,
            20
        );
        return pool;
    }

    private IEnumerator SpawnCoroutine()
    {
        while (shouldSpawn)
        {
            if (windPools.TryGetValue(currentWindType, out IObjectPool<ParticleSystem> pool))
            {
                ParticleSystem ps = pool.Get();
                float forwardOffset = Random.Range(35, 40f);
                float rightOffset = Random.Range(15, 20);
                float upOffset = Random.Range(-7f, 7f);
                ps.transform.position = mainCamera.transform.position +
                                        mainCamera.transform.forward * forwardOffset +
                                        mainCamera.transform.right * rightOffset +
                                        mainCamera.transform.up * upOffset;
                Quaternion lookRotation =
                    Quaternion.LookRotation(-mainCamera.transform.forward, mainCamera.transform.up);
                lookRotation.eulerAngles += new Vector3(0, 0, Random.Range(-10, 10));
                ps.transform.rotation = lookRotation;
                ps.Play();
            }
            
            yield return new WaitForSeconds(spawnInterval);
        }

    }
    
    
    
    

    
}
