using System;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.VFX;

public class DustTrailSpawner : MonoBehaviour
{
    [SerializeField] private VisualEffect dustPrefab;
    private IObjectPool<VisualEffect> dustPool;
    [SerializeField] private Transform spawnPoint;

    private void Awake()
    {
        dustPool = new ObjectPool<VisualEffect>(CreateDust, OnDustGet,
            OnDustRelease, OnDustDestroy, true, 20, 30);
    }

    private VisualEffect CreateDust()
    {
        VisualEffect instance = Instantiate(dustPrefab, spawnPoint.position, spawnPoint.rotation);
        return instance;
    }

    private void OnDustGet(VisualEffect dust)
    {
        dust.transform.position = spawnPoint.position;
        dust.transform.rotation = spawnPoint.rotation;
        dust.gameObject.SetActive(true);
        dust.Play();
        dust.GetComponent<DustTrail>().Pool = dustPool;
    }

    private void OnDustRelease(VisualEffect dust)
    {
        dust.gameObject.SetActive(false);
    }

    private void OnDustDestroy(VisualEffect dust)
    {
        Destroy(dust.gameObject);
    }

    public void SpawnDust()
    {
        dustPool.Get();
    }
    
    
}
