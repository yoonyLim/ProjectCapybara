using System.Collections;
using UnityEngine;


/// <summary>
/// Attach To Environment Object, Make sure Object Material's Surface Type is "Transparent"
/// </summary>
public class ObjectFader : MonoBehaviour
{
    [Tooltip("페이드가 완료되는 데 걸리는 시간(초)")]
    public float FadeDuration = 1.0f;
    [Tooltip("페이드 됐을 때의 목표 투명도 (0: 투명, 1: 불투명)")]
    [Range(0f, 1f)]
    public float TargetOpacity = 0.3f;

    public bool DoFade
    {
        get => _doFade;
        set
        {
            if (_doFade == value) return; // 값이 같으면 return
            _doFade = value; // 바뀌었으면, 값을 넣고 TriggerFade
            TriggerFade();
        }
    }
    
    private bool _doFade = false;
    private float _originalOpacity;
    private Material _materialInstance;
    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogError("Renderer 컴포넌트를 찾을 수 없습니다.", this);
            enabled = false; 
            return;
        }
        _materialInstance = renderer.material;
        _originalOpacity = _materialInstance.color.a;
    }
    
    private void OnDestroy()
    {
        if (_materialInstance != null)
        {
            Destroy(_materialInstance);
        }
    }

    /// <summary>
    /// DoFade 값 변경에 따라 페이드를 시작/중단
    /// </summary>
    private void TriggerFade()
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }

        float targetAlpha = _doFade ? TargetOpacity : _originalOpacity;
        _fadeCoroutine = StartCoroutine(FadeRoutine(targetAlpha));
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        Color startColor = _materialInstance.color;
        float startAlpha = startColor.a;
        float elapsedTime = 0f;

        while (elapsedTime < FadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / FadeDuration);
            
            _materialInstance.color = new Color(startColor.r, startColor.g, startColor.b, newAlpha);
            
            yield return null; 
        }
        
        // 페이드가 끝나고 목표값으로 Snap
        _materialInstance.color = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
    }
}
