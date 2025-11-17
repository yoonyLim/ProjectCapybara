using UnityEngine;
using UnityEngine.Splines;

public class GuideLight : MonoBehaviour
{
    private SplineAnimate _splineAnimate;

    public void Initialize(SplineContainer spline, float speed)
    {
        _splineAnimate = GetComponent<SplineAnimate>();
        _splineAnimate.Container = spline;
        _splineAnimate.AnimationMethod = SplineAnimate.Method.Speed;
        _splineAnimate.MaxSpeed = speed;
        _splineAnimate.Play();
        
        float pathLength = spline.CalculateLength();
        float duration = pathLength / speed;
        
        Destroy(gameObject, duration + 0.5f);
    }
}
