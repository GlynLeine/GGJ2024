using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RayMarchedSmokePass : ScriptableRenderPass
{
    private RayMarchedSmokeSettings settings;

    private Material compositeMaterial;
    private ComputeShader raymarchCompute;

    private int generateNoisePass, debugNoisePass, raymarchSmokePass;

    private RenderTextureDescriptor noiseDescriptor, depthDescriptor, colorDescriptor;
    private RTHandle noiseTex, depthTex, colorTex;

    private RenderTextureDescriptor smokeAlbedoFullDescriptor, smokeAlbedoHalfDescriptor, smokeAlbedoQuarterDescriptor;
    private RTHandle smokeAlbedoFullTex, smokeAlbedoHalfTex, smokeAlbedoQuarterTex;
    private RenderTextureDescriptor smokeMaskFullDescriptor, smokeMaskHalfDescriptor, smokeMaskQuarterDescriptor;
    private RTHandle smokeMaskFullTex, smokeMaskHalfTex, smokeMaskQuarterTex;

    private ComputeBuffer smokeVoxelBuffer;

    private Vector2Int GetRenderResolution()
    {
        switch ((int)settings.resolutionScale)
        {
            case 0:
                return new Vector2Int(smokeAlbedoFullDescriptor.width, smokeAlbedoFullDescriptor.height);
            case 1:
                return new Vector2Int(smokeAlbedoHalfDescriptor.width, smokeAlbedoHalfDescriptor.height);
            case 2:
                return new Vector2Int(smokeAlbedoQuarterDescriptor.width, smokeAlbedoQuarterDescriptor.height);
        }

        return new Vector2Int(smokeAlbedoFullDescriptor.width, smokeAlbedoFullDescriptor.height);

    }

    private RTHandle GetSmokeAlbedoTex()
    {
        switch ((int)settings.resolutionScale)
        {
            case 0:
                return smokeAlbedoFullTex;
            case 1:
                return smokeAlbedoHalfTex;
            case 2:
                return smokeAlbedoQuarterTex;
        }

        return smokeAlbedoFullTex;
    }

    private RTHandle GetSmokeMaskTex()
    {
        switch ((int)settings.resolutionScale)
        {
            case 0:
                return smokeMaskFullTex;
            case 1:
                return smokeMaskHalfTex;
            case 2:
                return smokeMaskQuarterTex;
        }

        return smokeMaskFullTex;
    }

    public RayMarchedSmokePass(RayMarchedSmokeSettings defaultSettings)
    {
        this.settings = defaultSettings;

        InitializeVariables();
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        //Set the raymarch texture size to be the same as the camera target size.
        smokeAlbedoFullDescriptor.width = cameraTextureDescriptor.width;
        smokeAlbedoFullDescriptor.height = cameraTextureDescriptor.height;

        //Check if the descriptor has changed, and reallocate the RTHandle if necessary.
        RenderingUtils.ReAllocateIfNeeded(ref smokeAlbedoFullTex, smokeAlbedoFullDescriptor);

        smokeAlbedoHalfDescriptor.width = Mathf.CeilToInt(cameraTextureDescriptor.width / 2);
        smokeAlbedoHalfDescriptor.height = Mathf.CeilToInt(cameraTextureDescriptor.height / 2);
        RenderingUtils.ReAllocateIfNeeded(ref smokeAlbedoHalfTex, smokeAlbedoHalfDescriptor);

        smokeAlbedoQuarterDescriptor.width = Mathf.CeilToInt(cameraTextureDescriptor.width / 4);
        smokeAlbedoQuarterDescriptor.height = Mathf.CeilToInt(cameraTextureDescriptor.height / 4);
        RenderingUtils.ReAllocateIfNeeded(ref smokeAlbedoQuarterTex, smokeAlbedoQuarterDescriptor);


        smokeMaskFullDescriptor.width = cameraTextureDescriptor.width;
        smokeMaskFullDescriptor.height = cameraTextureDescriptor.height;
        RenderingUtils.ReAllocateIfNeeded(ref smokeMaskFullTex, smokeMaskFullDescriptor);

        smokeMaskHalfDescriptor.width = Mathf.CeilToInt(cameraTextureDescriptor.width / 2);
        smokeMaskHalfDescriptor.height = Mathf.CeilToInt(cameraTextureDescriptor.height / 2);
        RenderingUtils.ReAllocateIfNeeded(ref smokeMaskHalfTex, smokeMaskHalfDescriptor);

        smokeMaskQuarterDescriptor.width = Mathf.CeilToInt(cameraTextureDescriptor.width / 4);
        smokeMaskQuarterDescriptor.height = Mathf.CeilToInt(cameraTextureDescriptor.height / 4);
        RenderingUtils.ReAllocateIfNeeded(ref smokeMaskQuarterTex, smokeMaskQuarterDescriptor);


        depthDescriptor.width = cameraTextureDescriptor.width;
        depthDescriptor.height = cameraTextureDescriptor.height;
        RenderingUtils.ReAllocateIfNeeded(ref depthTex, depthDescriptor);

        colorDescriptor.width = cameraTextureDescriptor.width;
        colorDescriptor.height = cameraTextureDescriptor.height;
        RenderingUtils.ReAllocateIfNeeded(ref colorTex, colorDescriptor);
    }

    void UpdateNoise()
    {
        raymarchCompute.SetTexture(generateNoisePass, "_RWNoiseTex", noiseTex);
        raymarchCompute.SetInt("_Octaves", settings.octaves);
        raymarchCompute.SetInt("_CellSize", settings.cellSize);
        raymarchCompute.SetInt("_AxisCellCount", settings.axisCellCount);
        raymarchCompute.SetFloat("_Amplitude", settings.amplitude);
        raymarchCompute.SetFloat("_Warp", settings.warp);
        raymarchCompute.SetFloat("_Add", settings.add);
        raymarchCompute.SetInt("_InvertNoise", settings.invertNoise ? 1 : 0);
        raymarchCompute.SetInt("_Seed", settings.seed);
        raymarchCompute.SetVector("_NoiseRes", new Vector4(128, 128, 128, 0));

        // 128 / 8
        raymarchCompute.Dispatch(generateNoisePass, 16, 16, 16);

        raymarchCompute.SetTexture(raymarchSmokePass, "_NoiseTex", noiseTex);
    }

    void InitializeNoise()
    {
        if (noiseTex != null)
        {
            UpdateNoise();
            return;
        }

        noiseDescriptor = new RenderTextureDescriptor(128, 128, RenderTextureFormat.RHalf, 0, Texture.GenerateAllMips, RenderTextureReadWrite.Linear);
        noiseDescriptor.enableRandomWrite = true;
        noiseDescriptor.dimension = TextureDimension.Tex3D;
        noiseDescriptor.volumeDepth = 128;

        noiseTex = RTHandles.Alloc(noiseDescriptor);

        UpdateNoise();
    }

    void InitializeVariables()
    {
        compositeMaterial = new Material(Shader.Find("Hidden/CompositeEffects"));
        raymarchCompute = (ComputeShader)Resources.Load("RenderSmoke");

        generateNoisePass = raymarchCompute.FindKernel("CS_GenerateNoise");
        debugNoisePass = raymarchCompute.FindKernel("CS_DebugNoise");
        raymarchSmokePass = raymarchCompute.FindKernel("CS_RayMarchSmoke");

        InitializeNoise();

        int width = Mathf.Max(4, Screen.width);
        int height = Mathf.Max(4, Screen.height);

        smokeAlbedoFullDescriptor = new RenderTextureDescriptor(width, height, RenderTextureFormat.ARGB64, 0, Texture.GenerateAllMips, RenderTextureReadWrite.Linear);
        smokeAlbedoFullDescriptor.enableRandomWrite = true;
        smokeAlbedoFullTex = RTHandles.Alloc(smokeAlbedoFullDescriptor);

        smokeAlbedoHalfDescriptor = new RenderTextureDescriptor(Mathf.CeilToInt(width / 2), Mathf.CeilToInt(height / 2), RenderTextureFormat.ARGB64, 0, Texture.GenerateAllMips, RenderTextureReadWrite.Linear);
        smokeAlbedoHalfDescriptor.enableRandomWrite = true;
        smokeAlbedoHalfTex = RTHandles.Alloc(smokeAlbedoHalfDescriptor);

        smokeAlbedoQuarterDescriptor = new RenderTextureDescriptor(Mathf.CeilToInt(width / 4), Mathf.CeilToInt(height / 4), RenderTextureFormat.ARGB64, 0, Texture.GenerateAllMips, RenderTextureReadWrite.Linear);
        smokeAlbedoQuarterDescriptor.enableRandomWrite = true;
        smokeAlbedoQuarterTex = RTHandles.Alloc(smokeAlbedoQuarterDescriptor);


        smokeMaskFullDescriptor = new RenderTextureDescriptor(width, height, RenderTextureFormat.RFloat, 0, Texture.GenerateAllMips, RenderTextureReadWrite.Linear);
        smokeMaskFullDescriptor.enableRandomWrite = true;
        smokeMaskFullTex = RTHandles.Alloc(smokeMaskFullDescriptor);

        smokeMaskHalfDescriptor = new RenderTextureDescriptor(Mathf.CeilToInt(width / 2), Mathf.CeilToInt(height / 2), RenderTextureFormat.RFloat, 0, Texture.GenerateAllMips, RenderTextureReadWrite.Linear);
        smokeMaskHalfDescriptor.enableRandomWrite = true;
        smokeMaskHalfTex = RTHandles.Alloc(smokeMaskHalfDescriptor);

        smokeMaskQuarterDescriptor = new RenderTextureDescriptor(Mathf.CeilToInt(width / 4), Mathf.CeilToInt(height / 4), RenderTextureFormat.RFloat, 0, Texture.GenerateAllMips, RenderTextureReadWrite.Linear);
        smokeMaskQuarterDescriptor.enableRandomWrite = true;
        smokeMaskQuarterTex = RTHandles.Alloc(smokeMaskQuarterDescriptor);


        depthDescriptor = new RenderTextureDescriptor(width, height, RenderTextureFormat.RFloat, 0, Texture.GenerateAllMips, RenderTextureReadWrite.Linear);
        depthDescriptor.enableRandomWrite = true;
        depthTex = RTHandles.Alloc(depthDescriptor);

        colorDescriptor = new RenderTextureDescriptor(width, height, RenderTextureFormat.RGB111110Float, 0, Texture.GenerateAllMips, RenderTextureReadWrite.Linear);
        colorDescriptor.enableRandomWrite = true;
        colorTex = RTHandles.Alloc(colorDescriptor);
    }

    private bool UpdateSettings()
    {
        if (settings.updateNoise)
        {
            UpdateNoise();
        }

        var voxelizer = GameObject.FindAnyObjectByType<Voxelizer>();
        if (voxelizer != null)
        {
            smokeVoxelBuffer = voxelizer.GetSmokeVoxelBuffer();
            if (smokeVoxelBuffer == null)
                return false;

            raymarchCompute.SetBuffer(2, "_SmokeVoxels", smokeVoxelBuffer);
            raymarchCompute.SetVector("_BoundsExtent", voxelizer.GetBoundsExtent());
            raymarchCompute.SetVector("_VoxelResolution", voxelizer.GetVoxelResolution());
            raymarchCompute.SetVector("_Radius", voxelizer.GetSmokeRadius());
            raymarchCompute.SetVector("_SmokeOrigin", voxelizer.GetSmokeOrigin());
        }

        if(compositeMaterial == null)
            compositeMaterial = new Material(Shader.Find("Hidden/CompositeEffects"));

        return true;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        //Get a CommandBuffer from pool.
        CommandBuffer cmd = CommandBufferPool.Get();

        using (new ProfilingScope(cmd, new ProfilingSampler("RayMarchedSmokePass")))
        {

            RTHandle cameraColorHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;
            RTHandle cameraDepthHandle = renderingData.cameraData.renderer.cameraDepthTargetHandle;

            if (!UpdateSettings())
                return;

            RTHandle smokeTex = GetSmokeAlbedoTex();
            RTHandle smokeMaskTex = GetSmokeMaskTex();
            Vector2Int renderResolution = GetRenderResolution();

            //Create depth tex for compute shader
            Blit(cmd, cameraDepthHandle, depthTex, compositeMaterial, 0);
            Blit(cmd, cameraColorHandle, colorTex);

            Matrix4x4 projMatrix = renderingData.cameraData.GetGPUProjectionMatrix();
            Matrix4x4 viewMatrix = renderingData.cameraData.GetViewMatrix();
            Matrix4x4 viewProjMatrix = projMatrix * viewMatrix;
            raymarchCompute.SetVector("_CameraForward", renderingData.cameraData.camera.transform.forward);


            raymarchCompute.SetVector("_CameraWorldPos", renderingData.cameraData.worldSpaceCameraPos);
            raymarchCompute.SetMatrix("_CameraToWorld", viewMatrix.inverse);
            raymarchCompute.SetMatrix("_CameraInvProjection", projMatrix.inverse);
            raymarchCompute.SetMatrix("_CameraInvViewProjection", viewProjMatrix.inverse);
            raymarchCompute.SetInt("_BufferWidth", renderResolution.x);
            raymarchCompute.SetInt("_BufferHeight", renderResolution.y);
            raymarchCompute.SetInt("_StepCount", settings.stepCount);
            raymarchCompute.SetInt("_LightStepCount", settings.lightStepCount);
            raymarchCompute.SetFloat("_SmokeSize", settings.smokeSize);
            raymarchCompute.SetFloat("_FrameTime", Time.time);
            raymarchCompute.SetFloat("_AbsorptionCoefficient", settings.absorptionCoefficient);
            raymarchCompute.SetFloat("_ScatteringCoefficient", settings.scatteringCoefficient);
            raymarchCompute.SetFloat("_DensityFalloff", 1 - settings.densityFalloff);
            raymarchCompute.SetFloat("_VolumeDensity", settings.volumeDensity * settings.stepSize);
            raymarchCompute.SetFloat("_StepSize", settings.stepSize);
            raymarchCompute.SetFloat("_ShadowDensity", settings.shadowDensity * settings.lightStepSize);
            raymarchCompute.SetFloat("_LightStepSize", settings.lightStepSize);
            raymarchCompute.SetFloat("_G", settings.scatteringAnisotropy);

            Assert.AreEqual((int)renderingData.lightData.visibleLights[renderingData.lightData.mainLightIndex].lightType, 1);
            raymarchCompute.SetVector("_SunDirection", renderingData.lightData.visibleLights[renderingData.lightData.mainLightIndex].light.transform.forward);

            raymarchCompute.SetVector("_AnimationDirection", settings.animationDirection);
            raymarchCompute.SetInt("_PhaseFunction", (int)settings.phaseFunction);
            raymarchCompute.SetVector("_CubeParams", settings.cubeParams);
            raymarchCompute.SetVector("_LightColor", settings.lightColor);
            raymarchCompute.SetVector("_SmokeColor", settings.smokeColor);
            raymarchCompute.SetVector("_ExtinctionColor", settings.extinctionColor);
            raymarchCompute.SetFloat("_AlphaThreshold", settings.alphaThreshold);

            if (settings.debugNoise)
            {
                raymarchCompute.SetTexture(debugNoisePass, "_NoiseTex", noiseTex);
                raymarchCompute.SetTexture(debugNoisePass, "_SmokeTex", smokeTex);
                raymarchCompute.SetInt("_DebugNoiseSlice", settings.debugNoiseSlice);
                raymarchCompute.SetInt("_DebugAxis", (int)settings.debugNoiseAxis);
                raymarchCompute.SetInt("_DebugTiledNoise", settings.debugTiledNoise ? 1 : 0);
                raymarchCompute.SetVector("_NoiseRes", new Vector4(128, 128, 128, 0));

                cmd.DispatchCompute(raymarchCompute, debugNoisePass, Mathf.CeilToInt(Screen.width / 8.0f), Mathf.CeilToInt(Screen.height / 8.0f), 1);

                Blit(cmd, smokeTex, cameraColorHandle);

                //Execute the command buffer and release it back to the pool.
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                CommandBufferPool.Release(cmd);
                return;
            }

            // Render volumes
            raymarchCompute.SetTexture(raymarchSmokePass, "_SmokeTex", smokeTex);
            raymarchCompute.SetTexture(raymarchSmokePass, "_SmokeMaskTex", smokeMaskTex);
            raymarchCompute.SetTexture(raymarchSmokePass, "_NoiseTex", noiseTex);
            raymarchCompute.SetTexture(raymarchSmokePass, "_DepthTex", depthTex);
            raymarchCompute.Dispatch(raymarchSmokePass, Mathf.CeilToInt(renderResolution.x / 8.0f), Mathf.CeilToInt(renderResolution.y / 8.0f), 1);

            if (settings.resolutionScale == RayMarchedSmokeSettings.Res.HalfResolution)
            {
                Blit(cmd, smokeMaskHalfTex, smokeMaskFullTex);
                Blit(cmd, smokeMaskFullTex, smokeMaskHalfTex);

                if (settings.bicubicUpscale)
                {
                    Blit(cmd, smokeAlbedoHalfTex, smokeAlbedoFullTex, compositeMaterial, 1);
                }
                else
                {
                    Blit(cmd, smokeAlbedoHalfTex, smokeAlbedoFullTex);
                }
            }

            if (settings.resolutionScale == RayMarchedSmokeSettings.Res.QuarterResolution)
            {
                Blit(cmd, smokeMaskQuarterTex, smokeMaskHalfTex);
                Blit(cmd, smokeMaskHalfTex, smokeMaskFullTex);
                Blit(cmd, smokeMaskFullTex, smokeMaskHalfTex);
                Blit(cmd, smokeMaskHalfTex, smokeMaskQuarterTex);

                if (settings.bicubicUpscale)
                {
                    Blit(cmd, smokeAlbedoQuarterTex, smokeAlbedoHalfTex, compositeMaterial, 1);
                    Blit(cmd, smokeAlbedoHalfTex, smokeAlbedoFullTex, compositeMaterial, 1);
                }
                else
                {
                    Blit(cmd, smokeAlbedoQuarterTex, smokeAlbedoHalfTex);
                    Blit(cmd, smokeAlbedoHalfTex, smokeAlbedoFullTex);
                }
            }

            // Composite volumes with source buffer
            compositeMaterial.SetTexture("_SmokeTex", smokeAlbedoFullTex);
            compositeMaterial.SetTexture("_SmokeMaskTex", smokeMaskTex);
            compositeMaterial.SetTexture("_DepthTex", depthTex);
            compositeMaterial.SetFloat("_Sharpness", settings.sharpness);
            compositeMaterial.SetFloat("_DebugView", (int)settings.debugView);

            Blit(cmd, colorTex, cameraColorHandle, compositeMaterial, 2);
        }

        //Execute the command buffer and release it back to the pool.
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();

        CommandBufferPool.Release(cmd);
    }

    public void Dispose()
    {
#if UNITY_EDITOR
        if (EditorApplication.isPlaying)
        {
            Object.Destroy(compositeMaterial);
        }
        else
        {
            Object.DestroyImmediate(compositeMaterial);
        }
#else
            Object.Destroy(compositeMaterial);
#endif

        if (smokeVoxelBuffer != null) smokeVoxelBuffer.Release();

        if (noiseTex != null) noiseTex.Release();

        if (depthTex != null) depthTex.Release();

        if (smokeAlbedoFullTex != null) smokeAlbedoFullTex.Release();

        if (smokeAlbedoHalfTex != null) smokeAlbedoHalfTex.Release();

        if (smokeAlbedoQuarterTex != null) smokeAlbedoQuarterTex.Release();

        if (smokeMaskFullTex != null) smokeMaskFullTex.Release();

        if (smokeMaskHalfTex != null) smokeMaskHalfTex.Release();

        if (smokeMaskQuarterTex != null) smokeMaskQuarterTex.Release();

    }
}
