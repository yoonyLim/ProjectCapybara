using ExternPropertyAttributes;
using UnityEngine;

public class LogPlatform : MonoBehaviour
{
    [SerializeField] private float startTimeOffset = 3f;
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float moveDistanceRatio = 1f;
    private Collider platformCollider;
    private Rigidbody platformRigidbody;
    
    
    private Vector3 startPosition;
    private float moveDistance;

    private void Awake()
    {
        platformRigidbody = GetComponent<Rigidbody>();
        platformCollider = GetComponent<Collider>();
        startPosition = transform.position;
        moveDistance = platformCollider.bounds.size.y * moveDistanceRatio;
    }

    private void FixedUpdate()
    {
        float currentPositionOffset = Mathf.PingPong(Time.time * moveSpeed + startTimeOffset, moveDistance);
        platformRigidbody.MovePosition(startPosition - transform.up * currentPositionOffset);
    }
    
}
