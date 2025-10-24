using System;
using System.Numerics;
using Moko;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class DustLandSpawnerAligner : MonoBehaviour
{
    [SerializeField] private LayerMask raycastLayerMask;

    private void FixedUpdate()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, raycastLayerMask))
        {
            Vector3 groundVector = Vector3.ProjectOnPlane(Vector3.up, hit.normal);
            if (groundVector != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(groundVector);
                transform.rotation = targetRotation;
            }
        }
    }
}
