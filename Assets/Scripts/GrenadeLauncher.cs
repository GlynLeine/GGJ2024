using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrenadeLauncher : MonoBehaviour
{
    [SerializeField] private PlayerInput input;

    [SerializeField,Tooltip("Maximum amount of bounces in grenade")] private int maxBounces;
    private int amountOfBounce = 0;

    private void Start()
    {
        if(input == null) throw new ArgumentNullException("input is null. Please fill it within the inspector");

       
    }

    public void OnAlterBounce(InputAction.CallbackContext context)
    {
        Debug.Log("ScrollWheel detected " + context.ReadValue<float>());
        float value = context.ReadValue<float>();
        amountOfBounce = Math.Clamp(amountOfBounce + (value > 0 ? 1 : value < 0 ? -1 : 0), 0, maxBounces);
    }
    

}
