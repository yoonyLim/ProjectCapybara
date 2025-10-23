using System;
using UnityEngine;
using UnityEngine.Splines;

public class GuidelineVisualizer : MonoBehaviour
{
    private Transform playerTransform;
    private Vector3 playerPosition;
    
    private int nearestKnotIndex = 0;

    [SerializeField] private Spline guideLine;
    private void Awake()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        UpdateNearestKnot();
    }

    private void UpdateNearestKnot()
    {

    }
}
