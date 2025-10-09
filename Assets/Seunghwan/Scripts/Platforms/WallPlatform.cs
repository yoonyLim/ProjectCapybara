using UnityEngine;

public class WallPlatform : MonoBehaviour
{
    [SerializeField] private float startTimeOffset = 0.3f;
    [SerializeField] private float moveSpeed = 5f;
    private BoxCollider boxCollider;
    private Rigidbody platformRigidbody;


    private Vector3 startPosition;
    private float moveDistance;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        platformRigidbody = GetComponent<Rigidbody>();
        startPosition = transform.position;
        moveDistance = boxCollider.size.z * transform.lossyScale.z;
    }

    private void FixedUpdate()
    {
        float currentPositionOffset = Mathf.PingPong(moveSpeed * Time.time + startTimeOffset, moveDistance);
       platformRigidbody.MovePosition(startPosition - transform.forward * currentPositionOffset);
    }
}
