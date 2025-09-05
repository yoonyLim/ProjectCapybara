using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.VFX;

public class DustSpawner : MonoBehaviour
{
    [SerializeField] private VisualEffect dustTrailPrefab;
    [SerializeField] private VisualEffect dustLandPrefab;
    private IObjectPool<VisualEffect> dustTrailPool;
    private IObjectPool<VisualEffect> dustLandPool;
    [SerializeField] private Transform spawnPoint;

    private void Awake()
    {
        dustTrailPool = new ObjectPool<VisualEffect>(CreateDustTrail, OnDustTrailGet,
            OnDustTrailRelease, OnDustTrailDestroy, true, 10, 20);
        
        dustLandPool = new ObjectPool<VisualEffect>(CreateDustLand, OnDustLandGet,
            OnDustLandRelease, OnDustLandDestroy, true, 3, 5);
    }
    
    #region Dust Trail
    private VisualEffect CreateDustTrail()
    {
        VisualEffect instance = Instantiate(dustTrailPrefab, spawnPoint.position, spawnPoint.rotation);
        return instance;
    }

    private void OnDustTrailGet(VisualEffect dust)
    {
        dust.transform.position = spawnPoint.position;
        dust.transform.rotation = spawnPoint.rotation;
        dust.gameObject.SetActive(true);
        dust.Play();
        dust.GetComponent<DustTrail>().Pool = dustTrailPool;
    }

    private void OnDustTrailRelease(VisualEffect dust)
    {
        dust.gameObject.SetActive(false);
    }

    private void OnDustTrailDestroy(VisualEffect dust)
    {
        Destroy(dust.gameObject);
    }

    public void SpawnDustTrail()
    {
        dustTrailPool.Get();
    }
    
    #endregion Dust Trail

    #region Dust Land    
    private VisualEffect CreateDustLand()
    {
        VisualEffect instance = Instantiate(dustLandPrefab);
        return instance;
    }

    private void OnDustLandGet(VisualEffect dust)
    {
        dust.transform.position = transform.position;
        dust.gameObject.SetActive(true);
        dust.Play();
        dust.GetComponent<DustLand>().Pool = dustLandPool;
    }

    private void OnDustLandRelease(VisualEffect dust)
    {
        dust.gameObject.SetActive(false);
    }

    private void OnDustLandDestroy(VisualEffect dust)
    {
        Destroy(dust.gameObject);
    }

    public void SpawnDustLand()
    {
        dustLandPool.Get();
    }
    #endregion Dust Land
    
    
    
    
}
