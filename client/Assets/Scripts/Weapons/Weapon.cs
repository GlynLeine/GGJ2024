using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public abstract class Weapon : MonoBehaviour
{
    public void OnFire(InputValue value)
    {
        Attack(value.isPressed);
    }

    public abstract void Attack(bool isPressed);
}
