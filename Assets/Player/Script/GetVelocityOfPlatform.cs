using UnityEngine;

public class GetVelocityOfPlatform : MonoBehaviour
{
    private Vector3 _previousPosition;
    private Vector3 _velocity;

    private void Start()
    {
        _previousPosition = transform.position;
    }

    private void Update()
    {
        _velocity = (transform.position - _previousPosition) / Time.deltaTime;
        _previousPosition = transform.position;
    }

    // player script gets the platform's velocity from here
    public Vector3 GetVelocity()
    {
        return _velocity;
    }
}
