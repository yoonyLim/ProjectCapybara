using Gamekit3D;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.VFX;

public class DustSpawner : MonoBehaviour
{
    [SerializeField] private VisualEffect dustTrailPrefab;
    [SerializeField] private VisualEffect dustLandPrefab;
    private IObjectPool<VisualEffect> dustTrailPool;
    private IObjectPool<VisualEffect> dustLandPool;
    [SerializeField] private Transform dustTrailSpawnPoint;
    [SerializeField] private Transform dustLandSpawnPoint;
    [SerializeField] private PlayerController playerController;

    private void Awake()
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController>();

        dustTrailPool = new ObjectPool<VisualEffect>(CreateDustTrail, OnDustTrailGet,
            OnDustTrailRelease, OnDustTrailDestroy, true, 10, 20);
        
        dustLandPool = new ObjectPool<VisualEffect>(CreateDustLand, OnDustLandGet,
            OnDustLandRelease, OnDustLandDestroy, true, 3, 5);
    }
    
    #region Dust Trail
    private VisualEffect CreateDustTrail()
    {
        VisualEffect instance = Instantiate(dustTrailPrefab, dustTrailSpawnPoint.position, dustTrailSpawnPoint.rotation);
        return instance;
    }

    private void OnDustTrailGet(VisualEffect dust)
    {
        dust.transform.position = dustTrailSpawnPoint.position;
        dust.transform.rotation = dustTrailSpawnPoint.rotation;
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
        if (dust == null) return;
        Destroy(dust.gameObject);
    }
    
    /// <summary>
    /// The public function wrapper for the internal get from dust trail pool. It is used by the animation event inside
    /// run animation clip.
    /// </summary>
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

        dust.transform.position = dustLandSpawnPoint.position;
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
    /// <summary>
    /// The public function wrapper for internal get from dust land pool.
    /// </summary>
    public void SpawnDustLand()
    {
        if (!playerController.canSpawnDustLand)
            return;

        playerController.canSpawnDustLand = false;
        dustLandPool.Get();
    }
    #endregion Dust Land
    
    
    
    
}
