using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GasThrower : Weapon_Ranged
{
    bool isEmitting = false;

    public override void Attack(bool isPressed)
    {
        isEmitting = isPressed;
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
