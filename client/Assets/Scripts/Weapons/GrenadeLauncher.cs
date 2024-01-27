using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrenadeLauncher : Weapon_Ranged
{
    [SerializeField] private PlayerInput input;

    [SerializeField] private Grenade grenadePrefab;
    [SerializeField] private float grenadeForce;

    [SerializeField] private float coolDown = 0.5f;
    private float timer = 0.0f;
    private bool onCoolDown = false;
    [SerializeField,Tooltip("Maximum amount of bounces in grenade")] private int maxBounces;
    private int amountOfBounce = 0;

    private void Start()
    {
        if(input == null) throw new ArgumentNullException("input is null. Please fill it within the inspector");
    }

    private void Update()
    {
        if(onCoolDown)
        {
            timer += Time.deltaTime;
            if(timer >= coolDown)
            {
                timer = 0.0f;
                onCoolDown = false;
            }
        }
    }

    public void OnAlterBounce(InputAction.CallbackContext context)
    {
      
        float value = context.ReadValue<float>();
        amountOfBounce = Math.Clamp(amountOfBounce + (value > 0 ? 1 : value < 0 ? -1 : 0), 0, maxBounces);
        Debug.Log("Amount of bounces: " + amountOfBounce);
    }
    public override void Attack()
    {
        if (onCoolDown) return;

       Grenade grenade = Instantiate(grenadePrefab, transform.position + transform.forward, Quaternion.identity);
       grenade.Initialize(amountOfBounce,transform.forward,grenadeForce);
        onCoolDown = true;
    }


}
