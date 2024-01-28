using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

public class GasThrower : Weapon_Ranged
{
    bool isEmitting = false;

    [SerializeField] private bool makeParent = false;

    [SerializeField] private GasTriggerSphere triggerSpherePrefab;

    [SerializeField] private float smokePuffsPerSecond = 5;
    [SerializeField] private VisualEffect effect;

    [SerializeField] private float minimumSize;
    [SerializeField] private float maximumSize;

    [SerializeField] private float minimumSpeed;
    [SerializeField] private float maximumSpeed;

    [SerializeField] private float minLifeSpan;
    [SerializeField] private float maximumLifeSpan;
    private float secondsPerPuff => 1 / smokePuffsPerSecond;
    private float timer = 0;
    public override void Attack(bool isPressed)
    {
        isEmitting = isPressed;
        if(isEmitting) effect.Play();
        else effect.Stop();
        //Emit particles/VFX
        
    }
    private void Update()
    {
        if(isEmitting)
        {
           Debug.Log("psssssssssssssssssh");
           timer += Time.deltaTime;
           if(timer > secondsPerPuff)
           {
             GasTriggerSphere sphere = Instantiate(triggerSpherePrefab, transform.position, Quaternion.identity, makeParent ? transform : null);
                sphere.Initialize(Random.RandomRange(minLifeSpan, maximumLifeSpan), transform.forward, Random.RandomRange(minimumSpeed, maximumSpeed), Random.RandomRange(minimumSize, maximumSize));
                timer = 0;
           }
        }
    }
}
