using ExternPropertyAttributes;
using UnityEngine;

public class LogPlatform : MonoBehaviour
{
    [SerializeField] private float startTimeOffset = 3f;
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float moveDistanceRatio = 1f;
    private Collider collider;
    
    
    private Vector3 startPosition;
    private float moveDistance;

    private void Awake()
    {
        collider = GetComponent<Collider>();
        startPosition = transform.position;
        moveDistance = collider.bounds.size.y * moveDistanceRatio;
    }

    private void FixedUpdate()
    {
        float currentPositionOffset = Mathf.PingPong(Time.time * moveSpeed + startTimeOffset, moveDistance);
        transform.position = startPosition - transform.up * currentPositionOffset;
    }
    
}
