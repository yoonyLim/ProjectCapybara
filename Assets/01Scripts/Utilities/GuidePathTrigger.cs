using System.Collections;
using Capybara;
using UnityEngine;
using UnityEngine.Splines;

public class GuidePathTrigger : MonoBehaviour
{
    [Header("References")]
    public GameObject guideLightPrefab;
    public SplineContainer path;

    [Header("Settings")]
    public float spawnInterval = 1.5f;
    public float movementSpeed = 5f;
    public int burstCount = 0;
    
    private bool _hasTriggered = false;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !_hasTriggered)
        {
            StartCoroutine(SpawnGuideLights());
        }
    }

    private IEnumerator SpawnGuideLights()
    {
        _hasTriggered = true;
        int spawnedCount = 0;

        while (burstCount == 0 || spawnedCount < burstCount)
        {
            SpawnGuideLight();
            spawnedCount++;
            
            yield return new WaitForSeconds(spawnInterval);
        }

        _hasTriggered = false;
    }

    private void SpawnGuideLight()
    {
        if (guideLightPrefab && path)
        {
            // Spawn at the start of the spline
            // Note: SplineAnimate will snap it to the start automatically on Play
            GameObject newFly = Instantiate(guideLightPrefab, transform.position, Quaternion.identity);
            
            // Setup the firefly
            var guideLight = newFly.GetComponent<GuideLight>();
            if (guideLight)
            {
                guideLight.Initialize(path, movementSpeed);
            }
        }
    }
}
