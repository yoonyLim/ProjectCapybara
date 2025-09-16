using UnityEngine;

public class LoadingTrigger : MonoBehaviour
{
    [SerializeField] private LoadingManager loadingManager;
    [SerializeField] private int sceneIndexToLoad;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            loadingManager.LoadScene(sceneIndexToLoad);
        }
    }
}
