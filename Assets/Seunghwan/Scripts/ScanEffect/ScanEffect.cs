using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteAlways]
public class ScanEffect : MonoBehaviour
{
    [SerializeField] private Material scanMaterial;
    [SerializeField] private float endScale = 50f;
    [SerializeField] private float startScale = 1.5f;
    [SerializeField] private float lifeTime = 2f;
    
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve alphaCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.2f, 1),
        new Keyframe(0.8f, 1), new Keyframe(1, 0));
    
    private Coroutine scanCoroutine;

    public void Execute()
    {
        if (scanCoroutine != null)
        {
            Moko.DebugExtension.LogWarning(gameObject, "Scan is already running");
            return;
        }

        scanCoroutine = StartCoroutine(ScanCoroutine());
    }

    private void Awake()
    {
        transform.localScale = Vector3.zero;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Execute();
        }
    }

    public IEnumerator ScanCoroutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < lifeTime)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / lifeTime);
            
            transform.localScale = Vector3.one * (startScale + scaleCurve.Evaluate(t) * (endScale - startScale));
            float currentAlpha = alphaCurve.Evaluate(t);
            
            Color color = scanMaterial.GetColor("_Colour");
            color.a = currentAlpha;
            scanMaterial.SetColor("_Colour", color);
            
            yield return null;
        }

        transform.localScale = Vector3.zero;
        scanCoroutine = null;
    }

    void LateUpdate()
    {
        scanMaterial.SetVector("_Position", transform.position);
        scanMaterial.SetFloat("_Radius", transform.localScale.x);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = scanMaterial.GetColor("_Colour");
        Gizmos.DrawWireSphere(transform.position, transform.lossyScale.x);
    }
}
