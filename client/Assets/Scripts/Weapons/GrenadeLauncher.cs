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
    [SerializeField] private Voxelizer voxelizer;

    [SerializeField] private float coolDown = 0.5f;
    private float timer = 0.0f;
    private bool onCoolDown = false;
    [SerializeField, Tooltip("Maximum amount of bounces in grenade")] private int maxBounces;
    [HideInInspector] public int amountOfBounces { get; private set; }

    private void Update()
    {
        if (onCoolDown)
        {
            timer += Time.deltaTime;
            if (timer >= coolDown)
            {
                timer = 0.0f;
                onCoolDown = false;
            }
        }
    }

    public void OnAlterBounce(InputValue value)
    {
        float axis = value.Get<float>();
        amountOfBounces = Math.Clamp(amountOfBounces + (axis > 0 ? 1 : axis < 0 ? -1 : 0), 0, maxBounces);
        Debug.Log("Amount of bounces: " + amountOfBounces + " input: " + axis);
    }

    public override void Attack()
    {
        if (onCoolDown || currentAmmo <= 0 || isReloading) return;

        currentAmmo--;

        Grenade grenade = Instantiate(grenadePrefab, transform.position + transform.forward, Quaternion.identity);
        grenade.Initialize(amountOfBounces, transform.forward, grenadeForce, voxelizer);
        onCoolDown = true;
    }


}
