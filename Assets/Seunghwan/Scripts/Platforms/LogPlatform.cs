using UnityEngine;

public class LogPlatform : MonoBehaviour
{
    [SerializeField] private float startTimeOffset = 0.3f;
    [SerializeField] private float moveUpDistance = 5f;
    [SerializeField] private float moveSpeed = 1f;
    [Header("This should be a curve from (0,0) to (1,1)")]
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    
    private Vector3 startPosition;
    private Vector3 endPosition;

    private void Awake()
    {
        startPosition = transform.position;
        endPosition = transform.position + Vector3.up * moveUpDistance;
    }

    private void FixedUpdate()
    {
        float timeRatio = Mathf.PingPong(Time.time * moveSpeed + startTimeOffset, 1);
        float moveRatio = moveCurve.Evaluate(timeRatio);
        transform.position = Vector3.Lerp(startPosition, endPosition, moveRatio);
    }
    
}
