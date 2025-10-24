using Capybara;
using UnityEngine;

public class SnowyWindZone : MonoBehaviour
{
    public float strength;
    [SerializeField] private bool isWindZone = false;
    private Rigidbody rb;

    private void FixedUpdate()
    {
        if (isWindZone)
        {
            if (rb)
            {
                rb.AddForce(transform.TransformDirection(Vector3.up) * strength, ForceMode.Impulse);
                DualSenseInputManager.Instance.RumbleControllerForDuration(1f, 0.1f);
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<CapybaraControllerSnow>().SetIsWindZoned(true);
            rb = other.attachedRigidbody;
            isWindZone = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isWindZone = false;
        }
    }

}
