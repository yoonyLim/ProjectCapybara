using UnityEngine;
using Random = UnityEngine.Random;

public class IslandPlacer : MonoBehaviour
{
    [SerializeField] private GameObject islandPrefab;
    [SerializeField] private int numberOfObjects = 10; 
    
    [SerializeField] private Vector3 placementAreaSize = new Vector3(20f, 0f, 20f);
    [SerializeField] private Vector3 ignoreAreaSize = new Vector3(5f, 0f, 5f);

    [SerializeField] private float minScale = 0.8f;
    [SerializeField] private float maxScale = 1.5f;
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawCube(transform.position, placementAreaSize);

        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(transform.position, ignoreAreaSize);
    }
    
    [ContextMenu("Generate Islands")]
    private void GenerateObjects()
    {
        if (islandPrefab == null)
        {
            Debug.LogError("Island prefab is null");
            return;
        }

        ClearIslands();

        int attempts = 0; 
        for (int i = 0; i < numberOfObjects; i++)
        {
            Vector3 randomPosition;
            
            while (true)
            {
                float randomX = Random.Range(-placementAreaSize.x / 2, placementAreaSize.x / 2);
                float randomY = Random.Range(-placementAreaSize.y / 2, placementAreaSize.y / 2);
                float randomZ = Random.Range(-placementAreaSize.z / 2, placementAreaSize.z / 2);

                randomPosition = new Vector3(randomX, randomY, randomZ);
                
                bool isOutsideIgnoreX = Mathf.Abs(randomPosition.x) > ignoreAreaSize.x / 2;
                bool isOutsideIgnoreZ = Mathf.Abs(randomPosition.z) > ignoreAreaSize.z / 2;
                
                if (isOutsideIgnoreX || isOutsideIgnoreZ)
                {
                    break;
                }
                
                attempts++;
                if (attempts > 1000)
                {
                    return;
                }
            }
            
            float randomYaw = Random.Range(0f, 360f);
            Quaternion randomRotation = Quaternion.Euler(0f, randomYaw, 0f);
            
            float randomScaleValue = Random.Range(minScale, maxScale);
            Vector3 randomScale = Vector3.one * randomScaleValue;
            
            GameObject newObj = Instantiate(islandPrefab, transform.position + randomPosition, randomRotation);
            
            newObj.transform.parent = this.transform;
            newObj.transform.localScale = randomScale;
        }
        
    }
    
    [ContextMenu("Clear Islands")]
    private void ClearIslands()
    {
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }
}