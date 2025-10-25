using System;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Splines;

public class GuidelineVisualizer : MonoBehaviour
{
    private Transform playerTransform;
    private Vector3 playerPosition => playerTransform.position;
    
    private int nearestKnotIndex = 0;
    private int previousNearestKnotIndex => nearestKnotIndex - 1;
    private int nextNearestKnotIndex => nearestKnotIndex + 1;

    [SerializeField] private RectTransform directionIndicator;
    [SerializeField] private float directionIndicatorRotationSpeed = 10f;

    [SerializeField] private float indicatorEnableThresholdAngle = 120f;
    [SerializeField] private float indicatorFadeSpeed = 10f;
    
    private Camera mainCamera;
    
    [SerializeField] private CanvasGroup indicatorCanvasGroup; 
    [SerializeField] private SplineContainer guideLine;

    [SerializeField] private Transform flymodeTranform;
    private void Awake()
    {
        playerTransform = flymodeTranform;
        mainCamera = Camera.main;
        indicatorCanvasGroup = directionIndicator.GetComponent<CanvasGroup>();
        indicatorCanvasGroup.alpha = 0f;
    }

    private void Update()
    {
        UpdateNearestKnotIndex();
        
        bool isForward = IsPlayerMovingForward();
        
        if (!isForward)
        {
            Debug.Log("WRONG WAY");
        }

        Vector3 splineForwardXZ = GetDirectionToNextKnotXZ();
        
        float targetAlpha = 0f;
        float deviationAngle = Vector3.Angle(splineForwardXZ, playerTransform.forward);
        
        UpdateIndicatorRotation(splineForwardXZ);
        
        if (deviationAngle > indicatorEnableThresholdAngle)
        {
            targetAlpha = 1f;
        }

        indicatorCanvasGroup.alpha = Mathf.Lerp(
            indicatorCanvasGroup.alpha,
            targetAlpha,
            Time.deltaTime * indicatorFadeSpeed);
    }

    private void UpdateNearestKnotIndex()
    {
        float previousNearestKnotDistance = Mathf.Infinity;
        float nextNearestKnotDistance = Mathf.Infinity;
        float nearestKnotDistance;
        
        if (previousNearestKnotIndex >= 0)
        {
            Vector3 prevKnotWorldPos = guideLine.transform.TransformPoint(guideLine.Spline[previousNearestKnotIndex].Position);
            previousNearestKnotDistance = Vector3.Distance(playerPosition, prevKnotWorldPos);
        }
        
        Vector3 nearestKnotWorldPos = guideLine.transform.TransformPoint(guideLine.Spline[nearestKnotIndex].Position);
        nearestKnotDistance = Vector3.Distance(playerPosition, nearestKnotWorldPos);

        if (nextNearestKnotIndex < guideLine.Spline.Count)
        {
            Vector3 nextKnotWorldPos = guideLine.transform.TransformPoint(guideLine.Spline[nextNearestKnotIndex].Position);
            nextNearestKnotDistance = Vector3.Distance(playerPosition, nextKnotWorldPos);
        }

        if (previousNearestKnotDistance < Mathf.Min(nextNearestKnotDistance, nearestKnotDistance)) 
        {
            nearestKnotIndex -= 1;
        }
        else if (nextNearestKnotDistance < Mathf.Min(previousNearestKnotDistance, nearestKnotDistance))
        {
            nearestKnotIndex += 1;
        }
    }

    private bool IsPlayerMovingForward()
    {
        Vector3 playerForward = playerTransform.forward.normalized;
        BezierKnot knot = guideLine.Spline[nearestKnotIndex];

        Vector3 splineOutgoingDirection = knot.TangentOut;
        splineOutgoingDirection.Normalize();
        
        Vector3 splineIncomingDirection = -knot.TangentIn;
        splineIncomingDirection.Normalize();

        float outgoingAlignment = Vector3.Dot(playerForward, splineOutgoingDirection);
        float incomingAlignment = Vector3.Dot(playerForward, splineIncomingDirection);

        if (outgoingAlignment > 0.3f || incomingAlignment > 0.3f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private Vector3 GetDirectionToNextKnotXZ()
    {
        int currentIndex = nearestKnotIndex;
        int nextIndex = currentIndex + 1;
        
        if (currentIndex == guideLine.Spline.Count - 1)
        {
            currentIndex = guideLine.Spline.Count - 2;
            nextIndex = guideLine.Spline.Count - 1;
        }
        
        Vector3 currentKnotWorldPos = guideLine.transform.TransformPoint(guideLine.Spline[currentIndex].Position);
        Vector3 nextKnotWorldPos = guideLine.transform.TransformPoint(guideLine.Spline[nextIndex].Position);

        Vector3 direction3D = (nextKnotWorldPos - currentKnotWorldPos).normalized;

        Vector3 directionXZ = new Vector3(direction3D.x, 0f, direction3D.z);

        if (directionXZ.sqrMagnitude < 0.001f)
        {
            return Vector3.zero; 
        }

        return directionXZ.normalized;
    }
    
    private void UpdateIndicatorRotation(Vector3 splineForwardXZ)
    {
        if (splineForwardXZ == Vector3.zero)
        {
            return;
        }

        Vector3 camForwardXZ = new Vector3(mainCamera.transform.forward.x, 0f, mainCamera.transform.forward.z).normalized;

        float targetAngle = Vector3.SignedAngle(
            camForwardXZ,      
            splineForwardXZ,  
            Vector3.up         
        );

        float uiTargetAngleZ = -targetAngle;

        float currentAngleZ = directionIndicator.localEulerAngles.z;

        float newAngleZ = Mathf.LerpAngle(
            currentAngleZ,
            uiTargetAngleZ,
            Time.deltaTime * directionIndicatorRotationSpeed 
        );

        directionIndicator.localEulerAngles = new Vector3(0, 0, newAngleZ);
    }
}
