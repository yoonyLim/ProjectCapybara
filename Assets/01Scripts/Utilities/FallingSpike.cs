using System.Collections;
using UnityEngine;

public class FallingSpike : MonoBehaviour
{
    public float distance = 0f;
    public AnimationCurve shakeCurve;
    public float shakeDuration = 1f;
    public float knockbackForce = 10f;
    
    private Rigidbody rb;
    CapsuleCollider collider;

    private bool isFalling = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<CapsuleCollider>();
    }

    IEnumerator Shake()
    {
        Vector3 startPos = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            elapsedTime += Time.deltaTime;
            float shakeStrength = shakeCurve.Evaluate(elapsedTime / shakeDuration);
            Vector3 newPos = startPos + Random.insideUnitSphere * shakeStrength;
            
            transform.position = new Vector3(newPos.x, startPos.y, newPos.z);
            yield return null;
        }
        
        transform.position = startPos;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player") && isFalling)
        {
            Vector3 knockbackDirection = (collision.collider.transform.position - transform.position);
            knockbackDirection = new Vector3(knockbackDirection.x, 0, knockbackDirection.z).normalized;
            
            collision.rigidbody.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
        }
        else
        {
            isFalling = false;
        }
    }
    
    void Update()
    {
        // Physics.queriesHitTriggers = false;

        if (!isFalling)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, distance))
            {
                if (hit.transform.CompareTag("Player"))
                {
                    Debug.Log("Player");

                    StartCoroutine(Shake());
                    
                    rb.useGravity = true;
                    isFalling = true;
                }
            }
            
            Debug.DrawRay(transform.position, Vector3.down * distance, Color.red);
        }
    }
}
