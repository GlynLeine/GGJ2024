using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Weapon_Ranged : Weapon
{
    [SerializeField] private int maxAmmo;
    private int currentAmmo;
    public void OnReload (InputAction.CallbackContext context)
    {
        //Add possible Delay 
        currentAmmo = maxAmmo;
    }

}
