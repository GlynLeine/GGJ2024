using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

public class GasThrower : Weapon_Ranged
{
    bool isEmitting = false;
    [SerializeField] private float smokePuffsPerSecond = 5;
    [SerializeField] private VisualEffect effect;
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

        }
    }
}
