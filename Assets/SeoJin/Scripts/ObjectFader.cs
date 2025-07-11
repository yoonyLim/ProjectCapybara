using System;
using UnityEditor;
using UnityEngine;


/// <summary>
/// Attach To Environment Object, Make sure Object Material's Surface Type is "Transparent"
/// </summary>
public class ObjectFader : MonoBehaviour
{
    public float FadeSpeed, TargetOpacity;
    private float originalOpacity;
    private Material mat;

    public bool DoFade = false;

    void Start()
    {
        mat = GetComponent<Renderer>().material;
        originalOpacity = mat.color.a;
    }

    private void Update()
    {
        if (DoFade)
        {
            Fade();
        }
        else
        {
            ResetOpacity();
        }
    }

    void Fade()
    {
        if (Mathf.Approximately(mat.color.a, TargetOpacity))
        {
            return;
        }
        
        Color currentColor = mat.color;
        Color smoothColor = new Color(currentColor.r, currentColor.g, currentColor.b,
            Mathf.Lerp(currentColor.a, TargetOpacity, FadeSpeed * Time.deltaTime));
        mat.color = smoothColor;
    }

    void ResetOpacity()
    {
        if (Mathf.Approximately(mat.color.a, originalOpacity))
        {
            return;
        }
        
        Color currentColor = mat.color;
        Color smoothColor = new Color(currentColor.r, currentColor.g, currentColor.b,
            Mathf.Lerp(currentColor.a, originalOpacity, FadeSpeed * Time.deltaTime));
        mat.color = smoothColor;
    }
}
