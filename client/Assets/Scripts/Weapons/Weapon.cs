using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class Weapon : MonoBehaviour
{
    public void OnFire()
    {
        Attack();
    }

    public abstract void Attack();
}
