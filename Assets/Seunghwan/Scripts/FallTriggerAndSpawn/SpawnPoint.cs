using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private GameObject spawnPointPosition;
    [SerializeField] private GameObject spawnPointTriggerBox;

    public Vector3 GetSpawnPointPosition()
    {
        return spawnPointPosition.transform.position;
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.0f, 0f, 0.05f);
        Gizmos.matrix = spawnPointTriggerBox.transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, Vector3.one);

        Gizmos.color = new Color(1f, 0.0f, 0f, 1f);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
#endif
}
