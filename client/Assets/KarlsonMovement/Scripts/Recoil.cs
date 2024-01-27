using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Recoil : MonoBehaviour
{
    private Vector3 currentRotation; 
    private Vector3 targetRotation;
    //Hipfire Recoil
    [SerializeField] private float recoilX; 
    [SerializeField] private float recoily;
    [SerializeField] private float recoilz;
    //Settings
    [SerializeField] private float snappiness;
    [SerializeField] private float returnSpeed;
   // public Animator anim;

  //  public int currentWeapon;
  
    void Start()
    {

    }

     void Update()
    {
        
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Lerp(currentRotation, targetRotation, snappiness * Time.fixedDeltaTime); 
        transform.localRotation = Quaternion.Euler(currentRotation);

       if(currentRotation == targetRotation)
        {
           // anim.enabled = true;
        }
    }

     public void Recoilfire()
    {
       // anim.enabled = false;

        
       
            targetRotation += new Vector3(recoilX, Random.Range(recoily, recoily), Random.Range(recoilz, recoilz));
        
        
    }

}
