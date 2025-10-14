using System;
using UnityEngine;

public class PlayerFollower : MonoBehaviour
{
    [SerializeField] private GameObject follower;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Vector3 followOffset;
    [SerializeField] private float followSpeed = 5f;

    public bool followPlayer = false;

    private void Awake()
    {
        if (follower == null) follower = gameObject;
        if (playerTransform == null) playerTransform = FindAnyObjectByType<PlayerController>().transform;
    }

    private void Update()
    {
        if (followPlayer) FollowPlayer();
    }

    private void FollowPlayer()
    {
        Vector3 targetPos = playerTransform.position + followOffset;
        follower.transform.position = Vector3.Lerp(follower.transform.position, targetPos, followSpeed * Time.deltaTime);
    }
}
