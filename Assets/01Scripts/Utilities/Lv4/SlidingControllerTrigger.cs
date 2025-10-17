using System;
using Capybara;
using Unity.Cinemachine;
using UnityEngine;

public class SlidingControllerTrigger : MonoBehaviour
{
    public float targetFOV = 30f;
    public float interpSpeed = 0.8f;
    
    private CinemachineCamera cineCam;
    private float originalFOV;
    private bool hasPlayerTriggers = false;

    private void Start()
    {
        cineCam = GameObject.FindGameObjectWithTag("CineCamera").GetComponent<CinemachineCamera>();
        originalFOV = cineCam.Lens.FieldOfView;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("controller trigger OnCollisionEnter");

            other.GetComponent<PlayerController>().enabled = false;
            other.GetComponent<CapybaraControllerSnow>().enabled = true;
            hasPlayerTriggers = true;
        }
    }

    private void Update()
    {
        float currentTargetFOV = hasPlayerTriggers ? targetFOV : originalFOV;
        
        if (!Mathf.Approximately(cineCam.Lens.FieldOfView, currentTargetFOV))
        {
            cineCam.Lens.FieldOfView = CapyHelperLibrary.FInterpTo(cineCam.Lens.FieldOfView, currentTargetFOV, Time.deltaTime, interpSpeed);
        }
    }
}
