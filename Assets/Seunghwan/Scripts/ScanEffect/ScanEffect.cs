using System.Collections;
using UnityEngine;

public class ScanEffect : MonoBehaviour
{
    private AudioSource scanAudioSource;
    private Material scanMaterial;
    [SerializeField] private Material invisibleGlowMaterial;
    [SerializeField] private Material highlightGlowMaterial;
    [SerializeField] private float endScale = 50f;
    [SerializeField] private float startScale = 1.5f;
    [SerializeField] private float lifeTime = 2f;
    
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve alphaCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.2f, 1),
        new Keyframe(0.8f, 1), new Keyframe(1, 0));
    
    private Coroutine scanCoroutine;
    
    [SerializeField] private LayerMask transparentLayerMask;
    Collider[] foundColliders = new Collider[10];

    public void Execute()
    {
        if (scanCoroutine != null)
        {
            return;
        }
        
        scanAudioSource.Play();
        scanCoroutine = StartCoroutine(ScanCoroutine());
    }

    private void Awake()
    {
        transform.localScale = Vector3.zero;
        scanMaterial = GetComponent<MeshRenderer>().material;
        scanAudioSource = GetComponent<AudioSource>();

        Shader.SetGlobalVector("_ScanCenter", transform.position);
        Shader.SetGlobalFloat("_ScanRadius", 0.0f);
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

        Shader.SetGlobalVector("_ScanCenter", transform.position);
        Shader.SetGlobalFloat("_ScanRadius", transform.localScale.x / 2.0f);
        invisibleGlowMaterial.SetFloat("_Alpha", 1f);
        highlightGlowMaterial.SetFloat("_Alpha", 1f);

        while (elapsedTime < lifeTime)
        {
            elapsedTime += Time.deltaTime;

            Shader.SetGlobalVector("_ScanCenter", transform.position);
            Shader.SetGlobalFloat("_ScanRadius", transform.localScale.x / 2.0f);
            
            float t = Mathf.Clamp01(elapsedTime / lifeTime);
            
            transform.localScale = Vector3.one * (startScale + scaleCurve.Evaluate(t) * (endScale - startScale));
            float currentAlpha = alphaCurve.Evaluate(t);
            
            Color color = scanMaterial.GetColor("_Color");
            color.a = currentAlpha;
            scanMaterial.SetColor("_Color", color);

            // int numberOfColliders = Physics.OverlapSphereNonAlloc(transform.position, transform.localScale.x / 2.0f,
            //     foundColliders, transparentLayerMask);
            // if (numberOfColliders > 0)
            // {
            //     for (int i = 0; i < numberOfColliders; i++)
            //     {
            //         if (!foundColliders[i].TryGetComponent<InvisiblePath>(out var invisiblePath)) continue;
            //
            //         float distance = Vector3.Distance(foundColliders[i].bounds.center, transform.position)
            //                          + foundColliders[i].bounds.extents.magnitude;
            //         if (distance < transform.localScale.x / 2.0f)
            //         {
            //             invisiblePath.EnablePersistMode();
            //         }
            //     }
            // }
            
            yield return null;
        }

        transform.localScale = Vector3.zero;

        yield return FadeCoroutine();
        
        scanCoroutine = null;
    }

    IEnumerator FadeCoroutine()
    {
        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, Mathf.Clamp01(elapsedTime / 1f));
            invisibleGlowMaterial.SetFloat("_Alpha", alpha);
            highlightGlowMaterial.SetFloat("_Alpha", alpha);
            yield return null;
        }
        
        invisibleGlowMaterial.SetFloat("_Alpha", 0f);
        highlightGlowMaterial.SetFloat("_Alpha", 0);
        
    }
}
