using System;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;

public class DestructibleRock : MonoBehaviour, IDestructible
{
    //private ParticleSystem rockParticleSystem;
    private Collider rockCollider;
    private MeshRenderer rockMeshRenderer;
    [SerializeField] private Transform brokenRock;
    [SerializeField] Vector3 brokenPosition;
    private void Awake()
    {
        //rockParticleSystem = GetComponent<ParticleSystem>();
        rockCollider = GetComponent<Collider>();
        rockMeshRenderer = GetComponent<MeshRenderer>();
    }

    public void Hit()
    {
        rockCollider.enabled = false;
        rockMeshRenderer.enabled = false;
        brokenRock.gameObject.SetActive(true);
        foreach (Transform child in brokenRock)
        {
            if (child.TryGetComponent<Rigidbody>(out Rigidbody childRigidbody))
            {
                childRigidbody.AddExplosionForce(60f, brokenPosition, 5f);
                Destroy(brokenRock.gameObject, 5f);
            }
            Destroy(gameObject, 5f);
        }
        //transform.GetChild(0).gameObject.SetActive(false);
    }

    
}
