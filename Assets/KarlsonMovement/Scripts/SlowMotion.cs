using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.Universal;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public class SlowMotion : MonoBehaviour
{

    

    //post
    ChromaticAberration chrom;
    ColorAdjustments color;

    //var
    bool canDeslow;
    bool canPost;
    bool isProcessing;
    public float time = 500;
    public float fovFinal = 120;

    //assignable
    public Camera cam;
    public PlayerMovement PlayerMovement;
    public Volume vol;


    void Start()
    {
        vol.profile.TryGet<ColorAdjustments>(out color);
        vol.profile.TryGet<ChromaticAberration>(out chrom);

       
    }

  
    // Update is called once per frame
    void Update()
    {
        if(!PlayerMovement.grounded && Input.GetKey(KeyCode.V) && !PlayerMovement.grounded)
        {
            WallRun.canFov = false;
            Time.timeScale = Mathf.Lerp(1, 0.2f, 5);


         
            canPost = false;

            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fovFinal, 10 * Time.deltaTime);
            chrom.intensity.value = Mathf.Lerp(chrom.intensity.value, 1, 10 * Time.deltaTime);
            color.postExposure.value = Mathf.Lerp(color.postExposure.value, -1.2f, 10 * Time.deltaTime);

            canDeslow = true;
            isProcessing = true;
            
            
        }
        if(PlayerMovement.grounded && canDeslow || Input.GetKeyUp(KeyCode.V))
        {
            canPost = true;
            Time.timeScale = Mathf.Lerp(0.2f, 1, 5);


           
            isProcessing = false;
            canDeslow = false;
        }

        

        if (canPost)
        {
            
            WallRun.canFov = true;
            chrom.intensity.value = Mathf.Lerp(chrom.intensity.value, 0.154f, 5);
            color.postExposure.value = Mathf.Lerp(color.postExposure.value, 0, 5 * Time.deltaTime);
        }


        
    }
}
