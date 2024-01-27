using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Weapon : MonoBehaviour
{
    public void OnFire(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        Attack();
    }

    public virtual void Attack()
    {

    }

}
