using Capybara;
using UnityEngine;

public class SetFinalScenePlayerSpeed : MonoBehaviour
{
    [SerializeField] private float speed;
    
    protected void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<CapybaraControllerFinalScene>().SetSpeed(speed);
        }
    }
}
