using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrenadeLauncher : Weapon_Ranged
{
    [SerializeField] private Grenade grenadePrefab;
    [SerializeField] private float grenadeForce;
    [SerializeField] private Voxelizer voxelizer;

    [SerializeField] private float coolDown = 0.5f;
    private Grenade m_grenade;

    private float timer = 0.0f;
    private bool onCoolDown = false;
    [SerializeField, Tooltip("Maximum amount of bounces in grenade")] private int maxBounces;
    [HideInInspector] public int amountOfBounces { get; private set; }

    private void Start()
    {
        if (!IsOwner) return;

        voxelizer = FindObjectOfType<Voxelizer>();
    }

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
        if (!IsOwner) return;

        if (onCoolDown || currentAmmo <= 0 || isReloading) return;
        currentAmmo--;
        AttackServerRpc();


        //Grenade grenade = Instantiate(grenadePrefab, transform.position + transform.forward, Quaternion.identity);
        //grenade.Initialize(amountOfBounces, transform.forward, grenadeForce, voxelizer);
        onCoolDown = true;
    }

    [ServerRpc]//this spanws the grenade, not attacking the server
    private void AttackServerRpc()
    {
        m_grenade = Instantiate(grenadePrefab, transform.root.position + (Vector3.up * 2) + transform.root.forward, Quaternion.identity);
        m_grenade.bounces = amountOfBounces;
        m_grenade.parent = this;
        m_grenade.voxelizer = voxelizer;
        m_grenade.GetComponent<Rigidbody>().AddForce(transform.root.forward * grenadeForce);
        m_grenade.GetComponent<NetworkObject>().Spawn();
    }

    [ServerRpc(RequireOwnership = false)]//Destroys the grenade, not the server
    public void DestroyServerRpc()
    {
        m_grenade.GetComponent<NetworkObject>().Despawn();
        Destroy(m_grenade.gameObject);
    }
}
