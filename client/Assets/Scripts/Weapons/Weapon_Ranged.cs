using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

public abstract class Weapon_Ranged : Weapon
{
    public int maxAmmo;
    public float reloadTime;
    
    [HideInInspector] public int currentAmmo;

    public bool isReloading {  get; private set; }

    public void OnReload ()
    {
        Reload();
    }

    public virtual void Reload()
    {
        Debug.Log("Reloading");
        isReloading = true;
        Invoke(nameof(ReloadImpl), reloadTime);
    }

    private void ReloadImpl()
    {
        isReloading = false;
        currentAmmo = maxAmmo;
        Debug.Log("Reloaded");
    }
}
