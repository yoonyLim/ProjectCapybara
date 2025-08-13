using System;
using System.Collections;
using UnityEngine;

public class InvisiblePath : MonoBehaviour
{
    [SerializeField] private Material defaultModeMaterial;
    [SerializeField] private Material persistModeMaterial;
    
    private float persistDuration = 3.0f;
    private float fadeDuration = 0.5f;
    
    private MaterialPropertyBlock materialPropertyBlock;
    private bool isPersistMode = false;
    
    private Renderer objectRenderer;
    private void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
        objectRenderer.material = defaultModeMaterial;
        materialPropertyBlock = new MaterialPropertyBlock();
    }
    
    public void EnablePersistMode()
    {
        if (isPersistMode) return;
        
        objectRenderer.material = persistModeMaterial;
        materialPropertyBlock.SetFloat("_Alpha", 1.0f);
        objectRenderer.SetPropertyBlock(materialPropertyBlock);
        isPersistMode = true;
        StartCoroutine(FadeCoroutine());
    
    }
    
    IEnumerator FadeCoroutine()
    {
        yield return new WaitForSeconds(persistDuration);
        
        float elapsedTime = 0.0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeDuration);
            float alpha = Mathf.Lerp(1.0f, 0.0f, t);
            materialPropertyBlock.SetFloat("_Alpha", alpha);
            objectRenderer.SetPropertyBlock(materialPropertyBlock);
            yield return null;
        }
        
        objectRenderer.material = defaultModeMaterial;
        objectRenderer.SetPropertyBlock(null);
        isPersistMode = false;
        
    }
    
    
}
