
using System.Collections.Generic;
using UnityEngine;


public class SnowController : MonoBehaviour
{
    public ComputeShader snowComputeShader;
    private RenderTexture snowRT;
    public RenderTexture SnowRT => snowRT;
    public float colorValueToAdd = 0.005f;

    private string snowImageProperty = "snowImage";
    private string colorValueProperty = "colorValueToAdd";
    private string resolutionProperty = "resolution";
    private string positionXProperty = "positionX";
    private string PositionYProperty = "positionY";
    private string spotSizeProperty = "spotSize";

    private string csMainKernel = "CSMain";
    private string fillWhiteKernel = "FillWhite";

    private MeshRenderer meshRenderer;

    private int resolution = 1024;

    private Camera mainCamera;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        mainCamera = Camera.main;
        CreateRenderTexture();
        SetRTColorToWhite();
        SetMaterialTexture();
        InvokeRepeating(nameof(AddSnowLayer), 0f, 0.05f);
        ExtendBoundOfMesh();
    }

    private void CreateRenderTexture()
    {
        snowRT = new RenderTexture(resolution, resolution, 24)
        {
            enableRandomWrite = true
        };
        snowRT.Create();
    }

    private void SetRTColorToWhite()
    {
        int kernelHandle = snowComputeShader.FindKernel(fillWhiteKernel);
        snowComputeShader.SetTexture(kernelHandle, snowImageProperty, snowRT);
        snowComputeShader.SetFloat(colorValueProperty, colorValueToAdd);
        snowComputeShader.SetFloat(resolutionProperty, resolution);
        snowComputeShader.SetFloat(positionXProperty, 0);
        snowComputeShader.SetFloat(PositionYProperty, 0);
        snowComputeShader.SetFloat(spotSizeProperty, 0);
        snowComputeShader.Dispatch(kernelHandle, snowRT.width / 8, snowRT.height / 8, 1);
    }

    private void SetMaterialTexture()
    {
        meshRenderer.material.SetTexture("_PathTexture", snowRT);
    }

    private void AddSnowLayer()
    {
        int kernelHandle = snowComputeShader.FindKernel(csMainKernel);
        snowComputeShader.SetTexture(kernelHandle, snowImageProperty, snowRT);
        snowComputeShader.SetFloat(colorValueProperty, colorValueToAdd);
        snowComputeShader.SetFloat(resolutionProperty, resolution);
        snowComputeShader.SetFloat(positionXProperty, 0);
        snowComputeShader.SetFloat(PositionYProperty, 0);
        snowComputeShader.SetFloat(spotSizeProperty, 0);
        snowComputeShader.Dispatch(kernelHandle, snowRT.width / 8, snowRT.height / 8, 1);
    }

    private void ExtendBoundOfMesh()
    {
        Bounds bounds = GetComponent<MeshFilter>().mesh.bounds;
        bounds.extents = new Vector3(2, 0, 2);
        GetComponent<MeshFilter>().mesh.bounds = bounds;
    }

}
