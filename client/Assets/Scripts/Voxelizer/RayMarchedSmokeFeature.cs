using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable]
public class RayMarchedSmokeSettings
{
    public enum Res
    {
        FullResolution = 0,
        HalfResolution,
        QuarterResolution
    }
    public Res resolutionScale;

    [Header("Noise Settings")]
    [Space(5)]
    [Range(0, 100000)]
    public int seed = 0;

    [Range(1, 16)]
    public int octaves = 1;

    [Range(1, 128)]
    public int cellSize = 16;

    [Range(1, 64)]
    public int axisCellCount = 4;

    [Range(0.1f, 16.0f)]
    public float amplitude = 1.0f;

    [Range(0.0f, 5.0f)]
    public float warp = 0.0f;

    [Range(-5.0f, 5.0f)]
    public float add = 0.0f;

    public bool invertNoise = false;

    public bool updateNoise = false;

    public bool debugNoise = false;

    public bool debugTiledNoise = false;

    public enum DebugAxis
    {
        X = 0,
        Y,
        Z
    }
    public DebugAxis debugNoiseAxis;

    [Range(0, 128)]
    public int debugNoiseSlice = 0;

    [Header("SDF Settings")]
    [Space(5)]
    public Vector4 cubeParams = new Vector4(0, 0, 0, 1);

    [Header("Smoke Settings")]
    [Space(5)]
    [ColorUsageAttribute(false, true)]
    public Color lightColor;

    public Color smokeColor;

    [Range(1, 256)]
    public int stepCount = 64;

    [Range(0.01f, 0.1f)]
    public float stepSize = 0.05f;

    [Range(1, 32)]
    public int lightStepCount = 8;

    [Range(0.01f, 1.0f)]
    public float lightStepSize = 0.25f;

    [Range(0.01f, 64.0f)]
    public float smokeSize = 32.0f;

    [Range(0.0f, 10.0f)]
    public float volumeDensity = 1.0f;

    [Range(0.0f, 3.0f)]
    public float absorptionCoefficient = 0.5f;

    [Range(0.0f, 3.0f)]
    public float scatteringCoefficient = 0.5f;

    public Color extinctionColor = new Color(1, 1, 1);

    [Range(0.0f, 10.0f)]
    public float shadowDensity = 1.0f;

    public enum PhaseFunction
    {
        HenyeyGreenstein = 0,
        Mie,
        Rayleigh
    }
    public PhaseFunction phaseFunction;

    [Range(-1.0f, 1.0f)]
    public float scatteringAnisotropy = 0.0f;

    [Range(0.0f, 1.0f)]
    public float densityFalloff = 0.25f;

    [Range(0.0f, 1.0f)]
    public float alphaThreshold = 0.1f;

    [Header("Animation Settings")]
    [Space(5)]
    public Vector3 animationDirection = new Vector3(0, -0.1f, 0);

    [Header("Composite Settings")]
    [Space(5)]
    public bool bicubicUpscale = true;

    [Range(-1.0f, 1.0f)]
    public float sharpness = 0.0f;

    public enum ViewTexture
    {
        Composite = 0,
        SmokeAlbedo,
        SmokeMask,
        PolygonalDepth
    }
    public ViewTexture debugView;
}

public class RayMarchedSmokeFeature : ScriptableRendererFeature
{
    [SerializeField] private RayMarchedSmokeSettings settings;

    private RayMarchedSmokePass renderPass;

    public override void Create()
    {
        renderPass = new RayMarchedSmokePass(settings);

        renderPass.renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game || renderingData.cameraData.cameraType == CameraType.SceneView)
        {
            renderPass.ConfigureInput(ScriptableRenderPassInput.Depth);
            renderPass.ConfigureInput(ScriptableRenderPassInput.Color);
        }
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game || renderingData.cameraData.cameraType == CameraType.SceneView)
        {
            renderer.EnqueuePass(renderPass);
        }
    }

    protected override void Dispose(bool disposing)
    {
        renderPass.Dispose();
    }
}
