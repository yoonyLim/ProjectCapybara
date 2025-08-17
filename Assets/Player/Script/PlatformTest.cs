using System;
using System.Collections;
//using Abiogenesis3d.UPixelator_Demo;
using Moko;
using UnityEngine;

public class PlatformPositionComposer : MonoBehaviour
{
    [SerializeField] private Transform start;
    [SerializeField] private Transform end;

    [Tooltip("start부터 end까지 이동하는 데 걸리는 시간")]
    [SerializeField] private float moveTime = 2f;

    [Tooltip("start나 end에 도착했을 때 멈춰있을 시간")]
    [SerializeField] private float pauseTime = 1f;

    [Tooltip("이동 시 이용할 애니메이션 커브")]
    public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Rigidbody rb;

    private void Awake()
    {
        if (start == null) DebugExtension.Log(this, "Start Transform not set");
        if (end == null) DebugExtension.Log(this, "End Transform not set");
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        StartCoroutine(MovePlatform());
    }

    IEnumerator MovePlatform()
    {
        Vector3 startPos = start.position;
        Vector3 endPos = end.position;

        while (true)
        {
            yield return StartCoroutine(MoveToTarget(startPos, endPos));

            yield return new WaitForSeconds(pauseTime);

            yield return StartCoroutine(MoveToTarget(endPos, startPos));

            yield return new WaitForSeconds(pauseTime);
        }
    }

    IEnumerator MoveToTarget(Vector3 from, Vector3 to)
    {
        float elapsedTime = 0f;

        while (elapsedTime < moveTime)
        {
            float t = elapsedTime / moveTime;
            Vector3 nextPos = Vector3.Lerp(from, to, curve.Evaluate(t));
            rb.MovePosition(nextPos);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rb.MovePosition(to);
    }
}
