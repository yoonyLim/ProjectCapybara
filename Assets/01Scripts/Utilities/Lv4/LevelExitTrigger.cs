using Capybara;
using UnityEngine;

public class LevelExitTrigger : MonoBehaviour
{
    [SerializeField] private GameObject exitOrb;
    
    protected void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            exitOrb.SetActive(true);
        }
    }
}
