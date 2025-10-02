using UnityEngine;

public class WindZone : MonoBehaviour
{
    public float strength;
    public Vector3 direction;
    [SerializeField] private bool isWindZone = false;
    private Rigidbody rb;

    private void FixedUpdate()
    {
        if (isWindZone)
        {
            if (rb != null)
            {
                Vector3 vel = rb.linearVelocity;
                vel.y = 0;
                rb.linearVelocity = vel;

                rb.AddForce(direction.normalized * strength, ForceMode.Impulse);
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
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
